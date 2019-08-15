using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationRowsSystem : JobComponentSystem
{
    EntityQuery gridQuery;
    EntityQuery gameStateQuery;
    EntityQuery gameRulesQuery;

    /// <summary>
    /// Find all the matches that are column-wise.
    /// </summary>
    public struct FindMatchesInRow : IJobParallelFor
    {
        [ReadOnly] public int rowCount;
        [ReadOnly] public int colCount;
        [ReadOnly] public int matchSize;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<OwnerComponent> board;

        [WriteOnly] public NativeQueue<MatchComponent>.ParallelWriter matchesFound;

        public void Execute(int row)
        {
            int firstInRowIndex = row * colCount;
            int lastInRowIndex = firstInRowIndex + colCount - 1;

            int currentIndex = firstInRowIndex + 1;
            int prevIndex = firstInRowIndex;

            int matchStartIndex = firstInRowIndex;
            int count = 1;

            Team currentTeam = default;
            Team previousTeam = default;

            while (currentIndex <= lastInRowIndex)
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
                ++currentIndex;
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
                matchType = MatchComponent.MatchType.HORIZONTAL
            });
        }
    }

    public struct CreateMatchEntities : IJob
    {
        public NativeQueue<MatchComponent> matches;
        public EntityCommandBuffer commandBuffer;

        public void Execute()
        {
            while (matches.Count > 0)
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

        JobHandle jobHandle = new FindMatchesInRow()
        {
            board = ownerGrid,
            colCount = gridDimensions.columnCount,
            rowCount = gridDimensions.rowCount,
            matchSize = matchSize.minMatchSize,

            matchesFound = matchesQueue.AsParallelWriter()
        }.Schedule(gridDimensions.rowCount, 1);

        jobHandle = new CreateMatchEntities()
        {
            matches = matchesQueue,
            commandBuffer = commandBufferSystem.CreateCommandBuffer()
        }.Schedule(jobHandle);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        matchesQueue.Dispose(jobHandle);

        return inputDeps;
    }
}
