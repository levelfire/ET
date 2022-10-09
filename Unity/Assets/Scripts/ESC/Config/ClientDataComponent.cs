using System;
using Unity.Entities;
using Unity.Collections;

public struct ClientDataComponent : IComponentData
{
    public FixedString64Bytes ConnectToServerIp;
    public ushort GamePort;
}