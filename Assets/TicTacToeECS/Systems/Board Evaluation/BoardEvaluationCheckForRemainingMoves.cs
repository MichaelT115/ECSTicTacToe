using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationCheckForRemainingMoves : JobComponentSystem
{
    EntityQuery ownerQuery;
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        ownerQuery = GetEntityQuery(typeof(OwnerComponent));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var owners = ownerQuery.ToComponentDataArray<OwnerComponent>(Allocator.TempJob);
        foreach (var owner in owners)
        {
            if (owner.team == Team.EMPTY)
            {
                owners.Dispose();
                return inputDeps;
            }
        }
        owners.Dispose();

        EntityManager.AddComponentData(gameStateQuery.GetSingletonEntity(), new NoMovesLeft());

        return inputDeps;
    }
}
