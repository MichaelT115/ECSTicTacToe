using Unity.Entities;

/// <summary>
/// Starts a turn for the current player.
/// </summary>
[DisableAutoCreation]
public class StartTurnSystem : ComponentSystem
{
    EntityQuery turnControllerQuery;
    EntityQuery playerQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent));
        turnControllerQuery = GetEntityQuery(typeof(CurrentPlayerIndexComponent), typeof(PlayerListElement));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        Entity turnControllerEntity = turnControllerQuery.GetSingletonEntity();

        // Get Current Player
        DynamicBuffer<PlayerListElement> playerList = entityManager.GetBuffer<PlayerListElement>(turnControllerEntity);
        int currentPlayerIndex = entityManager.GetComponentData<CurrentPlayerIndexComponent>(turnControllerEntity).index;
        Entity currentPlayer = playerList[currentPlayerIndex];

        // Give current player turn component
        entityManager.AddComponentData(currentPlayer, new HasTurnComponent());
    }
}
