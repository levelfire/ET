namespace ET
{
    public class MatchingComponentUpdateSystem : UpdateSystem<MatchingComponent>
    {
        protected override void Update(MatchingComponent self)
        {
            if (self.matchingUnits.TryDequeue(out var unit))
            {
                Log.Info("Matching Dequeue 1");
                Log.Info($"Matching 1111 unit{unit}");
                //StartBattle().Coroutine();
                Log.Info("StartBattle 1");
                System.Diagnostics.Process exep = new System.Diagnostics.Process();
                Log.Info("StartBattle 2");
                exep.StartInfo.FileName = @"E:\NTK\branches\Dots\Demos\temp\Builds\DS\093001\dots_3d_urp_root.exe";
                Log.Info("StartBattle 3");
                exep.EnableRaisingEvents = true;
                Log.Info("StartBattle 4");
                exep.Exited += new System.EventHandler(exep_Exited);
                Log.Info("StartBattle 5");
                exep.Start();
                Log.Info("StartBattle ex");
                void exep_Exited(object sender, System.EventArgs e)
                {
                    Log.Info("Battle complete");
                }
            }
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

    public class MatchingComponentDestroySystem : DestroySystem<MatchingComponent>
    {
        protected override void Destroy(MatchingComponent self)
        {
            self.matchingUnits.Clear();
        }
    }

    [FriendOf(typeof(MatchingComponent))]
    public static class MatchingComponentSystem
    {
        public static void Add(this MatchingComponent self, long accountId, long sessionInstanceId)
        {
            self.matchingUnits.Enqueue((accountId, sessionInstanceId));
        }
    }
}
