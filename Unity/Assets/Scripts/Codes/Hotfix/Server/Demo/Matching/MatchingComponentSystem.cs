using ET.Server;
using System;
using System.Collections.Generic;

namespace ET
{
    public class MatchingComponentAwakeSystem : AwakeSystem<MatchingComponent>
    {
        protected override void Awake(MatchingComponent self)
        {
            //TODO ip port
            self.Ts = DateTimeOffset.Now.ToUnixTimeSeconds() + 5;
            self.PortMin = 5001;
            self.PortMax = 5100;
            self.Port = self.PortMin;
            self.Ip = ConstValue.RouterHttpHost;
        }
    }

    public class MatchingComponentUpdateSystem : UpdateSystem<MatchingComponent>
    {
        protected override void Update(MatchingComponent self)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() >= self.Ts)
            {
                self.Ts += 5;
                Log.Debug($"Matching TryDequeue {self.Ts}");
                bool match = false;
                while (self.matchingUnits.TryDequeue(out var unit))
                {
                    Log.Debug($"Matching unit{unit}");
                    StartBattle(unit.Item2, self.Port,self.Ip).Coroutine();
                    match = true;
                }
                if (match)
                {
                    self.Port++;
                    if (self.Port >= self.PortMax)
                    {
                        self.Port = self.PortMin;
                    }
                }
            }
        }

        private async ETTask StartBattle(long sessionId,int curPort,string Ip)
        {
            Log.Debug($"Battle Start");
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = @"..\..\Builds\DSWorld\Battle\ET.exe";
            var ip = Ip;
            var port = curPort;

            //TODO Random MapModel
            var maps = new List<int>();
            var mapsStr = string.Empty;
            
            for (int i = 0; i < 8 * 8; i++)
            {
                var e = RandomGenerator.RandomNumber(1, 10);
                maps.Add(e);
                var appendstr = string.IsNullOrEmpty(mapsStr) ? e.ToString() : "," + e.ToString();
                mapsStr += appendstr;
            }

            Log.Debug($"ServerBattle {mapsStr}");

            exep.StartInfo.Arguments = $"ServerBattle {ip} {port} {mapsStr}";
            exep.EnableRaisingEvents = true;
            exep.Exited += new System.EventHandler(exep_Exited);
            exep.Start();
            void exep_Exited(object sender, System.EventArgs e)
            {
                Log.Debug("Battle complete");
            }

            SendResult(sessionId, ip, port, maps);
            await ETTask.CompletedTask;
        }

        public static void SendResult(long sesionInstanceId,string ip, int port,List<int> maps)
        {
            Session otherSession = EventSystem.Instance.Get(sesionInstanceId) as Session;
            var result = new A2C_MatchingResult { Error = 0, Ip = ip, Port = port };
            result.Maps = maps;
            //foreach (var e in maps)
            //{
            //    result.Maps.Add(e);
            //}
            otherSession?.Send(result);
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
