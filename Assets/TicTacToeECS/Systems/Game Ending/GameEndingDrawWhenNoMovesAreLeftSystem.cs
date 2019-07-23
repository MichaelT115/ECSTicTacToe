using Unity.Entities;

[UpdateInGroup(typeof(GameEvaluationUpdateGroup))]
public class GameEndingDrawWhenNoMovesAreLeftSystem : ComponentSystem
{
    EntityQuery gameStateQuery;

    protected override void OnCreate()
    {
        gameStateQuery = GetEntityQuery(new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(MatchFound) },
            All = new ComponentType[] { typeof(GameStateComponent), typeof(NoMovesLeft) }
        });

        RequireForUpdate(gameStateQuery);
    }

    protected override void OnUpdate()
    {
        Entity gameStateEntity = gameStateQuery.GetSingletonEntity();
        EntityManager.AddComponentData(gameStateEntity, new DrawComponent());
    }
}
