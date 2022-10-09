
namespace ET
{
    [MessageHandler(SceneType.Client)]
    public class A2C_MatchingResultHandler : AMHandler<A2C_MatchingResult>
    {
        protected override async ETTask Run(Session session, A2C_MatchingResult message)
        {
            Log.Debug($"A2C_MatchingResult port {message.port} ");
            System.Environment.SetEnvironmentVariable("BattlePort", message.port.ToString());
            await ETTask.CompletedTask;
        }
    }
}
