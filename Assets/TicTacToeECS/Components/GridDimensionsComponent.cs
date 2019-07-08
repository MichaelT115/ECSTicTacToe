using Unity.Entities;

public struct GridDimensionsComponent : IComponentData
{
    public int columnCount;
    public int rowCount;
}