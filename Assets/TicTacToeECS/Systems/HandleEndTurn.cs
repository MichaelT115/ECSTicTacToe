using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class HandleEndTurn : ComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(CompletedTurnComponent));
        RequireForUpdate(playerQuery);
    }

    protected override void OnUpdate()
    {
        // Clear Player Turn
        NativeArray<Entity> playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
        foreach (Entity playerEntity in playerEntities)
        {
            EntityManager.RemoveComponent(playerEntity, typeof(HasTurnComponent));
            EntityManager.RemoveComponent(playerEntity, typeof(CompletedTurnComponent));
            EntityManager.RemoveComponent(playerEntity, typeof(PlayerSelection));
            EntityManager.RemoveComponent(playerEntity, typeof(MadeSelectionComponent));
        }
        playerEntities.Dispose();

        // Evaluate Board
        World.GetOrCreateSystem<BoardEvaluationUpdateGroup>().Update();

        // Game Ending Conditions
        World.GetOrCreateSystem<GameEvaluationUpdateGroup>().Update();

        // If Game Win
        Entity gameStateEntity = gameStateQuery.GetSingletonEntity();
        if (EntityManager.HasComponent<WinnerComponent>(gameStateEntity))
        {
            Team winner = EntityManager.GetComponentData<WinnerComponent>(gameStateEntity).winner;
            Debug.Log($"Winner Found: {winner}");
            Debug.Break();
            return;
        }

        // If Game Draw
        if (EntityManager.HasComponent<DrawComponent>(gameStateEntity))
        {
            Debug.Log("Draw");
            Debug.Break();
            return;
        }

        // Advance Turn
        StartNextTurn();
        Debug.Break();
    }

    private void StartNextTurn()
    {
        Debug.Log("Advance Turn");

        // Set Next Player
        World.GetOrCreateSystem<AdvancePlayerSystem>().Update();

        // Start New Turn
        World.GetOrCreateSystem<StartTurnSystem>().Update();
    }
}
