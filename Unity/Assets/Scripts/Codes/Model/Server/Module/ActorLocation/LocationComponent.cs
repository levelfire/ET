using System.Collections.Generic;

namespace ET.Server
{
    [ChildOf(typeof(LocationComponent))]
    public class LockInfo: Entity, IAwake<long, CoroutineLock>, IDestroy
    {
        public long LockInstanceId;

        public CoroutineLock CoroutineLock;
    }
    
    [ComponentOf(typeof(Scene))]
    public class LocationComponent: Entity, IAwake
    {
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();

        public readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
    }

    [ComponentOf(typeof(Scene))]
    public class TokenComponent : Entity, IAwake
    {
        public readonly Dictionary<long, string> TokenDictionary = new Dictionary<long, string>();
        //public Dictionary<long, string> TokenDictionary { get; set; }
    }

    [ComponentOf(typeof(Scene))]
    public class AccountSessionsComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, long> AccountSessionDictionary = new Dictionary<long, long>();
    }

    [ComponentOf(typeof(Session))]
    public class AccountCheckOutTimeComponent : Entity, IAwake<long>, IDestroy
    {
        public long Timer = 0;
        public long AccountId = 0;
    }
}