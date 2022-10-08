using ET.EventType;

namespace ET.Client
{
	[Event(SceneType.Client)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.AppStartInitFinish args)
		{
			await UIHelper.Create(scene, UIType.UILogin, UILayer.Mid);

            Test(scene).Coroutine();
        }

        public async ETTask Test(Scene zoneScene)
        {
            Computer computer = zoneScene.AddChild<Computer>();
            EventSystem.Instance.Publish(zoneScene,new EventType.InstallComputer() { Computer = computer });
            //EventSystem.Instance.PublishAsync(zoneScene, new EventType.InstallComputerAsync() { Computer = computer }).Coroutine();
            //Game.EventSystem.Publish(new EventType.InstallComputer() { Computer = computer });

            

            //computer.AddComponent<PCCaseComponent>();
            //computer.Start();

            //await TimerComponent.Instance.WaitAsync(3000);
            //computer.Dispose();

            //UnitConfig unitconfig = UnitConfigCategory.Instance.Get(1001);
            //Log.Debug(unitconfig.Name);

            //var configlist = UnitConfigCategory.Instance.GetAll();
            //foreach (var e in configlist.Values)
            //{
            //    Log.Debug("list:" +  e.Name);
            //}
        }
    }
}
