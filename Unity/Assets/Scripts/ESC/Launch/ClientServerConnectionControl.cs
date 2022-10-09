using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.NetCode;
using UnityEngine;
using Unity;
using System;

#if UNITY_EDITOR
using Unity.NetCode.Editor;
#endif


[UpdateInWorld(TargetWorld.Server)]
public partial class ServerConnectionControl : SystemBase
{
    private ushort m_GamePort;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitializeServerComponent>();

    }

    protected override void OnUpdate()
    {
        var serverDataEntity = GetSingletonEntity<ServerDataComponent>();
        var serverData = EntityManager.GetComponentData<ServerDataComponent>(serverDataEntity);
        m_GamePort = serverData.GamePort;

        string[] CommandLineArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < CommandLineArgs.Length; i++)
        {
            Debug.Log("CommandLineArgs: " + i + ":" + CommandLineArgs[i]);
        }
        if (CommandLineArgs.Length >= 4)
        {
            //s_address = CommandLineArgs[1];
            m_GamePort = Convert.ToUInt16(CommandLineArgs[3]);
        }

        EntityManager.DestroyEntity(GetSingletonEntity<InitializeServerComponent>());

        var grid = EntityManager.CreateEntity();
        EntityManager.AddComponentData(grid, new GhostDistanceImportance
        {
            ScaleImportanceByDistance = GhostDistanceImportance.DefaultScaleFunctionPointer,
            TileSize = new int3(10, 10, 10),
            TileCenter = new int3(0, 0, 0),
            TileBorderWidth = new float3(1f, 1f, 1f)
        });

        NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
        ep.Port = m_GamePort;
        World.GetExistingSystem<NetworkStreamReceiveSystem>().Listen(ep);
        Debug.Log($"Server is listening on port: {m_GamePort}");
    }
}

[UpdateInWorld(TargetWorld.Client)]
public partial class ClientConnectionControl : SystemBase
{
    private string m_ConnectToServerIp;
    private ushort m_GamePort;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitializeClientComponent>();

    }

    protected override void OnUpdate()
    {
        var clientDataEntity = GetSingletonEntity<ClientDataComponent>();
        var clientData = EntityManager.GetComponentData<ClientDataComponent>(clientDataEntity);

        m_ConnectToServerIp = clientData.ConnectToServerIp.ToString();
        m_GamePort = clientData.GamePort;

        var battlePort = System.Environment.GetEnvironmentVariable("BattlePort");
        if (!string.IsNullOrEmpty(battlePort))
        {
            m_GamePort =  Convert.ToUInt16(battlePort);
        }
            
        EntityManager.DestroyEntity(GetSingletonEntity<InitializeClientComponent>());

        NetworkEndPoint ep = NetworkEndPoint.Parse(m_ConnectToServerIp, m_GamePort);
        World.GetExistingSystem<NetworkStreamReceiveSystem>().Connect(ep);
        Debug.Log($"Client connecting to ip: {m_ConnectToServerIp} and port: {m_GamePort}");
    }
}