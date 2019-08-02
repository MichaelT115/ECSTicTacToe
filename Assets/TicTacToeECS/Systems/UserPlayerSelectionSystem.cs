using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

/// <summary>
/// Create a player selection based of user input.
/// </summary>
public class UserPlayerSelectionSystem : JobComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery nonFocusedCellsQuery;
    EntityQuery focuesedCellsQuery;

    private struct UnfocusCellsThatDoNotIntersectPointJob : IJobForEachWithEntity_EC<RectangleComponent>
    {
        [ReadOnly] public float2 point;
        [ReadOnly] public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref RectangleComponent rectangle)
        {
            if (!CollisionHelper.Intersect(point, rectangle))
            {
                commandBuffer.RemoveComponent<FocusComponent>(index,  entity);
            }
        }
    }

    private struct FocusCellsThatIntersectPointJob : IJobForEachWithEntity_EC<RectangleComponent>
    {
        [ReadOnly] public float2 point;
        [ReadOnly] public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref RectangleComponent rectangle)
        {
            if (CollisionHelper.Intersect(point, rectangle))
            {
                commandBuffer.AddComponent(index, entity, new FocusComponent());
            }
        }
    }

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(HasTurnComponent));
        nonFocusedCellsQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectangleComponent) },
            None = new ComponentType[] { typeof(FocusComponent) }
        });
        focuesedCellsQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectangleComponent), typeof(FocusComponent) }
        });

        RequireForUpdate(playerQuery);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entity playerEntity = playerQuery.GetSingletonEntity();
        Team playerTeam = EntityManager.GetComponentData<PlayerTeamComponent>(playerEntity).team;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float2 position = new float2(mousePosition.x, mousePosition.y);

        BeginSimulationEntityCommandBufferSystem entityCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        new FocusCellsThatIntersectPointJob()
        {
            point = position,
            commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(nonFocusedCellsQuery).Complete();

        new UnfocusCellsThatDoNotIntersectPointJob()
        {
            point = position,
            commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(focuesedCellsQuery).Complete();

        if (focuesedCellsQuery.CalculateEntityCount() > 0 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            var focusedCells = focuesedCellsQuery.ToEntityArray(Allocator.TempJob);
            EntityManager.AddComponentData(playerEntity, new PlayerSelection() { selectedEntity = focusedCells[0] });
            focusedCells.Dispose();
        }

        return inputDeps;
    }
}

public struct FocusComponent : IComponentData { };
