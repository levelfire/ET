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
            //StartBattle().Coroutine();
        }

        private async ETTask StartBattle()
        {
            Log.Info($"StartBattle");
            //System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            //info.FileName = @"E:\NTK\branches\Dots\Demos\temp\Builds\DS\093001\dots_3d_urp_root.exe";
            //info.Arguments = "";
            //info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
            //System.Diagnostics.Process pro = System.Diagnostics.Process.Start(info);
            //pro.WaitForExit();

            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = @"E:\NTK\branches\Dots\Demos\temp\Builds\DS\093001\dots_3d_urp_root.exe";
            exep.EnableRaisingEvents = true;
            exep.Exited += new System.EventHandler(exep_Exited);
            exep.Start();
            void exep_Exited(object sender, System.EventArgs e)
            {
                Log.Info($"Battle complete");
            }
            await ETTask.CompletedTask;
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
