using System;


namespace ET.Server
{
    [MessageHandler(SceneType.Account)]
    public class C2A_PingHandler : AMRpcHandler<C2G_Ping, G2C_Ping>
    {
        protected override async ETTask Run(Session session, C2G_Ping request, G2C_Ping response, Action reply)
        {
            response.Time = TimeHelper.ServerNow();
            reply();
            await ETTask.CompletedTask;
        }
    }
}