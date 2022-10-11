using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class EmpDataConfigCategory : ConfigSingleton<EmpDataConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, EmpDataConfig> dict = new Dictionary<int, EmpDataConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<EmpDataConfig> list = new List<EmpDataConfig>();
		
        public void Merge(object o)
        {
            EmpDataConfigCategory s = o as EmpDataConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (EmpDataConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public EmpDataConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EmpDataConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EmpDataConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EmpDataConfig> GetAll()
        {
            return this.dict;
        }

        public EmpDataConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class EmpDataConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>轮数</summary>
		[ProtoMember(2)]
		public int Round { get; set; }
		/// <summary>预热时间</summary>
		[ProtoMember(3)]
		public string PreparationTime { get; set; }
		/// <summary>预警时间</summary>
		[ProtoMember(4)]
		public string WarningTime { get; set; }
		/// <summary>缩圈时间</summary>
		[ProtoMember(5)]
		public int CoverTime { get; set; }
		/// <summary>缩圈块数</summary>
		[ProtoMember(6)]
		public int CoverMapNum { get; set; }
		/// <summary>伤害</summary>
		[ProtoMember(7)]
		public int Damage { get; set; }
		/// <summary>降低治疗百分比</summary>
		[ProtoMember(8)]
		public int ReducedHealPrecent { get; set; }

	}
}
