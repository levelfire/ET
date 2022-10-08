using System.Collections.Generic;

namespace ET.Server
{
    //public class TokenComponentAwakeSystem : AwakeSystem<TokenComponent>
    //{
    //    protected override void Awake(TokenComponent self)
    //    {
    //        self.TokenDictionary = new Dictionary<long, string>();
    //    }

    //}

    [FriendOfAttribute(typeof(TokenComponent))]
    public static class TokenComponentSystem
    {
        public static void Add(this TokenComponent self, long key, string token)
        {
            self.TokenDictionary.Add(key, token);
            self.TimeOutRemoveKey(key, token).Coroutine();
        }

        public static string Get(this TokenComponent self, long key)
        {
            string token = null;
            self.TokenDictionary.TryGetValue(key, out token);
            return token;
        }

        public static void Remove(this TokenComponent self, long key)
        {
            if (self.TokenDictionary.ContainsKey(key))
            {
                self.TokenDictionary.Remove(key);
            }
        }

        private static async ETTask TimeOutRemoveKey(this TokenComponent self,long key,string tokenkey)
        {
            await TimerComponent.Instance.WaitAsync(10*60*1000);
            string onlineToken = self.Get(key);

            if (!string.IsNullOrEmpty(onlineToken) && onlineToken == tokenkey)
            {
                self.Remove(key);
            }
        }
    }
}
