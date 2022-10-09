namespace ET
{
    public class MatchingComponentUpdateSystem : UpdateSystem<MatchingComponent>
    {
        protected override void Update(MatchingComponent self)
        {
            if (self.matchingUnits.TryDequeue(out var unit))
            {
                Log.Info($"Matching unit{unit}");
                //StartBattle().Coroutine();
            }
        }

        private async ETTask StartBattle()
        {
            Log.Info($"StartBattle");
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = @"E:\NTK\branches\Dots\Demos\temp\Builds\DS\093001\dots_3d_urp_root.exe";
            info.Arguments = "";
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
            System.Diagnostics.Process pro = System.Diagnostics.Process.Start(info);
            pro.WaitForExit();
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
