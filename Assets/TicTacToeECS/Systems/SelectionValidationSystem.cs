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
        var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);

        Entity gridEntity = gridQuery.GetSingletonEntity();
        var gridBuffer = EntityManager.GetBuffer<GridCellData>(gridEntity).ToNativeArray(Allocator.Temp);


        foreach (Entity playerEntity in playerEntities)
        {
            Team playerTeam = EntityManager.GetComponentData<PlayerTeamComponent>(playerEntity).team;
            int selectionIndex = EntityManager.GetComponentData<PlayerSelection>(playerEntity);
            EntityManager.RemoveComponent<PlayerSelection>(playerEntity);

            Entity tileEntity = gridBuffer[selectionIndex].entity;
            OwnerComponent cellOwner = EntityManager.GetComponentData<OwnerComponent>(tileEntity);

            if (cellOwner.team == Team.EMPTY)
            {
                EntityManager.SetComponentData(tileEntity, new OwnerComponent() { team = playerTeam });
                EntityManager.AddComponentData(playerEntity, new MadeSelectionComponent());
            }
        }

        playerEntities.Dispose();
        gridBuffer.Dispose();
    }
}