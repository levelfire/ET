using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct HpComponent : IComponentData
{
    public int Value;
}