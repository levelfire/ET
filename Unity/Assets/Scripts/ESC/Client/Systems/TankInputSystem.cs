using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class TankInputSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

    private ClientSimulationSystemGroup m_ClientSimulationSystemGroup;

    private int m_FrameCount;

    //private int m_LastDir;

    protected override void OnCreate()
    {
        m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        m_ClientSimulationSystemGroup = World.GetOrCreateSystem<ClientSimulationSystemGroup>();

        //m_LastDir = 0;

        RequireSingletonForUpdate<NetworkStreamInGame>(); 
    }

    protected override void OnUpdate()
    {
        bool isThinClient = HasSingleton<ThinClientComponent>();
        if (HasSingleton<CommandTargetComponent>() && GetSingleton<CommandTargetComponent>().targetEntity == Entity.Null)
        {
            if (isThinClient)
            {
                var ent = EntityManager.CreateEntity();
                EntityManager.AddBuffer<TankCommand>(ent);
                SetSingleton(new CommandTargetComponent { targetEntity = ent });
            }
        }

        byte right, left, thrust, reverseThrust, selfDestruct, shoot;
        right = left = thrust = reverseThrust = selfDestruct = shoot = 0;

        //float mouseX = 0;
        //float mouseY = 0;

        if (!isThinClient)
        {
            if (Input.GetKey("d"))
            {
                right = 1;
            }
            if (Input.GetKey("a"))
            {
                left = 1;
            }
            if (Input.GetKey("w"))
            {
                thrust = 1;
            }
            if (Input.GetKey("s"))
            {
                reverseThrust = 1;
            }
            if (Input.GetKey("p"))
            {
                selfDestruct = 1;
            }
            if (Input.GetKey("j"))
            {
                shoot = 1;
            }
        }
        else
        {
            var state = (int)Time.ElapsedTime % 3;
            if (state == 0)
            {
                left = 1;
            }
            else
            {
                thrust = 1;
            }
            ++m_FrameCount;
            if (m_FrameCount % 100 == 0)
            {
                shoot = 1;
                m_FrameCount = 0;
            }
        }

        var inputTargetTick = m_ClientSimulationSystemGroup.ServerTick;
        var commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
        var inputFromEntity = GetBufferFromEntity<TankCommand>();
        //var curDir = left | (right << 1) | (thrust << 2) | (reverseThrust << 3) | (shoot << 4);
        //if (m_LastDir == curDir)
        //{
        //    return;
        //}

        TryGetSingletonEntity<TankCommand>(out var targetEntity);
        Job.WithCode(() =>
        {
            if (isThinClient && shoot != 0)
            {
                var req = commandBuffer.CreateEntity();
                commandBuffer.AddComponent<TankSpawnRequestRpc>(req);
                commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent());
            }
            if (targetEntity == Entity.Null)
            {
                if (shoot != 0)
                {
                    var req = commandBuffer.CreateEntity();
                    commandBuffer.AddComponent<TankSpawnRequestRpc>(req);
                    commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent());
                }
            }
            else
            {
                var input = inputFromEntity[targetEntity];
                input.AddCommandData(new TankCommand
                {
                    Tick = inputTargetTick,
                    left = left,
                    right = right,
                    thrust = thrust,
                    reverseThrust = reverseThrust,
                    selfDestruct = selfDestruct,
                    shoot = shoot
                });
            }
        }).Schedule();

        //m_LastDir = curDir;

        m_BeginSimEcb.AddJobHandleForProducer(Dependency);
    }
}