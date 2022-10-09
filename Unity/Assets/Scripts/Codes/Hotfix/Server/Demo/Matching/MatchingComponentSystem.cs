using ET.Server;
using System;

namespace ET
{
    public class MatchingComponentAwakeSystem : AwakeSystem<MatchingComponent>
    {
        protected override void Awake(MatchingComponent self)
        {
            self.ts = DateTimeOffset.Now.ToUnixTimeSeconds() + 5;
        }
    }

    public class MatchingComponentUpdateSystem : UpdateSystem<MatchingComponent>
    {
        protected override void Update(MatchingComponent self)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() >= self.ts)
            {
                self.ts += 5;
                Log.Info($"Matching TryDequeue {self.ts}");
                if (self.matchingUnits.TryDequeue(out var unit))
                {
                    Log.Info($"Matching unit{unit}");
                    //StartBattle().Coroutine();
                    Log.Info("StartBattle 1");
                    System.Diagnostics.Process exep = new System.Diagnostics.Process();
                    Log.Info("StartBattle 2");
                    //exep.StartInfo.FileName = @"E:\NTK\branches\Dots\Demos\temp\Builds\DS\093001\dots_3d_urp_root.exe";
                    exep.StartInfo.FileName = @"E:\github.com\levelfire\et\Unity\Builds\DSWorld\2022100902\ET.exe";
                    exep.StartInfo.Arguments = "battle 127.0.0.1 5002";
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

                    SendResult(unit.Item2);
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

        public static void SendResult(long sesionInstanceId)
        {
            Session otherSession = EventSystem.Instance.Get(sesionInstanceId) as Session;
            otherSession?.Send(new A2C_MatchingResult { Error = 0,Ip="127.0.0.1",port = 5002 });
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

        public static long GetAccount(this MatchingComponent self, long accountId)
        {
            if (!self.AccountSessionDictionary.TryGetValue(accountId, out long instanceId))
            {
                return 0;
            }
            return instanceId;
        }

        public static void AddAccount(this MatchingComponent self, long accountId, long sessionInstanceId)
        {
            if (self.AccountSessionDictionary.ContainsKey(accountId))
            {
                self.AccountSessionDictionary[accountId] = sessionInstanceId;
                return;
            }
            self.AccountSessionDictionary.Add(accountId, sessionInstanceId);
        }

        public static void RemoveAccount(this MatchingComponent self, long accountId)
        {
            if (self.AccountSessionDictionary.ContainsKey(accountId))
            {
                self.AccountSessionDictionary.Remove(accountId);
            }
        }
    }
}
