using Unity.Entities;

[InternalBufferCapacity(1024)]
public struct GridCellData : IBufferElementData
{
    public Entity entity;
}
