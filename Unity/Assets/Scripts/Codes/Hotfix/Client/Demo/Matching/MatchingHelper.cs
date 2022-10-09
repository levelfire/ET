using System;

namespace ET.Client
{
    public static class MatchingHelper
    {
        public static async ETTask<int> MatchingApply(Scene clientScene)
        {
            A2C_MatchingApply a2C_MatchingApply = null;

            try
            {
                a2C_MatchingApply = (A2C_MatchingApply)await clientScene.GetComponent<SessionComponent>().Session.Call(new C2A_MatchingApply { });
            }
            catch (Exception e)
            {
                //accountSession.Dispose();
                Log.Error(e.ToString());
                return ErrorCode.ERR_NetWorkError;
            }

            Log.Debug("进入匹配队列!");
            //await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());

            return ErrorCode.ERR_Success;
        }
    }
}
