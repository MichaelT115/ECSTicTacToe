using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// Checks if there are any remain moves on the board and adds the NoMovesLeft component,.
/// </summary>
[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationCheckForRemainingMoves : JobComponentSystem
{
    EntityQuery ownerQuery;
    EntityQuery gameStateQuery;

    EndBoardEvaluationCommandBufferSystem commandBufferSystem;

    struct CheckForUnownedCellsJob : IJob
    {
        [ReadOnly] public Entity gameStateEntity;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<OwnerComponent> cellOwners;
        [WriteOnly] public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute()
        {
            foreach (var owner in cellOwners)
            {
                if (owner.team == Team.EMPTY)
                {
                    return;
                }
            }

            commandBuffer.AddComponent<NoMovesLeft>(0, gameStateEntity);
        }
    }

    protected override void OnCreate()
    {
        ownerQuery = GetEntityQuery(typeof(OwnerComponent));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));

        commandBufferSystem = World.GetOrCreateSystem<EndBoardEvaluationCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new CheckForUnownedCellsJob()
        {
            cellOwners = ownerQuery.ToComponentDataArray<OwnerComponent>(Allocator.TempJob),
            gameStateEntity = gameStateQuery.GetSingletonEntity(),
            commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule();

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
