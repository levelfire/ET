using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using System.Diagnostics;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;
using Unity.Physics;
using Unity.NetCode;
using ET;
using System;

public enum MoveDirect
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
    None = 4,
}

public enum e_emp_state
{
    Wait,
    Begin,
    Warmup,
    Warning,
    Shrinkage,
    End,
    Finish,
}

public class Config_EMPRound
{
    public int Key { get; set; }
    public int Round { get; set; }
    public int PreparationTime { get; set; }
    public int WarningTime { get; set; }
    public int CoverTime { get; set; }
    public int CoverMapNum { get; set; }
    public int DPS { get; set; }
    public int ReducedHealPrecent { get; set; }
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class EmpSpawnSystem : SystemBase
{
    private EntityQuery m_QueryEmp;
    private BeginSimulationEntityCommandBufferSystem m_BeginSimECB;
    private GhostPredictionSystemGroup m_PredictionGroup;
    private EntityQuery m_GameSettingsQuery;
    private Unity.Entities.Entity m_PrefabEmp;
    private EntityQuery m_ConnectionGroup;


    const int LenSide = 26;
    static int MapSide = 8;
    int m_mapid;
    Dictionary<int, Config_EMPRound> m_round_dic;//TODO check m_round =¡· Index

    int[,] m_matix_grids;
    int[,] m_matix_grids_ner;
    int[,] m_matix_grids_temp;

    HashSet<int> m_grids_live = new HashSet<int>();
    SortedList<int, int> m_grids_find = new SortedList<int, int>();
    SortedList<int, int> m_grids_find_pre = new SortedList<int, int>();

    MoveDirect m_dir_shrinkage = MoveDirect.None; // 1.up 2.down 3.left 4.right
    MoveDirect m_dir_shrinkage_cur = MoveDirect.None; // 1.up 2.down 3.left 4.right
    List<int> m_list_shrinkage = new List<int>();
    List<int> m_list_warning = new List<int>();
    Queue<(MoveDirect, int[])> m_queue_shrinkage = new Queue<(MoveDirect, int[])>();
    e_emp_state m_state = e_emp_state.Wait;

    Dictionary<int, (MoveDirect, GameObject)> m_dic_empbox = new Dictionary<int, (MoveDirect, GameObject)>();

    int m_round = 0;
    float m_ts_warmup;
    float m_ts_warning;
    float m_ts_shrinkage;
    float m_ts_shrinkage_step;
    float m_shrinkage_step_time;

    float m_ts_shrinkage_scene;
    float m_shrinkage_scene_time;

    int m_row_min_live = 0;
    int m_row_max_live = MapSide - 1;
    int m_col_min_live = 0;
    int m_col_max_live = MapSide - 1;
    int m_row_min_del;
    int m_row_max_del;
    int m_col_min_del;
    int m_col_max_del;
    int m_row_mid;
    int m_col_mid;

    //TODO
    //public SharedInt EmpCenter;
    public int EmpCenter;

    //GameObject EmpRoot;
    public int Round { get => m_round; set => m_round = value; }
    protected override void OnCreate()
    {
        m_QueryEmp = GetEntityQuery(ComponentType.ReadWrite<EmpTag>());

        m_BeginSimECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();

        m_GameSettingsQuery = GetEntityQuery(ComponentType.ReadWrite<GameSettingsComponent>());

        RequireForUpdate(m_GameSettingsQuery);

        m_ConnectionGroup = GetEntityQuery(ComponentType.ReadWrite<NetworkStreamConnection>());

        //UnitConfig unitconfig = UnitConfigCategory.Instance.Get(1001);
        ////Log.Debug(unitconfig.Name);

        //var configlist = EmpDataConfigCategory.Instance.GetAll();
        //foreach (var e in configlist.Values)
        //{
        //    Log.Debug("list:" + e.Id);
        //}

        init();
    }

    protected override void OnUpdate()
    {
        if (m_PrefabEmp == Unity.Entities.Entity.Null)
        {
            m_PrefabEmp = GetSingleton<EmpAuthoringComponent>().Prefab;
            begin();
            return;
        }

        var settings = GetSingleton<GameSettingsComponent>();

        var commandBuffer = m_BeginSimECB.CreateCommandBuffer();

        var count = m_QueryEmp.CalculateEntityCountWithoutFiltering();


        var rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        var Prefab = m_PrefabEmp;

        //Job
        //.WithCode(() =>
        //{
        //    for (int i = count; i < 20; ++i)
        //    {
        //        var padding = 0.1f;
        //        var xPosition = rand.NextFloat(-1f * ((settings.levelWidth) / 2 - padding), (settings.levelWidth) / 2 - padding);
        //        var zPosition = rand.NextFloat(-1f * ((settings.levelDepth) / 2 - padding), (settings.levelDepth) / 2 - padding);
        //        var pos = new Translation { Value = new float3(xPosition, 0, zPosition) };
        //        var e = commandBuffer.Instantiate(Prefab);
        //        commandBuffer.SetComponent(e, pos);
        //        //commandBuffer.SetComponent(e, new HpComponent { Value = 2 });
        //    }
        //}).Schedule();

        //m_BeginSimECB.AddJobHandleForProducer(Dependency);

        
        if (m_state != e_emp_state.Finish)
        {
            var deltaTime = m_PredictionGroup.Time.DeltaTime;
            //UnityEngine.Debug.Log($"deltaTime {deltaTime}");
            Job
            .WithoutBurst()
            .WithCode(() =>
            {
                UpdateState(deltaTime, ref commandBuffer);
            }).Run();
            
        }
        
        //float m_ts_warmup;
        //float m_ts_warning;
        //float m_ts_shrinkage;
        //float m_ts_shrinkage_step;
        //UnityEngine.Debug.Log($"Emp round {m_round} state {m_state} ts {m_ts_warmup} {m_ts_warning} {m_ts_shrinkage} {m_ts_shrinkage_step}");
    }


    public void UpdateState(float deltaTime,ref EntityCommandBuffer commandBuffer)
    {
        //UnityEngine.Debug.Log($"UpdateState deltaTime {deltaTime}");
        switch (m_state)
        {
            case e_emp_state.Wait:
                {
                }
                break;
            case e_emp_state.Begin:
                {
                    UpdateStateBegin();
                }
                break;
            case e_emp_state.Warmup:
                {
                    UpdateStateWarmup(deltaTime);
                }
                break;
            case e_emp_state.Warning:
                {
                    UpdateStateWarning(deltaTime);
                }
                break;
            case e_emp_state.Shrinkage:
                {
                    UpdateStateShrinkage(deltaTime,ref commandBuffer);
                }
                break;
            case e_emp_state.End:
                {
                    UpdateStateEnd();
                }
                break;
            case e_emp_state.Finish:
                {

                }
                break;
            default:
                break;
        }
    }

    void UpdateStateBegin()
    {
        if (m_round_dic.ContainsKey(m_round))
        {
            m_ts_warmup = m_round_dic[m_round].PreparationTime;
            m_ts_warning = m_round_dic[m_round].WarningTime;
            m_ts_shrinkage = m_round_dic[m_round].CoverTime;

            m_state = e_emp_state.Warmup;
        }
        else
        {
            m_state = e_emp_state.Finish;
        }
    }

    void UpdateStateWarmup(float deltaTime)
    {
        //UnityEngine.Debug.Log($"UpdateStateWarmup deltaTime {deltaTime}");
        m_ts_warmup -= deltaTime;// Time.DeltaTime;
        if (m_ts_warmup <= 0)
        {
            m_state = e_emp_state.Warning;
            //warning();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            shrinkage_format();
            sw.Stop();
            TimeSpan ts1 = sw.Elapsed;
            //Log.Info("shrinkage_format cost:" + ts1.TotalMilliseconds);

            sw.Reset();
            //Stopwatch sw = new Stopwatch();
            sw.Start();
            shrinkage_action_precomputation();
            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;
            //Log.Info("shrinkage_action_precomputation cost:"+ ts2.TotalMilliseconds);

            int mid = MethHelper.getGridKey(m_row_mid, m_col_mid);
            //var Bh_EmpCenter = GlobalVariables.Instance.GetVariable("EmpCenter");
            //Bh_EmpCenter?.SetValue(mid);
            //EmpCenter.Value = mid;
            EmpCenter = mid;

            //TODO
            //ServerSender.sender.OnEmpWarning(m_round_dic[m_round].WarningTime, m_list_warning, mid);
            //PlayerManager.Instance?.EventCenter(mid);
            ////ProcedureBattle pro_battle = TMW_Main.m_Procedure.GetProcedure<ProcedureBattle>();
            ////if (pro_battle != null)
            ////{
            ////    pro_battle.playerManager.EventCenter(mid);
            ////}
        }
    }

    void UpdateStateWarning(float deltaTime)
    {
        m_ts_warning -= deltaTime;// Time.DeltaTime;
        if (m_ts_warning <= 0)
        {
            m_state = e_emp_state.Shrinkage;
            //shrinkage_format();
            if (m_shrinkage_step_time == 0)
            {
                m_state = e_emp_state.Finish;
                UnityEngine.Debug.Log("m_shrinkage_step_time is zero");
                return;
            }
            m_ts_shrinkage_step = m_shrinkage_step_time;
            m_ts_shrinkage_scene = m_shrinkage_scene_time;
            //shrinkage_mid();

            //TODO
            //ServerSender.sender.OnEmpShrinkage((int)m_ts_shrinkage, m_queue_shrinkage.Count);
        }
    }


    //TODO
    void UpdateStateShrinkage(float deltaTime, ref EntityCommandBuffer commandBuffer)
    {
        m_ts_shrinkage_scene -= deltaTime;// Time.DeltaTime;
        if (m_ts_shrinkage_scene <= 0)
        {
            m_ts_shrinkage_scene = m_shrinkage_scene_time;
            //TODO
            //ServerSender.sender.OnEmpScene(m_dir_shrinkage_cur,0);
            foreach ((MoveDirect dir, GameObject obj) in m_dic_empbox.Values)
            {
                var box = obj.GetComponent<BoxCollider2D>();

                if (box.size.x < LenSide || box.size.y < LenSide)
                {
                    update_empbox(obj, dir);
                }
            }
        }


        m_ts_shrinkage_step -= deltaTime;// Time.DeltaTime;
        if (m_ts_shrinkage_step <= 0)
        {
            m_ts_shrinkage_step = m_shrinkage_step_time;
            //shrinkage_action_v1();
            //if (m_list_shrinkage.Count > 0)
            //{
            //    ServerSender.sender.OnEmpList(m_list_shrinkage);
            //}
            //printMatix(ref m_matix_grids);
            shrinkage_action_v2(ref commandBuffer);
            //Log.Info("m_queue_shrinkage left:" + m_queue_shrinkage.Count);
        }
        m_ts_shrinkage -= deltaTime;// Time.DeltaTime;
        if (m_ts_shrinkage <= 0
            //&& ((m_row_min_del > m_row_max_del) || (m_col_min_del > m_col_max_del))
            && m_queue_shrinkage.Count <= 0
            )
        {
            m_state = e_emp_state.End;
        }
    }

    void UpdateStateEnd()
    {
        if (m_round < m_round_dic.Count)
        {
            m_round += 1;
            m_state = e_emp_state.Begin;
        }
        else
        {
            m_state = e_emp_state.Finish;
        }
    }

    public void init()
    {
        //TODO check config
        //if (!GameConfigComponent.Contents.Config_EMPRoundDict.ContainsKey(StageMapComponent.Instance.StageMapData.empID))
        //{
        //    return;
        //}

        //MapSide = StageMapComponent.Instance.StageMapData.Size;
        MapSide = 8;

        //TODO
        //EmpRoot = GameObject.Find("Emp");

        m_round = 1;
        m_state = e_emp_state.Wait;

        //TODO init config dic
        //m_mapid = StageMapComponent.Instance.StageMapData.empID;
        ////m_round_dic = GameConfigComponent.Contents.Config_EMPRoundDict[m_mapid];
        //m_round_dic = GameConfigComponent.Contents.Config_EMPRoundDict[m_mapid].ToDictionary(key => key.Round, obj => obj);

        //1002    1   30  75  75  38  3   20
        //1002    2   0   60  60  18  7   40
        //1002    3   0   45  45  7   13  60
        //1002    4   0   30  30  1   20  80
        //public class Config_EMPRound
        //{
        //    public int Key { get; set; }
        //    public int Round { get; set; }
        //    public int PreparationTime { get; set; }
        //    public int WarningTime { get; set; }
        //    public int CoverTime { get; set; }
        //    public int CoverMapNum { get; set; }
        //    public int DPS { get; set; }
        //    public int ReducedHealPrecent { get; set; }
        //}
        m_round_dic = new Dictionary<int, Config_EMPRound>();
        //m_round_dic.Add(1, new Config_EMPRound { Round = 1, PreparationTime = 30, WarningTime = 75, CoverTime = 75, CoverMapNum = 38, DPS = 3,ReducedHealPrecent=20 });
        //m_round_dic.Add(2, new Config_EMPRound { Round = 2, PreparationTime = 0, WarningTime = 60, CoverTime = 60, CoverMapNum = 18, DPS = 7, ReducedHealPrecent = 40 });
        //m_round_dic.Add(3, new Config_EMPRound { Round = 3, PreparationTime = 0, WarningTime = 45, CoverTime = 45, CoverMapNum = 7, DPS = 13, ReducedHealPrecent = 60 });
        //m_round_dic.Add(4, new Config_EMPRound { Round = 4, PreparationTime = 0, WarningTime = 30, CoverTime = 30, CoverMapNum = 1, DPS = 20, ReducedHealPrecent = 80 });
        m_round_dic.Add(1, new Config_EMPRound { Round = 1, PreparationTime = 10, WarningTime = 7, CoverTime = 75, CoverMapNum = 38, DPS = 3, ReducedHealPrecent = 20 });
        m_round_dic.Add(2, new Config_EMPRound { Round = 2, PreparationTime = 0, WarningTime = 6, CoverTime = 60, CoverMapNum = 18, DPS = 7, ReducedHealPrecent = 40 });
        m_round_dic.Add(3, new Config_EMPRound { Round = 3, PreparationTime = 0, WarningTime = 4, CoverTime = 45, CoverMapNum = 7, DPS = 13, ReducedHealPrecent = 60 });
        m_round_dic.Add(4, new Config_EMPRound { Round = 4, PreparationTime = 0, WarningTime = 3, CoverTime = 30, CoverMapNum = 1, DPS = 20, ReducedHealPrecent = 80 });

        int round_max_index = m_round_dic.Count;
        int rowCenter = UnityEngine.Random.Range(0, MapSide);
        int colCenter = UnityEngine.Random.Range(0, MapSide);

        m_matix_grids = new int[MapSide, MapSide];
        m_matix_grids_ner = new int[MapSide, MapSide];
        //m_matix_grids_temp = new int[MapSide, MapSide];
        m_list_shrinkage.Clear();
        m_grids_live.Clear();
        m_grids_find.Clear();
        m_grids_find_pre.Clear();
        m_grids_find.Add(MethHelper.getGridKey(rowCenter, colCenter), 0);

        m_row_min_live = 0;
        m_row_max_live = MapSide - 1;
        m_col_min_live = 0;
        m_col_max_live = MapSide - 1;

        for (int round = round_max_index; round > 0; round--)
        {
            int count = m_round_dic[round].CoverMapNum;
            per_add_live(round, count);
        }
        m_matix_grids_temp = (int[,])m_matix_grids.Clone();
        m_queue_shrinkage.Clear();
        m_dir_shrinkage = MoveDirect.None;
        m_dic_empbox.Clear();
        //EmpCenter = new SharedInt();
        //GlobalVariables.Instance.SetVariable("EmpCenter", EmpCenter);
        //EmpCenter.Value = -1;
        EmpCenter = -1;
    }

    public void begin()
    {
//#if UNITY_EDITOR
//        if (!ConfigDebug.EnableEmp)
//            return;
//#endif
        m_state = e_emp_state.Begin;
    }

    void per_add_live(int round, int count)
    {
        while (count-- > 0)
        {
            if (m_grids_find_pre.Count > 0)
            {
                int key = (int)m_grids_find_pre.Keys[0];//.IndexOfKey(0);//.GetKey(0);
                if (!m_grids_live.Contains(key))
                {
                    m_grids_live.Add(key);
                    m_grids_find.Remove(key);
                    m_grids_find_pre.Remove(key);

                    (int row, int col) = MethHelper.getGirdRC(key);
                    m_matix_grids[row, col] = round;

                    per_find_neighbour(key);
                }
            }
            else
            {
                if (m_grids_find.Count <= 0)
                {
                    Log.Error("gird count emp > map");
                    return;
                }
                int indexnext = UnityEngine.Random.Range(0, m_grids_find.Count - 1);
                int key = (int)m_grids_find.Keys[indexnext];// [indexnext];

                if (!m_grids_live.Contains(key))
                {
                    m_grids_live.Add(key);
                    m_grids_find.Remove(key);

                    (int row, int col) = MethHelper.getGirdRC(key);

                    UnityEngine.Debug.Log($"row {row } col {col} len {m_matix_grids.Length}");
                    m_matix_grids[row, col] = round;

                    per_find_neighbour(key);
                }
            }
        }
    }

    void per_find_neighbour(int key)
    {
        (int row, int col) = MethHelper.getGirdRC(key);
        if (row < 0 || row >= MapSide || col < 0 || col >= MapSide)
        {
            return;
        }

        per_add_find(row - 1, col);
        per_add_find(row + 1, col);
        per_add_find(row, col - 1);
        per_add_find(row, col + 1);
    }

    void per_add_find(int row, int col)
    {
        if (row < 0 || row >= MapSide || col < 0 || col >= MapSide)
        {
            return;
        }
        int key = MethHelper.getGridKey(row, col);
        if (!m_grids_live.Contains(key))
        {
            if (!m_grids_find.ContainsKey(key))
            {
                m_grids_find.Add(key, 0);
            }
            m_matix_grids_ner[row, col] += 1;
            if (m_matix_grids_ner[row, col] >= 4)
            {
                m_grids_find_pre.Add(key, 0);
            }
        }
    }

    void warning()
    {
        m_list_warning.Clear();
        for (int row_cur = 0; row_cur < MapSide; row_cur++)
        {
            for (int col_cur = 0; col_cur < MapSide; col_cur++)
            {
                int m = m_matix_grids[row_cur, col_cur];
                if (m > 0 && m <= m_round)
                {
                    m_list_warning.Add(MethHelper.getGridKey(row_cur, col_cur));
                }
            }
        }
    }
    void shrinkage_format()
    {
        m_list_warning.Clear();
        int row_min = MapSide - 1;
        int row_max = 0;
        int col_min = MapSide - 1;
        int col_max = 0;

        //R
        m_row_min_del = MapSide - 1;
        m_row_max_del = 0;
        m_col_min_del = MapSide - 1;
        m_col_max_del = 0;

        for (int row_cur = m_row_min_live; row_cur <= m_row_max_live; row_cur++)
        {
            for (int col_cur = m_col_min_live; col_cur <= m_col_max_live; col_cur++)
            {
                int m = m_matix_grids[row_cur, col_cur];
                if (m > 0)
                {
                    if (m <= m_round)
                    {
                        m_row_min_del = m_row_min_del <= row_cur ? m_row_min_del : row_cur;
                        m_row_max_del = m_row_max_del > row_cur ? m_row_max_del : row_cur;
                        m_col_min_del = m_col_min_del <= col_cur ? m_col_min_del : col_cur;
                        m_col_max_del = m_col_max_del > col_cur ? m_col_max_del : col_cur;

                        m_list_warning.Add(MethHelper.getGridKey(row_cur, col_cur));
                    }
                    else
                    {
                        row_min = row_min <= row_cur ? row_min : row_cur;
                        row_max = row_max > row_cur ? row_max : row_cur;
                        col_min = col_min <= col_cur ? col_min : col_cur;
                        col_max = col_max > col_cur ? col_max : col_cur;
                    }
                }
            }
        }

        m_row_min_live = row_min;
        m_row_max_live = row_max;
        m_col_min_live = col_min;
        m_col_max_live = col_max;

        //int row_d = m_row_max_del - m_row_min_del;
        //int col_d = m_col_max_del - m_col_min_del;
        //int step = (row_d+1) + (col_d+1);
        //int time = m_round_dic[m_round].CoverTime;
        //m_shrinkage_step_time = time / step;

        m_row_mid = (m_row_min_live + m_row_max_live) / 2;
        m_col_mid = (m_col_min_live + m_col_max_live) / 2;
    }

    bool shrinkage_action_v1()
    {
        m_dir_shrinkage = (MoveDirect)UnityEngine.Random.Range(0, 4);// MoveDirect.None;
        m_list_shrinkage.Clear();
        while (m_list_shrinkage.Count <= 0)
        {
            if (m_row_max_del < m_row_min_del)
            {
                return false;
            }
            if (m_col_max_del < m_col_min_del)
            {
                return false;
            }

            int row_d = m_row_max_del - m_row_min_del;
            int col_d = m_col_max_del - m_col_min_del;
            if (row_d > col_d || (row_d == col_d && UnityEngine.Random.Range(0, 2) == 0))
            //if (UnityEngine.Random.Range(0, 2) == 0)
            {
                shrinkage_row_v1();
            }
            else
            {
                shrinkage_col_v1();
            }
        }
        return true;
    }

    void shrinkage_action_v2(ref EntityCommandBuffer commandBuffer)
    {
        if (m_queue_shrinkage.Count <= 0)
        {
            //Log.Warning("queue_shrinkage empty");
            return;
        }
        (MoveDirect dir, var arr) = m_queue_shrinkage.Dequeue();
        m_dir_shrinkage_cur = dir;
        foreach (var key in arr)
        {
            (int row, int col) = MethHelper.getGirdRC(key);
            m_matix_grids_temp[row, col] = 0;
            //create_empbox(key, dir);
            CreateEmpBoxEntity(key, dir, ref commandBuffer);
        }
        //TODO
        //ServerSender.sender.OnEmpList((int)m_shrinkage_step_time, arr.ToList(), dir);
        ////printMatix(ref m_matix_grids_temp);
    }

    void printMatix(ref int[,] grids)
    {
        string l = "";
        for (int i = 0; i < MapSide; i++)
        {
            for (int j = 0; j < MapSide; j++)
            {
                int v = grids[i, j];
                //string sv = v >= 0 ? "+" : "";
                //l = l + "   " + sv + v;
                l = l + "   " + v;
            }
            l = l + "\n";
        }
        Log.Info(l);
    }

    void shrinkage_action_precomputation()
    {
        while (shrinkage_action_v1())
        {
            m_queue_shrinkage.Enqueue((m_dir_shrinkage, m_list_shrinkage.ToArray()));
        }
        int time = m_round_dic[m_round].CoverTime;
        m_shrinkage_step_time = time / (m_queue_shrinkage.Count + 1);
        m_shrinkage_scene_time = m_shrinkage_step_time / (LenSide / 2);
    }

    void shrinkage_row_v1()
    {
        ////if (row_max_del - row_mid >= row_mid - row_min_del)
        //if (UnityEngine.Random.Range(0, 2) == 0)
        //{
        //    //for (int i = col_min_del; i <= col_max_del; i++)
        //    for (int i = 0; i < MapSide; i++)
        //    {
        //        shrinkage_list_v1(m_row_max_del, i);
        //    }
        //    m_row_max_del--;
        //}
        //else
        //{
        //    //for (int i = col_min_del; i <= col_max_del; i++)
        //    for (int i = 0; i < MapSide; i++)
        //    {
        //        shrinkage_list_v1(m_row_min_del, i);
        //    }
        //    m_row_min_del++;
        //}

        bool del_max = false;
        if (m_row_min_del >= m_row_mid)
        {
            del_max = true;
        }
        else if (m_row_max_del <= m_row_mid)
        {
            del_max = false;
        }
        else
        {
            if (m_row_max_del - m_row_mid >= m_row_mid - m_row_min_del)
            {
                del_max = true;
            }
        }

        if (del_max)
        {
            for (int i = 0; i < MapSide; i++)
            {
                shrinkage_list_v1(m_row_max_del, i);
            }
            //Log.Info("row min:" + m_row_min_del + " max:" + m_row_max_del + " del:" + m_row_max_del);
            m_row_max_del--;
            m_dir_shrinkage = MoveDirect.Down;
        }
        else
        {
            for (int i = 0; i < MapSide; i++)
            {
                shrinkage_list_v1(m_row_min_del, i);
            }
            //Log.Info("row min:" + m_row_min_del + " max:" + m_row_max_del + " del:" + m_row_min_del);
            m_row_min_del++;
            m_dir_shrinkage = MoveDirect.Up;
        }
    }

    void shrinkage_col_v1()
    {
        ////if (row_max_del - row_mid >= row_mid - row_min_del)
        //if (UnityEngine.Random.Range(0, 2) == 0)
        //{
        //    //for (int i = col_min_del; i <= col_max_del; i++)
        //    for (int i = 0; i < MapSide; i++)
        //    {
        //        shrinkage_list_v1(i, m_col_max_del);
        //    }
        //    m_col_max_del--;
        //}
        //else
        //{
        //    //for (int i = col_min_del; i <= col_max_del; i++)
        //    for (int i = 0; i < MapSide; i++)
        //    {
        //        shrinkage_list_v1(i, m_col_min_del);
        //    }
        //    m_col_min_del++;
        //}

        bool del_max = false;
        if (m_col_min_del >= m_col_mid)
        {
            del_max = true;
        }
        else if (m_col_max_del <= m_col_mid)
        {
            del_max = false;
        }
        else
        {
            if (m_col_max_del - m_col_mid >= m_col_mid - m_col_min_del)
            {
                del_max = true;
            }
        }

        if (del_max)
        {
            for (int i = 0; i < MapSide; i++)
            {
                shrinkage_list_v1(i, m_col_max_del);
            }
            //Log.Info("col min:" + m_col_min_del + " max:" + m_col_max_del + " del:" + m_col_max_del);
            m_col_max_del--;
            m_dir_shrinkage = MoveDirect.Right;
        }
        else
        {
            for (int i = 0; i < MapSide; i++)
            {
                shrinkage_list_v1(i, m_col_min_del);
            }
            //Log.Info("col min:" + m_col_min_del + " max:" + m_col_max_del + " del:" + m_col_min_del);
            m_col_min_del++;
            m_dir_shrinkage = MoveDirect.Left;
        }
    }

    void shrinkage_list_v1(int row, int col)
    {
        int m = m_matix_grids[row, col];
        if (m > 0 && m <= m_round)
        {
            m_matix_grids[row, col] = 0;
            m_list_shrinkage.Add(MethHelper.getGridKey(row, col));
        }
    }

    void create_empbox(int key, MoveDirect dir)
    {
        return;
        (int row, int col) = MethHelper.getGirdRC(key);
        //TODO offset£¬path emp map need unify
        int offcheck = 0;// 1;
        int offsetX = LenSide * col - offcheck;
        int offsetY = (-LenSide) * row + offcheck;

        var obj = new GameObject("EmpBox" + key);
        GameObject.Instantiate(obj);
        obj.transform.position = new Vector3(offsetX, offsetY, 0);
        var box = obj.AddComponent<BoxCollider2D>();
        box.isTrigger = true;

        (int sizeX, int sizeY) = get_emp_change_size(dir, 0);
        (int oX, int oY) = get_emp_change_offset(dir, 0);

        box.size = new Vector2(sizeX, sizeY);
        box.offset = new Vector2(oX, oY);
        obj.tag = "Emp";
        obj.layer = 12;

        //TODO
        //box.transform.SetParent(EmpRoot.transform);

        m_dic_empbox.Add(key, (dir, obj));
    }

    void CreateEmpBoxEntity(int key, MoveDirect dir,ref EntityCommandBuffer commandBuffer)
    {
        UnityEngine.Debug.Log($"CreateEmpBoxEntity");
        (int row, int col) = MethHelper.getGirdRC(key);
        //TODO offset£¬path emp map need unify
        int offcheck = 0;// 1;
        int offsetX = LenSide * col - offcheck;
        int offsetY = (-LenSide) * row + offcheck;

        //var obj = new GameObject("EmpBox" + key);
        //GameObject.Instantiate(obj);
        //obj.transform.position = new Vector3(offsetX, offsetY, 0);
        //var box = obj.AddComponent<BoxCollider2D>();
        //box.isTrigger = true;

        //(int sizeX, int sizeY) = get_emp_change_size(dir, 0);
        //(int oX, int oY) = get_emp_change_offset(dir, 0);

        //box.size = new Vector2(sizeX, sizeY);
        //box.offset = new Vector2(oX, oY);

        var e = commandBuffer.Instantiate(m_PrefabEmp);
        //var scale = new NonUniformScale {Value = new float3(10,1,10) };
        //commandBuffer.SetComponent(e, scale);

        //NonUniformScale scale;
        //if (e.HasComponent<NonUniformScale>())
        //{
        //    scale = EntityManager.GetComponentData<NonUniformScale>(e);
        //}
        //else
        //{
        //    scale = new NonUniformScale();

        //}

        ////TODO
        //var scale = new NonUniformScale { Value = new float3(10, 10, 10) };
        //commandBuffer.AddComponent(e, scale);
        //scale.Value = new float3(10, 10, 10);
        //commandBuffer.SetComponent(e, scale);

        //var scale = new Scale { Value = 10 };
        //commandBuffer.AddComponent(e, scale);
        //commandBuffer.SetComponent(e, scale);
        var pos = new Translation { Value = new float3(offsetX, -0.5f, offsetY) };
        commandBuffer.SetComponent(e, pos);

        //commandBuffer.SetComponent(e, new HpComponent { Value = 2 });
    }
    void update_empbox(GameObject obj, MoveDirect dir)
    {
        return;
        var box = obj.GetComponent<BoxCollider2D>();
        int x = (int)box.size.x;
        int y = (int)box.size.y;
        if (x == y && x == LenSide)
        {
            return;
        }

        int len = x > y ? y / 2 : x / 2;
        (int sizeX, int sizeY) = get_emp_change_size(dir, len + 1);
        (int offsetX, int offsetY) = get_emp_change_offset(dir, len + 1);

        box.size = new Vector2(sizeX, sizeY);
        //box.offset = new Vector2(offsetX + 13, offsetY - 13);
        box.offset = new Vector2(offsetX, offsetY);
    }

    (int, int) get_emp_change_size(MoveDirect dir, int len)
    {
        int x = 0;
        int y = 0;
        switch (dir)
        {
            case MoveDirect.Up:
            case MoveDirect.Down:
                {
                    x = LenSide;
                    y = len * 2;
                }
                break;
            case MoveDirect.Left:
            case MoveDirect.Right:
                {
                    x = len * 2;
                    y = LenSide;
                }
                break;
            default:
                break;
        }
        return (x, y);
    }

    (int, int) get_emp_change_offset(MoveDirect dir, int len)
    {
        int x = LenSide / 2;
        int y = -LenSide / 2;
        switch (dir)
        {
            case MoveDirect.Up:
                {
                    y = -len;
                }
                break;
            case MoveDirect.Left:
                {
                    x = len;
                }
                break;
            case MoveDirect.Down:
                {
                    y = -LenSide + len;
                }
                break;
            case MoveDirect.Right:
                {
                    x = LenSide - len;
                }
                break;
            default:
                break;
        }
        return (x, y);
    }

    public int getDps()
    {
        if (m_round <= 0 || m_round > 4)
        {
            return 0;
        }

        return m_round_dic[m_round].DPS;
    }

    public int getReducedHealPrecent()
    {
        if (m_round <= 0 || m_round > 4)
        {
            return 0;
        }

        return m_round_dic[m_round].ReducedHealPrecent;
    }
}
