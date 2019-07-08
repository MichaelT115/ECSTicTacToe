using Unity.Entities;

[DisableAutoCreation]
public class SetTurnSystem : ComponentSystem
{
    EntityQuery playerQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        Entity playerEntity = playerQuery.GetSingletonEntity();
        entityManager.AddComponent(playerEntity, typeof(HasTurnComponent));
    }
}
