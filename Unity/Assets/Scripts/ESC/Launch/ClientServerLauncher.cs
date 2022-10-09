using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ClientServerLauncher : MonoBehaviour
{
    void OnEnable()
    {
    }

    void Start()
    {
        ServerLauncher();
        ClientLauncher();

        SceneManager.LoadSceneAsync("SampleScene");
    }

    public void ServerLauncher()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        ClientServerBootstrap.CreateServerWorld(world, "ServerWorld");
    }

    public void ClientLauncher()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        ClientServerBootstrap.CreateClientWorld(world, "ClientWorld");
    }

    void StartGameScene()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            SceneManager.LoadSceneAsync("SampleScene");
#if UNITY_EDITOR
        else
            Debug.Log("Loading: " + "SampleScene");
#endif
    }
}