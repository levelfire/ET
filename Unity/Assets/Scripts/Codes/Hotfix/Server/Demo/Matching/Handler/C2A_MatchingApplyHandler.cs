using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Account)]
    public class C2A_MatchingApplyHandler : AMRpcHandler<C2A_MatchingApply, A2C_MatchingApply>
    {
        protected override async ETTask Run(Session session, C2A_MatchingApply request, A2C_MatchingApply response, Action reply)
        {
            if (session.DomainScene().SceneType != SceneType.Account)
            {
                Log.Error($"Error Scene Request,curscene is :{session.DomainScene().SceneType}");
                session.Dispose();
                return;
            }

            await ETTask.CompletedTask;
        }
    }
}
