using Unity.Entities;

public struct OwnerComponent : IComponentData
{
    public static implicit operator OwnerComponent(Team team) => new OwnerComponent() { team = team };
    public static implicit operator Team(OwnerComponent ownerComponent) => ownerComponent.team;

    public Team team;
}