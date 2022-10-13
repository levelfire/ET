using Unity.Entities;

[GenerateAuthoringComponent]
[InternalBufferCapacity(16)]
public struct PackageData : IBufferElementData
{
    public int Uuid;
    public int ItemId;
}
