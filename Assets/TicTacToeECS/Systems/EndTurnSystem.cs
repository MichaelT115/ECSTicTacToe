using Unity.Entities;

public class EndTurnSystem : ComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(HasTurnComponent), typeof(CompletedTurnComponent));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
    }

    protected override void OnUpdate()
    {
        Entity playerEntity = playerQuery.GetSingletonEntity();

        EntityManager.RemoveComponent<HasTurnComponent>(playerEntity);
        EntityManager.RemoveComponent<PlayerSelection>(playerEntity);
        EntityManager.RemoveComponent<CompletedTurnComponent>(playerEntity);
        EntityManager.RemoveComponent<MadeSelectionComponent>(playerEntity);

        EntityManager.AddComponent(gameStateQuery.GetSingletonEntity(), typeof(TurnCompletedComponent));
    }
}

struct TurnCompletedComponent : IComponentData { }

public class HandleEndTurn : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Evaluate Board
        var boardEvaluationUpdateGroup = World.GetOrCreateSystem<BoardEvaluationUpdateGroup>();
        boardEvaluationUpdateGroup.Update();

        // Game Ending Conditions

        // If Game Win

        // If Game Draw

        // Set Next Player

        // Start New Turn
        World.GetOrCreateSystem<StartTurnSystem>().Update();
    }
}

/// <summary>
/// Update Player.
/// </summary>
public class NextPlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {

    }
}