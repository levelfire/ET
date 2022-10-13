using Unity.NetCode;

public struct SendClientBlockChangeRpc : IRpcCommand
{
    public ushort Pos;
}