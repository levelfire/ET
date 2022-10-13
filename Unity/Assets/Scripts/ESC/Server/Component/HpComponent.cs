using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct HpComponent : IComponentData
{
    [GhostField]
    public int Value;
}