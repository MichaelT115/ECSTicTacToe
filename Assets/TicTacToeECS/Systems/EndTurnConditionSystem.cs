using Unity.Collections;
using Unity.Entities;
using UnityEngine;
// Checks if the player has reached an end turn condition.
// A turn is over if a validated selection was made.
public class EndTurnConditionSystem : ComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(HasTurnComponent), typeof(MadeSelectionComponent));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));

        RequireForUpdate(playerQuery);
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> players = playerQuery.ToEntityArray(Allocator.TempJob);

        Debug.Log("Turn Ends");

        foreach (Entity playerEntity in players)
        {
            EntityManager.RemoveComponent<HasTurnComponent>(playerEntity);
            EntityManager.RemoveComponent<MadeSelectionComponent>(playerEntity);
            EntityManager.AddComponentData(playerEntity, new CompletedTurnComponent());
        }
    }
}

/// <summary>
/// 
/// </summary>
public struct CompletedTurnComponent : IComponentData
{

}