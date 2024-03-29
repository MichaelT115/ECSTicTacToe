﻿using Unity.Collections;
using Unity.Entities;

// Checks if the player has reached an end turn condition.
// A turn is over if a validated selection was made.
public class EndTurnConditionSystem : ComponentSystem
{
    EntityQuery playerQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(PlayerTeamComponent), typeof(HasTurnComponent), typeof(MadeSelectionComponent) },
            None = new ComponentType[] { typeof(CompletedTurnComponent) }
        });
        RequireForUpdate(playerQuery);
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> players = playerQuery.ToEntityArray(Allocator.TempJob);
        foreach (Entity playerEntity in players)
        {
            EntityManager.AddComponentData(playerEntity, new CompletedTurnComponent());
        }
        players.Dispose();
    }
}
