using Unity.Entities;

public class EndTurnSystem : ComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery gridQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent));
        gridQuery = GetEntityQuery(typeof(GridCellData));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        Entity gameStateEntity = GetSingletonEntity<GameStateComponent>();

        if (!entityManager.HasComponent(gameStateEntity, typeof(MoveMadeComponent)))
            return;

        entityManager.RemoveComponent<MoveMadeComponent>(gameStateEntity);

        Entity playerEntity = playerQuery.GetSingletonEntity();
        entityManager.AddComponent(playerEntity, typeof(HasTurnComponent));
    }
}
