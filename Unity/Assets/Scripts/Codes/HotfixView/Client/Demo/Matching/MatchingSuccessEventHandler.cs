using ET.EventType;

namespace ET
{
    [Event(SceneType.Client)]
    public class MatchingSuccessEventHandler : AEvent<MatchingSuccess>
    {
        protected override async ETTask Run(Scene scene, MatchingSuccess a)
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LocalScene");
            //await ETTask.CompletedTask;
        }
    }
}
