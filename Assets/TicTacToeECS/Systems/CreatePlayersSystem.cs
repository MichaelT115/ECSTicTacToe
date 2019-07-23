using Unity.Entities;

[DisableAutoCreation]
public class CreatePlayersSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        var entityQuery = GetEntityQuery(typeof(PlayerListElement));
        var turnControllerEntity = entityQuery.GetSingletonEntity();  

        // All players have a team component
        EntityArchetype playerArchetype = EntityManager.CreateArchetype(typeof(PlayerTeamComponent));

        Entity playerXEntity = EntityManager.CreateEntity(playerArchetype);
        EntityManager.SetComponentData(playerXEntity, new PlayerTeamComponent() { team = Team.X });
        EntityManager.AddComponentData(playerXEntity, new UserControlledComponent());

        Entity playerOEntity = EntityManager.CreateEntity(playerArchetype);
        EntityManager.SetComponentData(playerOEntity, new PlayerTeamComponent() { team = Team.O });
        EntityManager.AddComponentData(playerOEntity, new UserControlledComponent());

        var buffer = EntityManager.GetBuffer<PlayerListElement>(turnControllerEntity);
        buffer.Add(new PlayerListElement() { playerEntity = playerXEntity });
        buffer.Add(new PlayerListElement() { playerEntity = playerOEntity });
    }
}