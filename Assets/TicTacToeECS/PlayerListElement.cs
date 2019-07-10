using Unity.Entities;

struct PlayerListElement : IBufferElementData
{
    public static implicit operator Entity(PlayerListElement playerListElement)
    {
        return playerListElement.playerEntity;
    }

    public Entity playerEntity;
}