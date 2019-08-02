using Unity.Entities;

[DisableAutoCreation]
public class StartBoardEvaluationCleanExistingMatchesSystem : ComponentSystem
{
    EntityQuery matchesQuery;

    protected override void OnCreate()
    {
        matchesQuery = GetEntityQuery(typeof(MatchComponent));
    }

    protected override void OnUpdate()
    {
        EntityManager.DestroyEntity(matchesQuery);
    }
}
