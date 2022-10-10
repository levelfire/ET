using ET.Server;
using System;

namespace ET
{
    [ActorMessageHandler(SceneType.LoginCenter)]
    public class A2L_LoginAccountRequestHandler : AMActorRpcHandler<Scene, A2L_LoginAccountRequest, L2A_LoginAccountResponse>
    {
        protected override async ETTask Run(Scene scene, A2L_LoginAccountRequest request, L2A_LoginAccountResponse response, Action reply)
        {
            long accountId = request.AccountId;
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginCenterLock, accountId.GetHashCode()))
            {
                if (scene.GetComponent<LoginInfoRecordComponent>().IsExit(accountId))
                {
                    reply();
                    return;
                }

                int zone = scene.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                //TODO update other GetGate
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGateByAccountId(zone,accountId);

                var g2lDisconnectGateUnit = (G2L_DisconnectGateUnit) await MessageHelper.CallActor(gateConfig.InstanceId, new L2G_DisconnectGateUnit { AccountId = accountId });

                response.Error = g2lDisconnectGateUnit.Error;
                reply();
            }
        }
    }
}
