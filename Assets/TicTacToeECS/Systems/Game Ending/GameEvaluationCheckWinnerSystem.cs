using Unity.Entities;

[UpdateInGroup(typeof(GameEvaluationUpdateGroup))]
public class GameEvaluationCheckWinnerSystem : ComponentSystem
{
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent), typeof(MatchFound));

        RequireForUpdate(gameStateQuery);
    }

    protected override void OnUpdate()
    {
        Entity gameStateEntity = gameStateQuery.GetSingletonEntity();
        MatchFound matchFound = EntityManager.GetComponentData<MatchFound>(gameStateEntity);

        EntityManager.AddComponentData(gameStateEntity, new WinnerComponent() { winner = matchFound.team });
    }
}
