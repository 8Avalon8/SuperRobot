using System.Collections.Generic;

namespace SuperRobot
{
    // 驾驶员管理系统
public class PilotManagementSystem : GameSystem
{
    private readonly Dictionary<int, GameEntity> _pilots           = new Dictionary<int, GameEntity>();
    private readonly Dictionary<int, int>        _pilotAssignments = new Dictionary<int, int>(); // PilotId -> UnitId
    private          int                         _nextPilotId      = 1;
    
    public override void Initialize()
    {
        // 订阅事件
        EventManager.Subscribe<PilotRecruiteEvent>(OnPilotRecruited);
        EventManager.Subscribe<PilotAssignmentRequestEvent>(OnPilotAssignmentRequest);
        EventManager.Subscribe<PilotTrainRequestEvent>(OnPilotTrainRequest);
        EventManager.Subscribe<TurnEndedEvent>(OnTurnEnded);
    }
    
    public override void Execute()
    {
        // 主要逻辑在事件处理程序中
    }
    
    /// <summary>
    /// 创建新驾驶员
    /// </summary>
    public GameEntity CreatePilot(string pilotName, Dictionary<string, int> initialStats = null)
    {
        // 创建实体
        var pilotEntity = EntityManager.CreateEntity("Pilot");
        
        // 添加驾驶员属性组件
        var statsComp = pilotEntity.AddComponent<PilotStatsComponent>();
        statsComp.PilotId = _nextPilotId++;
        statsComp.PilotName = pilotName;
        
        // 设置初始属性
        if (initialStats != null)
        {
            if (initialStats.TryGetValue("Reaction", out int reaction))
                statsComp.Reaction = reaction;
            if (initialStats.TryGetValue("Shooting", out int shooting))
                statsComp.Shooting = shooting;
            if (initialStats.TryGetValue("Melee", out int melee))
                statsComp.Melee = melee;
            if (initialStats.TryGetValue("Focus", out int focus))
                statsComp.Focus = focus;
            if (initialStats.TryGetValue("Adaptability", out int adaptability))
                statsComp.Adaptability = adaptability;
            if (initialStats.TryGetValue("NTLevel", out int ntLevel))
                statsComp.NTLevel = ntLevel;
        }
        else
        {
            // 随机生成属性（20-60范围内）
            statsComp.Reaction = UnityEngine.Random.Range(20, 61);
            statsComp.Shooting = UnityEngine.Random.Range(20, 61);
            statsComp.Melee = UnityEngine.Random.Range(20, 61);
            statsComp.Focus = UnityEngine.Random.Range(20, 61);
            statsComp.Adaptability = UnityEngine.Random.Range(20, 61);
            statsComp.NTLevel = UnityEngine.Random.value < 0.1f ? 1 : 0; // 10%概率有NT能力
        }
        
        // 注册驾驶员
        _pilots[statsComp.PilotId] = pilotEntity;
        
        return pilotEntity;
    }
    
    /// <summary>
    /// 获取驾驶员
    /// </summary>
    public GameEntity GetPilot(int pilotId)
    {
        if (_pilots.TryGetValue(pilotId, out var pilot))
            return pilot;
        
        return null;
    }
    
    /// <summary>
    /// 获取所有驾驶员
    /// </summary>
    public List<GameEntity> GetAllPilots()
    {
        return new List<GameEntity>(_pilots.Values);
    }
    
    /// <summary>
    /// 获取可用驾驶员
    /// </summary>
    public List<GameEntity> GetAvailablePilots()
    {
        List<GameEntity> availablePilots = new List<GameEntity>();
        
        foreach (var pilot in _pilots.Values)
        {
            var statsComp = pilot.GetComponent<PilotStatsComponent>();
            if (statsComp != null && statsComp.IsAvailable && !_pilotAssignments.ContainsKey(statsComp.PilotId))
            {
                availablePilots.Add(pilot);
            }
        }
        
        return availablePilots;
    }
    
    /// <summary>
    /// 分配驾驶员到单位
    /// </summary>
    public bool AssignPilotToUnit(int pilotId, int unitId)
    {
        if (!_pilots.TryGetValue(pilotId, out var pilot))
            return false;
        
        var statsComp = pilot.GetComponent<PilotStatsComponent>();
        if (statsComp == null || !statsComp.IsAvailable)
            return false;
        
        // 检查单位是否有效
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var unit = unitSystem.GetUnit(unitId);
        if (unit == null)
            return false;
        
        // 检查单位是否需要驾驶员
        var unitStatsComp = unit.GetComponent<UnitStatsComponent>();
        if (unitStatsComp == null || !unitStatsComp.RequiresPilot)
            return false;
        
        // 如果单位已有驾驶员，先移除
        var pilotComp = unit.GetComponent<PilotComponent>();
        if (pilotComp != null && pilotComp.PilotId != -1)
        {
            _pilotAssignments.Remove(pilotComp.PilotId);
        }
        
        // 分配驾驶员
        if (pilotComp == null)
        {
            pilotComp = unit.AddComponent<PilotComponent>();
        }
        
        pilotComp.PilotId = pilotId;
        _pilotAssignments[pilotId] = unitId;
        
        // 计算兼容性
        pilotComp.CalculateCompatibility(unit, pilot);
        
        // 重新计算单位属性
        unitSystem.RecalculateUnitStats(unit);
        
        return true;
    }
    
    /// <summary>
    /// 移除驾驶员从单位
    /// </summary>
    public bool RemovePilotFromUnit(int pilotId)
    {
        if (!_pilotAssignments.TryGetValue(pilotId, out var unitId))
            return false;
        
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var unit = unitSystem.GetUnit(unitId);
        if (unit == null)
            return false;
        
        var pilotComp = unit.GetComponent<PilotComponent>();
        if (pilotComp == null || pilotComp.PilotId != pilotId)
            return false;
        
        // 重置驾驶员ID
        pilotComp.PilotId = -1;
        pilotComp.CompatibilityRate = 1.0f;
        
        // 移除分配记录
        _pilotAssignments.Remove(pilotId);
        
        // 重新计算单位属性
        unitSystem.RecalculateUnitStats(unit);
        
        return true;
    }
    
    /// <summary>
    /// 训练驾驶员
    /// </summary>
    public bool TrainPilot(int pilotId, string attributeToTrain)
    {
        if (!_pilots.TryGetValue(pilotId, out var pilot))
            return false;
        
        var statsComp = pilot.GetComponent<PilotStatsComponent>();
        if (statsComp == null)
            return false;
        
        // 消耗资源
        var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
        if (!resourceSystem.ConsumeResource(ResourceType.Money, 500))
            return false;
        
        // 提升对应属性
        switch (attributeToTrain.ToLower())
        {
            case "reaction":
                if (statsComp.Reaction < 99)
                    statsComp.Reaction += 1;
                break;
            case "shooting":
                if (statsComp.Shooting < 99)
                    statsComp.Shooting += 1;
                break;
            case "melee":
                if (statsComp.Melee < 99)
                    statsComp.Melee += 1;
                break;
            case "focus":
                if (statsComp.Focus < 99)
                    statsComp.Focus += 1;
                break;
            case "adaptability":
                if (statsComp.Adaptability < 99)
                    statsComp.Adaptability += 1;
                break;
            default:
                return false;
        }
        
        // 如果驾驶员已分配到单位，重新计算单位属性
        if (_pilotAssignments.TryGetValue(pilotId, out var unitId))
        {
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var unit = unitSystem.GetUnit(unitId);
            if (unit != null)
            {
                unitSystem.RecalculateUnitStats(unit);
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 驾驶员事件处理程序
    /// </summary>
    private void OnPilotRecruited(PilotRecruiteEvent evt)
    {
        CreatePilot(evt.PilotName, evt.InitialStats);
    }
    
    private void OnPilotAssignmentRequest(PilotAssignmentRequestEvent evt)
    {
        if (evt.Assign)
        {
            AssignPilotToUnit(evt.PilotId, evt.UnitId);
        }
        else
        {
            RemovePilotFromUnit(evt.PilotId);
        }
    }
    
    private void OnPilotTrainRequest(PilotTrainRequestEvent evt)
    {
        TrainPilot(evt.PilotId, evt.AttributeToTrain);
    }
    
    private void OnTurnEnded(TurnEndedEvent evt)
    {
        // 回合结束时恢复驾驶员疲劳
        foreach (var pilot in _pilots.Values)
        {
            var statsComp = pilot.GetComponent<PilotStatsComponent>();
            if (statsComp != null)
            {
                statsComp.RecoverFatigue(10);
            }
        }
        
        // 如果驾驶员参与了战斗，增加经验值
        foreach (var battleData in evt.CompletedBattles)
        {
            foreach (var participantUnitId in battleData.ParticipantUnitIds)
            {
                var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
                var unit = unitSystem.GetUnit(participantUnitId);
                if (unit != null)
                {
                    var pilotComp = unit.GetComponent<PilotComponent>();
                    if (pilotComp != null && pilotComp.PilotId != -1)
                    {
                        var pilot = GetPilot(pilotComp.PilotId);
                        if (pilot != null)
                        {
                            var statsComp = pilot.GetComponent<PilotStatsComponent>();
                            if (statsComp != null)
                            {
                                // 根据战斗表现给予经验
                                int expGain = battleData.IsPlayerWin ? 50 : 20;
                                if (battleData.Contribution.TryGetValue(participantUnitId, out var contribution))
                                {
                                    expGain += contribution;
                                }
                                
                                statsComp.AddExperience(expGain);
                                
                                // 增加疲劳度
                                statsComp.AddFatigue(15);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public override void Cleanup()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<PilotRecruiteEvent>(OnPilotRecruited);
        EventManager.Instance.Unsubscribe<PilotAssignmentRequestEvent>(OnPilotAssignmentRequest);
        EventManager.Instance.Unsubscribe<PilotTrainRequestEvent>(OnPilotTrainRequest);
        EventManager.Instance.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
    }
}
}
