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

public class LocalLauncher : MonoBehaviour
{
    bool Load = false;
    void OnEnable()
    {
    }

    void Start()
    {
        //LLauncher();

        //SceneManager.LoadSceneAsync("SampleScene");
    }

    private void Update()
    {
        if (!Load && World.DefaultGameObjectInjectionWorld != null)
        {
            LLauncher();
            SceneManager.LoadSceneAsync("SampleScene");

            Load = true;
        }
    }

    public void LLauncher()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        ClientServerBootstrap.CreateClientWorld(world, "ClientWorld");
    }
}