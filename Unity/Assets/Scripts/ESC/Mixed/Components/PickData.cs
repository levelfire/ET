using Unity.Entities;

[GenerateAuthoringComponent]
[InternalBufferCapacity(16)]
public struct PickData : IBufferElementData
{
    public int Uuid;
    public int ItemId;
}
