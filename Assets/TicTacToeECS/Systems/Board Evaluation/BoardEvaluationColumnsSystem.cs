using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// 
/// </summary>
[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationColumnsSystem : JobComponentSystem
{
    EntityQuery gridQuery;
    EntityQuery gameStateQuery;
    EntityQuery gameRulesQuery;

    /// <summary>
    /// Find all the matches that are column-wise.
    /// </summary>
    public struct FindMatchesInColumn : IJobParallelFor
    {
        [ReadOnly] public int rowCount;
        [ReadOnly] public int colCount;
        [ReadOnly] public int matchSize;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<OwnerComponent> board;

        [WriteOnly] public NativeQueue<MatchComponent>.ParallelWriter matchesFound;

        public void Execute(int column)
        {
            int firstInColumnIndex = column;
            int lastInColumnIndex = firstInColumnIndex + (rowCount - 1) * colCount;

            int currentIndex = firstInColumnIndex + colCount;
            int prevIndex = firstInColumnIndex;

            int matchStartIndex = firstInColumnIndex;
            int count = 1;

            Team currentTeam = default;
            Team previousTeam = default;

            while (currentIndex <= lastInColumnIndex)
            {
                currentTeam = board[currentIndex];
                previousTeam = board[matchStartIndex];

                // Is the match sequence broken.
                if (currentTeam != previousTeam)
                {
                    if (previousTeam != Team.EMPTY && count >= matchSize)
                    {
                        AddMatch(previousTeam, matchStartIndex, prevIndex);
                    }

                    matchStartIndex = currentIndex;
                    previousTeam = currentTeam;
                    count = 0;
                }


                prevIndex = currentIndex;
                currentIndex += colCount;
                ++count;
            }

            if (previousTeam != Team.EMPTY && count >= matchSize)
            {
                AddMatch(previousTeam, matchStartIndex, prevIndex);
            }
        }

        private void AddMatch(Team team, int startIndex, int prevIndex)
        {
            matchesFound.Enqueue(new MatchComponent()
            {
                team = team,
                startIndex = startIndex,
                endIndex = prevIndex,
                matchType = MatchComponent.MatchType.VERTICAL
            });
        }
    }

    public struct CreateMatchEntities : IJob
    {
        public NativeQueue<MatchComponent> matches;
        public EntityCommandBuffer commandBuffer;

        public void Execute()
        {
            while(matches.Count > 0)
            {
                Entity entity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(entity, matches.Dequeue());
            }
        }
    }

    protected override void OnCreate()
    {
        gridQuery = GetEntityQuery(typeof(GridCellData));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
        gameRulesQuery = GetEntityQuery(typeof(MinMatchSizeRuleComponent));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entity gridEntity = gridQuery.GetSingletonEntity();
        GridDimensionsComponent gridDimensions = EntityManager.GetComponentData<GridDimensionsComponent>(gridEntity);

        EndBoardEvaluationCommandBufferSystem commandBufferSystem = World.GetOrCreateSystem<EndBoardEvaluationCommandBufferSystem>();

        MinMatchSizeRuleComponent matchSize = gameRulesQuery.GetSingleton<MinMatchSizeRuleComponent>();

        DynamicBuffer<GridCellData> buffer = EntityManager.GetBuffer<GridCellData>(gridEntity);
        NativeArray<OwnerComponent> ownerGrid = new NativeArray<OwnerComponent>(buffer.Length, Allocator.TempJob);

        for (int i = 0; i < ownerGrid.Length; ++i)
        {
            ownerGrid[i] = EntityManager.GetComponentData<OwnerComponent>(buffer[i].entity);
        }

        NativeQueue<MatchComponent> matchesQueue = new NativeQueue<MatchComponent>(Allocator.TempJob);

        JobHandle jobHandle = new FindMatchesInColumn()
        {
            board = ownerGrid,
            colCount = gridDimensions.columnCount,
            rowCount = gridDimensions.rowCount,
            matchSize = matchSize.minMatchSize,

            matchesFound = matchesQueue.AsParallelWriter()
        }.Schedule(gridDimensions.columnCount, 1);

        jobHandle = new CreateMatchEntities() {
            matches = matchesQueue,
            commandBuffer = commandBufferSystem.CreateCommandBuffer()
        }.Schedule(jobHandle);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        matchesQueue.Dispose(jobHandle);

        return inputDeps;
    }
}
