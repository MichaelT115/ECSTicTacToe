using Unity.Collections;
using Unity.Entities;

public class SelectionTestSystem : ComponentSystem
{
    EntityQuery query;
    EntityQuery gridQuery;

    protected override void OnCreate()
    {
        query = GetEntityQuery(typeof(PlayerTeamComponent), typeof(PlayerSelection));
        gridQuery = GetEntityQuery(typeof(GridCellData));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        var playerEntities = query.ToEntityArray(Allocator.Temp);

        Entity gridEntity = query.GetSingletonEntity();
        var gridBuffer = entityManager.GetBuffer<GridCellData>(gridEntity);

        foreach (Entity entity in playerEntities)
        {
            Team playerTeam = entityManager.GetComponentData<PlayerTeamComponent>(entity).team;
            int selectionIndex = entityManager.GetComponentData<PlayerSelection>(entity);
            entityManager.RemoveComponent<PlayerSelection>(entity);

            OwnerComponent cellOwner = entityManager.GetComponentData<OwnerComponent>(gridBuffer[selectionIndex].entity);

            if (cellOwner.team == Team.EMPTY)
            {
                cellOwner.team = playerTeam;
                // End Turn
            }
        }
    }
}