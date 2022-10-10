using System.Runtime.CompilerServices;

namespace ET
{
    public class LoginInfoRecordComponentDestroySystem : DestroySystem<LoginInfoRecordComponent>
    {
        protected override void Destroy(LoginInfoRecordComponent self)
        {
            self.AccountLoginInfoDict.Clear();
        }
    }

    [FriendOf(typeof(LoginInfoRecordComponent))]
    public static class LoginInfoRecordComponentSystem
    {
        public static void Add(this LoginInfoRecordComponent self, long key,int value)
        {
            if (self.AccountLoginInfoDict.ContainsKey(key))
            {
                self.AccountLoginInfoDict[key] = value;
                return;
            }
            self.AccountLoginInfoDict.Add(key, value);
        }

        public static void Remove(this LoginInfoRecordComponent self, long key)
        {
            if (self.AccountLoginInfoDict.ContainsKey(key))
            {
                self.AccountLoginInfoDict.Remove(key);
            }
        }

        public static int Get(this LoginInfoRecordComponent self, long key)
        {
            if (!self.AccountLoginInfoDict.TryGetValue(key,out var value))
            {
                return -1;
            }
            return value;
        }

        public static bool IsExit(this LoginInfoRecordComponent self, long key)
        {
            return self.AccountLoginInfoDict.ContainsKey(key);
        }
    }

    
}
