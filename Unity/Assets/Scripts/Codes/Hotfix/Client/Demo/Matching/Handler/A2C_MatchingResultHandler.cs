
namespace ET
{
    [MessageHandler(SceneType.Client)]
    public class A2C_MatchingResultHandler : AMHandler<A2C_MatchingResult>
    {
        protected override async ETTask Run(Session session, A2C_MatchingResult message)
        {
            Log.Debug($"A2C_MatchingResult port {message.Port} ");
            System.Environment.SetEnvironmentVariable("BattlePort", message.Port.ToString());

            await EventSystem.Instance.PublishAsync(session.DomainScene(), new EventType.MatchingSuccess());
            await ETTask.CompletedTask;
        }
    }
}
