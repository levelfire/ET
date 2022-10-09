using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class ExampleBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        GenerateSystemLists(systems);

        var world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, ExplicitDefaultWorldSystems);
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
        return true;
    }

}
