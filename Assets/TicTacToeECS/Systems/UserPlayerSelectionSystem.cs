using Unity.Entities;
using UnityEngine;

/// <summary>
/// Create a player selection based of user input.
/// </summary>
public class UserPlayerSelectionSystem : ComponentSystem
{
    EntityQuery playerQuery;
    EntityQuery gridQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(HasTurnComponent));
        gridQuery = GetEntityQuery(typeof(GridCellData));
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = World.Active.EntityManager;
        if (playerQuery.CalculateLength() == 0)
            return;

        Entity playerEntity = playerQuery.GetSingletonEntity();

        Team playerTeam = entityManager.GetComponentData<PlayerTeamComponent>(playerEntity).team;

        Entity gridEntity = gridQuery.GetSingletonEntity();
        DynamicBuffer<GridCellData> gridBuffer = entityManager.GetBuffer<GridCellData>(gridEntity);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 0 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 1 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 2 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 3 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 4 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 5 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 6 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 7 });
            return;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            entityManager.AddComponentData(playerEntity, new PlayerSelection() { selctionIndex = 8 });
            return;
        }
    }
}
