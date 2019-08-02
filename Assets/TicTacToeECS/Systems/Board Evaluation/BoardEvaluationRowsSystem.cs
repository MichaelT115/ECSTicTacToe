using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationRowsSystem : JobComponentSystem
{
    EntityQuery gridQuery;
    EntityQuery gameStateQuery;

    private struct FindRowMatchesJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<OwnerComponent> componentDataFromEntity;
        [ReadOnly] public GridDimensionsComponent gridDimensions;
        [ReadOnly] public int sequenceLength;
        [ReadOnly] public DynamicBuffer<GridCellData> gridBuffer;

        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(int row)
        {
            int width = gridDimensions.columnCount;
            int height = gridDimensions.rowCount;


            int firstInRowIndex = row * width;
            int lastInRowIndex = firstInRowIndex + width - 1;

            int prevIndex = firstInRowIndex;
            int currentIndex = firstInRowIndex + 1;
            int matchStartIndex = firstInRowIndex;
            int count = 1;

            Team currentTeam,
                previousTeam = componentDataFromEntity[gridBuffer[firstInRowIndex].entity].team;

            while (currentIndex <= lastInRowIndex)
            {
                currentTeam = componentDataFromEntity[gridBuffer[currentIndex].entity].team;

                if (currentTeam != previousTeam)
                {
                    if (previousTeam != Team.EMPTY && count >= sequenceLength)
                    {
                        Entity entity = commandBuffer.CreateEntity(row);
                        commandBuffer.AddComponent(row, entity, new MatchComponent()
                        {
                            startIndex = matchStartIndex,
                            endIndex = prevIndex,
                            matchType = MatchComponent.MatchType.HORIZONTAL
                        });
                    }

                    matchStartIndex = currentIndex;
                    previousTeam = currentTeam;
                    count = 0;
                }

                prevIndex = currentIndex;
                ++currentIndex;
                ++count;
            }

            if (previousTeam != Team.EMPTY && count >= sequenceLength)
            {
                Entity entity = commandBuffer.CreateEntity(row);
                commandBuffer.AddComponent(row, entity, new MatchComponent()
                {
                    startIndex = matchStartIndex,
                    endIndex = prevIndex,
                    matchType = MatchComponent.MatchType.HORIZONTAL
                });
            }
        }
    }

    protected override void OnCreate()
    {
        gridQuery = GetEntityQuery(typeof(GridCellData));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entity gridEntity = gridQuery.GetSingletonEntity();
        GridDimensionsComponent gridDimensions = EntityManager.GetComponentData<GridDimensionsComponent>(gridEntity);

        EndBoardEvaluationCommandBufferSystem commandBufferSystem = World.GetOrCreateSystem<EndBoardEvaluationCommandBufferSystem>();

        JobHandle jobHandle =  new FindRowMatchesJob()
        {
            componentDataFromEntity = GetComponentDataFromEntity<OwnerComponent>(true),

            gridDimensions = gridDimensions,

            gridBuffer = EntityManager.GetBuffer<GridCellData>(gridEntity),
            sequenceLength = 1,

            commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(gridDimensions.rowCount, 1);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
