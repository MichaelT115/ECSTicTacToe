using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class SelectionValidationSystem : ComponentSystem
{
    Bootstrapper bootstrapper;

    EntityQuery playerQuery;
    EntityQuery gridQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(PlayerSelection));

        RequireForUpdate(playerQuery);

        bootstrapper = GameObject.FindObjectOfType<Bootstrapper>();
    }

    protected override void OnUpdate()
    {
        var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);

        foreach (Entity playerEntity in playerEntities)
        {
            Team playerTeam = EntityManager.GetComponentData<PlayerTeamComponent>(playerEntity).team;
            Entity cellEntity = EntityManager.GetComponentData<PlayerSelection>(playerEntity).selectedEntity;
            EntityManager.RemoveComponent<PlayerSelection>(playerEntity);

            OwnerComponent cellOwner = EntityManager.GetComponentData<OwnerComponent>(cellEntity);

            if (cellOwner.team == Team.EMPTY)
            {
                EntityManager.SetComponentData(cellEntity, new OwnerComponent() { team = playerTeam });
                EntityManager.AddComponentData(playerEntity, new MadeSelectionComponent());

                EntityManager.SetSharedComponentData(cellEntity, new RenderMesh()
                {
                    mesh = bootstrapper.mesh,
                    material = playerTeam == Team.X ? bootstrapper.xMaterial : bootstrapper.yMaterial
                });
            }
        }

        playerEntities.Dispose();
    }
}