using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// Update Player.
/// </summary>
[DisableAutoCreation]
public class AdvancePlayerSystem : ComponentSystem
{
    EntityQuery turnControllerQuery;
    EntityQuery playerQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent));
        turnControllerQuery = GetEntityQuery(typeof(CurrentPlayerIndexComponent), typeof(PlayerListElement));
    }

    protected override void OnUpdate()
    {
        Entity turnControllerEntity = turnControllerQuery.GetSingletonEntity();

        // Get Current Player
        int playerCount = EntityManager.GetBuffer<PlayerListElement>(turnControllerEntity).Length;
        int currentPlayerIndex = EntityManager.GetComponentData<CurrentPlayerIndexComponent>(turnControllerEntity).index;
        EntityManager.SetComponentData(turnControllerEntity, new CurrentPlayerIndexComponent()
        {
            index = (currentPlayerIndex + 1) % playerCount
        });
    }
}
