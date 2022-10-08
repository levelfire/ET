using ET.EventType;

namespace ET
{
    [Event(SceneType.Client)]
    public class InstallComputer_AddComponent : AEvent<InstallComputer>
    {
        protected override async ETTask Run(Scene scene, InstallComputer a)
        {
            Computer computer = a.Computer;

            computer.AddComponent<PCCaseComponent>();
            computer.Start();

            await TimerComponent.Instance.WaitAsync(3000);
            computer.Dispose();

            UnitConfig unitconfig = UnitConfigCategory.Instance.Get(1001);
            Log.Debug(unitconfig.Name);

            var configlist = UnitConfigCategory.Instance.GetAll();
            foreach (var e in configlist.Values)
            {
                Log.Debug("list:" + e.Name);
            }
            //throw new System.NotImplementedException();
            //await ETTask.CompletedTask;
        }
    }

    [Event(SceneType.Client)]
    public class InstallComputerAsync_AddComponent : AEvent<InstallComputerAsync>
    {
        protected override async ETTask Run(Scene scene, InstallComputerAsync a)
        {
            Computer computer = a.Computer;

            computer.AddComponent<PCCaseComponent>();
            computer.Start();

            await TimerComponent.Instance.WaitAsync(3000);
            computer.Dispose();

            UnitConfig unitconfig = UnitConfigCategory.Instance.Get(1001);
            Log.Debug(unitconfig.Name);

            var configlist = UnitConfigCategory.Instance.GetAll();
            foreach (var e in configlist.Values)
            {
                Log.Debug("list:" + e.Name);
            }
            //throw new System.NotImplementedException();
        }
    }
}
