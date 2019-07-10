using Unity.Entities;

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
        var playerList = entityManager.GetBuffer<PlayerListElement>(turnControllerEntity);
        int currentPlayerIndex = entityManager.GetComponentData<CurrentPlayerIndexComponent>(turnControllerEntity).index;
        Entity currentPlayer = playerList[currentPlayerIndex];

        // Give current player turn component
        entityManager.AddComponentData(currentPlayer, new HasTurnComponent());

    }
}
