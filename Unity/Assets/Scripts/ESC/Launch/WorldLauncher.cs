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

public class WorldLauncher : MonoBehaviour
{
    void OnEnable()
    {
    }

    void Start()
    {
        WLauncher();

        SceneManager.LoadSceneAsync("SampleScene");
    }


    public void WLauncher()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        ClientServerBootstrap.CreateServerWorld(world, "ServerWorld");
    }
}