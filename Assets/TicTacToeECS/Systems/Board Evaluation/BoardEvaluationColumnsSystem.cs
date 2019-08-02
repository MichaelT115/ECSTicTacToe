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
    
    private struct FindMatchesInColumn : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<OwnerComponent> componentDataFromEntity;
        [ReadOnly] public GridDimensionsComponent gridDimensions;
        [ReadOnly] public int sequenceLength;
        [ReadOnly] public DynamicBuffer<GridCellData> gridBuffer;

        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(int column)
        {
            int width = gridDimensions.columnCount;
            int height = gridDimensions.rowCount;

            int lastInColumnIndex = column + (height - 1) * width,
                currentIndex = column + 1,
                prevIndex = column,
                matchStartIndex = column,
                count = 1;

            Team currentTeam,
                previousTeam = componentDataFromEntity[gridBuffer[column].entity].team;

            while (currentIndex <= lastInColumnIndex)
            {
                currentTeam = componentDataFromEntity[gridBuffer[currentIndex].entity].team;

                if (currentTeam != previousTeam)
                {
                    if (previousTeam != Team.EMPTY && count >= sequenceLength)
                    {
                        Entity entity = commandBuffer.CreateEntity(column);
                        commandBuffer.AddComponent(column, entity, new MatchComponent()
                        {
                            startIndex = matchStartIndex,
                            endIndex = prevIndex,
                            matchType = MatchComponent.MatchType.VERTICAL
                        });
                    }

                    matchStartIndex = currentIndex;
                    previousTeam = currentTeam;
                    count = 0;
                }


                prevIndex = currentIndex;
                currentIndex += width;
                ++count;
            }

            if (previousTeam != Team.EMPTY && count >= sequenceLength)
            {
                Entity entity = commandBuffer.CreateEntity(column);
                commandBuffer.AddComponent(column, entity, new MatchComponent()
                {
                    startIndex = matchStartIndex,
                    endIndex = prevIndex,
                    matchType = MatchComponent.MatchType.VERTICAL                  
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

        JobHandle jobHandle = new FindMatchesInColumn()
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