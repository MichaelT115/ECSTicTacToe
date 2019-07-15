using Unity.Collections;
using Unity.Entities;

public class SelectionValidationSystem : ComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery gridQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(PlayerSelection));
        gridQuery = GetEntityQuery(typeof(GridCellData));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);

        Entity gridEntity = gridQuery.GetSingletonEntity();
        var gridBuffer = entityManager.GetBuffer<GridCellData>(gridEntity).ToNativeArray(Allocator.Temp);


        foreach (Entity playerEntity in playerEntities)
        {
            Team playerTeam = entityManager.GetComponentData<PlayerTeamComponent>(playerEntity).team;
            int selectionIndex = entityManager.GetComponentData<PlayerSelection>(playerEntity);
            entityManager.RemoveComponent<PlayerSelection>(playerEntity);

            Entity tileEntity = gridBuffer[selectionIndex].entity;
            OwnerComponent cellOwner = entityManager.GetComponentData<OwnerComponent>(tileEntity);

            if (cellOwner.team == Team.EMPTY)
            {
                entityManager.SetComponentData(tileEntity, new OwnerComponent() { team = playerTeam });
                entityManager.AddComponentData(playerEntity, new MadeSelectionComponent());
            }
        }

        playerEntities.Dispose();
        gridBuffer.Dispose();
    }
}