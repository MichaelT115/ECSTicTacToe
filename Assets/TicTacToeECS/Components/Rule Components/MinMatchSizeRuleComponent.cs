using Unity.Entities;

/// <summary>
/// The minimum size of a match. 
/// </summary>
public struct MinMatchSizeRuleComponent : IComponentData
{
    public static implicit operator int(MinMatchSizeRuleComponent minMatchSizeComponent) => minMatchSizeComponent.minMatchSize;

    public int minMatchSize;
}