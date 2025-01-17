﻿using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class MatchingComponent:Entity,IAwake,IUpdate,IDestroy
    {
        public long Ts;
        public int Port;
        public int PortMin;
        public int PortMax;
        public string Ip;

        public System.Collections.Concurrent.ConcurrentQueue<(long,long)> matchingUnits 
            = new System.Collections.Concurrent.ConcurrentQueue<(long, long)>();

        public Dictionary<long, long> AccountSessionDictionary = new Dictionary<long, long>();
    }
}
