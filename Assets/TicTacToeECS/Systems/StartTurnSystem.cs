﻿using Unity.Entities;

/// <summary>
/// Starts a turn for the current player.
/// </summary>
[DisableAutoCreation]
public class StartTurnSystem : ComponentSystem
{
    EntityQuery turnControllerQuery;

    protected override void OnCreate()
    {
        turnControllerQuery = GetEntityQuery(typeof(CurrentPlayerIndexComponent), typeof(PlayerListElement));
    }

    protected override void OnUpdate()
    {
        Entity turnControllerEntity = turnControllerQuery.GetSingletonEntity();

        // Get Current Player
        DynamicBuffer<PlayerListElement> playerList = EntityManager.GetBuffer<PlayerListElement>(turnControllerEntity);
        int currentPlayerIndex = EntityManager.GetComponentData<CurrentPlayerIndexComponent>(turnControllerEntity).index;
        Entity currentPlayer = playerList[currentPlayerIndex];

        // Give current player turn component
        EntityManager.AddComponentData(currentPlayer, new HasTurnComponent());
    }
}
