namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class MatchingComponent:Entity,IAwake,IUpdate,IDestroy
    {
        public System.Collections.Concurrent.ConcurrentQueue<(long,long)> matchingUnits 
            = new System.Collections.Concurrent.ConcurrentQueue<(long, long)>();
    }
}
