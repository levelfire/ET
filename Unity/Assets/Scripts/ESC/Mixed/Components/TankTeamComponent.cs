using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct TankTeamComponent : IComponentData
{
    [GhostField]
    public int TeamId;
}