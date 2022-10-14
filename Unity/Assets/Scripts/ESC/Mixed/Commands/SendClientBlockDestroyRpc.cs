using Unity.NetCode;

public struct SendClientBlockDestroyRpc : IRpcCommand
{
    public ushort Pos;
}