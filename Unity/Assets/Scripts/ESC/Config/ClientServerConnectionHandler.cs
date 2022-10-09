using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ClientServerConnectionHandler : MonoBehaviour
{
    public ClientServerInfo ClientServerInfo;

    private GameObject[] launchObjects;

    void Awake()
    {
        launchObjects = GameObject.FindGameObjectsWithTag("LaunchObject");
        foreach (GameObject launchObject in launchObjects)
        {
            if (launchObject.GetComponent<ServerLaunchObjectData>() != null)
            {
                ClientServerInfo.IsServer = true;

                foreach (var world in World.All)
                {
                    if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
                    {
                        var ServerDataEntity = world.EntityManager.CreateEntity();
                        world.EntityManager.AddComponentData(ServerDataEntity, new ServerDataComponent
                        {
                            GamePort = ClientServerInfo.GamePort
                        });
                        world.EntityManager.CreateEntity(typeof(InitializeServerComponent));
                    }
                }
            }

            if (launchObject.GetComponent<ClientLaunchObjectData>() != null)
            {
                ClientServerInfo.IsClient = true;
                ClientServerInfo.ConnectToServerIp = launchObject.GetComponent<ClientLaunchObjectData>().IPAddress;

                foreach (var world in World.All)
                {
                    if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
                    {
                        var ClientDataEntity = world.EntityManager.CreateEntity();
                        world.EntityManager.AddComponentData(ClientDataEntity, new ClientDataComponent
                        {
                            ConnectToServerIp = ClientServerInfo.ConnectToServerIp,
                            GamePort = ClientServerInfo.GamePort
                        });
                        world.EntityManager.CreateEntity(typeof(InitializeClientComponent));
                    }
                }
            }
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}