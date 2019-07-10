using Unity.Entities;

public class GameStateSystem : ComponentSystem
{
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        Entity gameStateEntity = gameStateQuery.GetSingletonEntity();

        if (entityManager.HasComponent<TurnSelectionState>(gameStateEntity))
        {
            StartTurnSystem setTurnSystem = World.Active.GetOrCreateSystem<StartTurnSystem>();
            setTurnSystem.Update();
        }
    }
}
