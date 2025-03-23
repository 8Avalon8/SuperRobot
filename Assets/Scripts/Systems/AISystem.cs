using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperRobot
{
    // AI系统
public class AISystem : GameSystem
{
    // AI玩家拥有的资源和单位
    private Dictionary<ResourceType, int> _aiResources;
    private List<GameEntity> _aiUnits;
    private List<GameEntity> _aiBases;
    
    // AI行为控制参数
    private float _aggressionLevel;
    private float _expansionPriority;
    private float _defensePriority;
    private float _techPriority;
    
    // 当前AI目标
    private AITargetType _currentMainTarget;
    private Vector2Int _targetPosition;
    private int _targetEntityId;
    
    private enum AITargetType
    {
        Expansion,
        Attack,
        Defense,
        Research
    }
    
    public override void Initialize()
    {
        // 初始化AI资源
        _aiResources = new Dictionary<ResourceType, int>();
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            _aiResources[type] = 0;
        }
        _aiResources[ResourceType.Money] = 5000;
        _aiResources[ResourceType.StandardAlloy] = 200;
        
        // 初始化AI单位和基地列表
        _aiUnits = new List<GameEntity>();
        _aiBases = new List<GameEntity>();
        
        // 设置AI行为参数（可根据难度调整）
        var gameConfig = GameManager.Instance.GameConfig;
        _aggressionLevel = gameConfig.AIAggressionBase;
        _expansionPriority = 0.7f;
        _defensePriority = 0.5f;
        _techPriority = 0.3f;
        
        // 订阅事件
        EventManager.Subscribe<TurnPhaseChangedEvent>(OnTurnPhaseChanged);
        EventManager.Subscribe<AIBaseDestroyedEvent>(OnAIBaseDestroyed);
        EventManager.Subscribe<AIUnitDestroyedEvent>(OnAIUnitDestroyed);
    }
    
    public override void Execute()
    {
        // AI行为主要在回合阶段事件中处理
    }
    
    /// <summary>
    /// AI回合处理
    /// </summary>
    private void ProcessAITurn()
    {
        // 更新AI资源
        UpdateResources();
        
        // 决策主要目标
        DecideMainTarget();
        
        // 根据主要目标执行行动
        switch (_currentMainTarget)
        {
            case AITargetType.Expansion:
                PerformExpansion();
                break;
            case AITargetType.Attack:
                PerformAttack();
                break;
            case AITargetType.Defense:
                PerformDefense();
                break;
            case AITargetType.Research:
                PerformResearch();
                break;
        }
        
        // 生产单位
        ManageProduction();
        
        // 移动单位
        MoveUnits();
        
        // 触发AI回合结束事件
        EventManager.TriggerEvent(new AITurnEndedEvent());
    }
    
    /// <summary>
    /// 更新AI资源
    /// </summary>
    private void UpdateResources()
    {
        // 基础收入
        _aiResources[ResourceType.Money] += 1000;
        _aiResources[ResourceType.StandardAlloy] += 50;
        
        // 基地产出
        foreach (var baseEntity in _aiBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null && baseComp.IsOperational)
            {
                _aiResources[ResourceType.Money] += baseComp.MoneyProduction;
                
                foreach (var resource in baseComp.ResourceProduction)
                {
                    if (resource.Value > 0)
                    {
                        _aiResources[resource.Key] += resource.Value;
                    }
                }
            }
        }
        
        // 应用AI资源倍率
        var gameConfig = GameManager.Instance.GameConfig;
        foreach (var resource in _aiResources.Keys.ToList())
        {
            _aiResources[resource] = (int)(_aiResources[resource] * gameConfig.AIResourceMultiplier);
        }
    }
    
    /// <summary>
    /// 决定主要目标
    /// </summary>
    private void DecideMainTarget()
    {
        // 基于多种因素的决策逻辑
        
        // 1. 如果基地数量很少，优先扩张
        if (_aiBases.Count < 3)
        {
            _currentMainTarget = AITargetType.Expansion;
            return;
        }
        
        // 2. 如果有基地受到威胁，优先防御
        var threatLevel = EvaluateThreats();
        if (threatLevel > 0.7f)
        {
            _currentMainTarget = AITargetType.Defense;
            return;
        }
        
        // 3. 如果有足够资源且攻击力足够，考虑进攻
        if (_aiUnits.Count > 8 && _aggressionLevel > 0.6f && _aiResources[ResourceType.Money] > 5000)
        {
            _currentMainTarget = AITargetType.Attack;
            return;
        }
        
        // 4. 否则研发新技术或继续扩张
        if (_techPriority > _expansionPriority && _aiResources[ResourceType.Money] > 3000)
        {
            _currentMainTarget = AITargetType.Research;
        }
        else
        {
            _currentMainTarget = AITargetType.Expansion;
        }
    }
    
    /// <summary>
    /// 评估威胁级别
    /// </summary>
    private float EvaluateThreats()
    {
        // 获取玩家单位分布
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var playerUnits = unitSystem.GetAllUnits().Where(u => !IsAIUnit(u)).ToList();
        
        // 计算对每个AI基地的威胁
        float maxThreat = 0f;
        
        foreach (var baseEntity in _aiBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null)
                continue;
            
            var basePos = baseComp.Position;
            
            // 计算附近玩家单位的威胁值
            float threatValue = 0f;
            foreach (var unit in playerUnits)
            {
                var posComp = unit.GetComponent<PositionComponent>();
                if (posComp == null)
                    continue;
                
                var unitPos = posComp.Position;
                float distance = HexUtils.Distance(basePos, unitPos);
                
                if (distance < 10f) // 威胁范围
                {
                    var statsComp = unit.GetComponent<UnitStatsComponent>();
                    if (statsComp != null)
                    {
                        // 威胁计算公式（单位战斗力/距离）
                        float unitThreat = CalculateUnitCombatPower(unit) / distance;
                        threatValue += unitThreat;
                    }
                }
            }
            
            maxThreat = Mathf.Max(maxThreat, threatValue);
            
            // 如果某个基地面临高威胁，记录为防御目标
            if (threatValue > 0.7f)
            {
                _targetEntityId = baseComp.BaseId;
                _targetPosition = basePos;
            }
        }
        
        return Mathf.Clamp01(maxThreat / 10f); // 归一化威胁值
    }
    
    /// <summary>
    /// 计算单位战斗力
    /// </summary>
    private float CalculateUnitCombatPower(GameEntity unit)
    {
        var statsComp = unit.GetComponent<UnitStatsComponent>();
        if (statsComp == null)
            return 0f;
        
        // 简化的战斗力计算
        float power = statsComp.MaxHealth / 100f;
        
        // 考虑武器威力
        var weapons = unit.GetComponents<WeaponComponent>();
        foreach (var weapon in weapons)
        {
            power += weapon.BaseDamage / 50f;
        }
        
        // 考虑驾驶员因素
        var pilotComp = unit.GetComponent<PilotComponent>();
        if (pilotComp != null && pilotComp.PilotId != -1)
        {
            power *= pilotComp.CompatibilityRate;
        }
        
        return power;
    }
    
    /// <summary>
    /// 执行扩张策略
    /// </summary>
    private void PerformExpansion()
    {
        // 寻找适合建立新基地的位置
        var bestLocation = FindOptimalExpansionLocation();
        
        // 如果找到合适位置且资源足够，建立新基地
        if (bestLocation != Vector2Int.zero && 
            _aiResources[ResourceType.Money] >= 2000 && 
            _aiResources[ResourceType.StandardAlloy] >= 100)
        {
            // 消耗资源
            _aiResources[ResourceType.Money] -= 2000;
            _aiResources[ResourceType.StandardAlloy] -= 100;
            
            // 确定基地类型（根据当前需求）
            BaseType baseType = DetermineBaseType();
            
            // 创建基地
            var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
            var baseEntity = baseSystem.CreateBase("AI Base " + _aiBases.Count, baseType, bestLocation);
            
            // 添加到AI基地列表
            _aiBases.Add(baseEntity);
            
            // 触发事件
            EventManager.TriggerEvent(new AIBaseConstructedEvent
            {
                BaseId = baseEntity.GetComponent<BaseComponent>().BaseId,
                Position = bestLocation,
                BaseType = baseType
            });
        }
    }
    
    /// <summary>
    /// 寻找最佳扩张位置
    /// </summary>
    private Vector2Int FindOptimalExpansionLocation()
    {
        // 游戏地图大小（假设为100x100的网格）
        int mapSize = 100;
        
        // 获取所有基地位置（包括玩家和AI）
        List<Vector2Int> allBasePositions = new List<Vector2Int>();
        
        // 添加AI基地位置
        foreach (var baseEntity in _aiBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null)
            {
                allBasePositions.Add(baseComp.Position);
            }
        }
        
        // 添加玩家基地位置
        var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
        foreach (var baseEntity in baseSystem.GetAllBases())
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null && !_aiBases.Contains(baseEntity))
            {
                allBasePositions.Add(baseComp.Position);
            }
        }
        
        // 候选位置评分
        Dictionary<Vector2Int, float> locationScores = new Dictionary<Vector2Int, float>();
        
        // 在地图上选择一些候选点进行评估
        int candidateCount = 20;
        List<Vector2Int> candidateLocations = new List<Vector2Int>();
        
        for (int i = 0; i < candidateCount; i++)
        {
            Vector2Int pos = new Vector2Int(
                UnityEngine.Random.Range(5, mapSize - 5),
                UnityEngine.Random.Range(5, mapSize - 5)
            );
            candidateLocations.Add(pos);
        }
        
        // 评估每个候选位置
        foreach (var pos in candidateLocations)
        {
            float score = 0f;
            
            // 1. 远离现有基地（避免过于集中）
            float minDistanceToBase = float.MaxValue;
            foreach (var basePos in allBasePositions)
            {
                float distance = HexUtils.Distance(pos, basePos);
                minDistanceToBase = Mathf.Min(minDistanceToBase, distance);
            }
            score += minDistanceToBase * 0.5f;
            
            // 2. 远离地图边缘
            float distanceToEdge = Mathf.Min(
                pos.x, pos.y, mapSize - pos.x, mapSize - pos.y
            );
            score += distanceToEdge * 0.3f;
            
            // 3. 随机因素（避免总是选择相同位置）
            score += UnityEngine.Random.Range(0f, 10f);
            
            locationScores[pos] = score;
        }
        
        // 选择评分最高的位置
        if (locationScores.Count > 0)
        {
            return locationScores.OrderByDescending(kvp => kvp.Value).First().Key;
        }
        
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 确定新基地类型
    /// </summary>
    private BaseType DetermineBaseType()
    {
        // 统计现有基地类型
        Dictionary<BaseType, int> baseCounts = new Dictionary<BaseType, int>();
        foreach (BaseType type in Enum.GetValues(typeof(BaseType)))
        {
            baseCounts[type] = 0;
        }
        
        foreach (var baseEntity in _aiBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null)
            {
                baseCounts[baseComp.BaseType]++;
            }
        }
        
        // 如果没有总部，优先建立总部
        if (baseCounts[BaseType.Headquarters] == 0)
        {
            return BaseType.Headquarters;
        }
        
        // 如果研究设施不足，优先建立研究设施
        if (baseCounts[BaseType.ResearchFacility] < 2 && _techPriority > 0.5f)
        {
            return BaseType.ResearchFacility;
        }
        
        // 如果防御设施不足，考虑建立防御哨站
        if (baseCounts[BaseType.DefenseOutpost] < 2 && _defensePriority > 0.6f)
        {
            return BaseType.DefenseOutpost;
        }
        
        // 如果资源矿场不足，考虑建立资源矿场
        if (baseCounts[BaseType.ResourceMine] < 3)
        {
            return BaseType.ResourceMine;
        }
        
        // 默认建立生产工厂
        return BaseType.ProductionPlant;
    }
    
    /// <summary>
    /// 执行攻击策略
    /// </summary>
    private void PerformAttack()
    {
        // 选择攻击目标（玩家基地）
        var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
        var playerBases = baseSystem.GetAllBases().Where(b => !_aiBases.Contains(b)).ToList();
        
        if (playerBases.Count == 0)
            return;
        
        // 选择最脆弱或最有价值的基地作为目标
        GameEntity targetBase = null;
        float bestScore = 0f;
        
        foreach (var baseEntity in playerBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null)
                continue;
            
            // 评分标准：基地价值/防御强度
            float defenseStrength = baseComp.DefenseStrength + 10f;
            float baseValue = 0f;
            
            // 不同类型基地有不同价值
            switch (baseComp.BaseType)
            {
                case BaseType.Headquarters:
                    baseValue = 100f;
                    break;
                case BaseType.ProductionPlant:
                    baseValue = 80f;
                    break;
                case BaseType.ResearchFacility:
                    baseValue = 70f;
                    break;
                case BaseType.ResourceMine:
                    baseValue = 60f;
                    break;
                case BaseType.DefenseOutpost:
                    baseValue = 40f;
                    break;
            }
            
            // 考虑基地健康状况
            baseValue *= (float)baseComp.Health / baseComp.MaxHealth;
            
            float score = baseValue / defenseStrength;
            
            // 考虑距离因素
            var aiBasePositions = _aiBases.Select(b => b.GetComponent<BaseComponent>().Position).ToList();
            float minDistance = aiBasePositions.Count > 0 
                ? aiBasePositions.Min(p => HexUtils.Distance(p, baseComp.Position)) 
                : 0f;
            
            // 距离越近，评分越高
            if (minDistance > 0)
            {
                score *= (100f / minDistance);
            }
            
            if (score > bestScore)
            {
                bestScore = score;
                targetBase = baseEntity;
            }
        }
        
        if (targetBase != null)
        {
            var baseComp = targetBase.GetComponent<BaseComponent>();
            _targetPosition = baseComp.Position;
            _targetEntityId = baseComp.BaseId;
            
            // 触发AI攻击事件
            EventManager.TriggerEvent(new AIAttackInitiatedEvent
            {
                TargetBaseId = baseComp.BaseId,
                TargetPosition = baseComp.Position,
                AttackingUnitIds = SelectUnitsForAttack().Select(u => u.EntityId.ToString()).ToList()
            });
        }
    }
    
    /// <summary>
    /// 选择攻击单位
    /// </summary>
    private List<GameEntity> SelectUnitsForAttack()
    {
        // 选择适合攻击的单位
        List<GameEntity> attackForce = new List<GameEntity>();
        
        // 按战斗力排序
        var sortedUnits = _aiUnits.OrderByDescending(u => CalculateUnitCombatPower(u)).ToList();
        
        // 选择前60%的单位进行攻击
        int attackForceSize = Mathf.Max(1, Mathf.FloorToInt(sortedUnits.Count * 0.6f));
        for (int i = 0; i < attackForceSize && i < sortedUnits.Count; i++)
        {
            attackForce.Add(sortedUnits[i]);
        }
        
        return attackForce;
    }
    
    /// <summary>
    /// 执行防御策略
    /// </summary>
    private void PerformDefense()
    {
        // 在EvaluateThreats中已设置了防御目标
        if (_targetEntityId == 0)
            return;
        
        // 将单位移动到受威胁的基地附近
        var defendingUnits = SelectUnitsForDefense();
        
        // 触发AI防御事件
        EventManager.TriggerEvent(new AIDefenseInitiatedEvent
        {
            BaseToDefendId = _targetEntityId,
            DefensePosition = _targetPosition,
            DefendingUnitIds = defendingUnits.Select(u => u.EntityId.ToString()).ToList()
        });
        
        // 如果资源足够，在受威胁基地建设防御设施
        DefendBaseWithFacilities(_targetEntityId);
    }
    
    /// <summary>
    /// 选择防御单位
    /// </summary>
    private List<GameEntity> SelectUnitsForDefense()
    {
        // 选择适合防御的单位
        List<GameEntity> defenseForce = new List<GameEntity>();
        
        // 按与目标基地的距离排序
        var sortedUnits = _aiUnits.OrderBy(u => {
            var posComp = u.GetComponent<PositionComponent>();
            return posComp != null ? HexUtils.Distance(posComp.Position, _targetPosition) : float.MaxValue;
        }).ToList();
        
        // 选择前80%的单位进行防御
        int defenseForceSize = Mathf.Max(1, Mathf.FloorToInt(sortedUnits.Count * 0.8f));
        for (int i = 0; i < defenseForceSize && i < sortedUnits.Count; i++)
        {
            defenseForce.Add(sortedUnits[i]);
        }
        
        return defenseForce;
    }

    // 接着上面的AISystem类中的DefendBaseWithFacilities方法
    private void DefendBaseWithFacilities(int baseId)
    {
        if (_aiResources[ResourceType.Money] < 1000 || _aiResources[ResourceType.StandardAlloy] < 50)
            return;
        
        var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
        var baseEntity = baseSystem.GetBase(baseId);
        if (baseEntity == null)
            return;
        
        var baseComp = baseEntity.GetComponent<BaseComponent>();
        if (baseComp == null || !baseComp.IsOperational)
            return;
        
        // 检查现有设施数量
        if (baseComp.Facilities.Count >= baseComp.Level + 2)
            return; // 已达到最大设施数量
        
        // 检查是否已有防御设施
        int defenseTurretCount = baseComp.Facilities.Count(f => f.FacilityType == FacilityType.DefenseTurret);
        
        // 如果防御设施不足，建造防御炮塔
        if (defenseTurretCount < 2)
        {
            // 创建临时防御炮塔设施
            Facility defenseTurret = new Facility
            {
                FacilityId = "defense_turret_" + Guid.NewGuid().ToString().Substring(0, 8),
                FacilityName = "Defense Turret",
                FacilityType = FacilityType.DefenseTurret,
                EffectValue = 15, // 防御力加成
                BuildCost = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Money, 1000 },
                    { ResourceType.StandardAlloy, 50 }
                }
            };
            
            // 消耗资源
            _aiResources[ResourceType.Money] -= 1000;
            _aiResources[ResourceType.StandardAlloy] -= 50;
            
            // 添加设施
            baseComp.AddFacility(defenseTurret);
            
            // 触发设施建造事件
            EventManager.TriggerEvent(new AIFacilityConstructedEvent
            {
                BaseId = baseId,
                FacilityId = defenseTurret.FacilityId,
                FacilityType = FacilityType.DefenseTurret
            });
        }
    }
    
    /// <summary>
    /// 执行研究策略
    /// </summary>
    private void PerformResearch()
    {
        // 确定研究方向
        string techToResearch = DecideResearchTech();
        
        if (string.IsNullOrEmpty(techToResearch))
            return;
        
        // 消耗资源启动研究项目
        Dictionary<ResourceType, int> researchCost = new Dictionary<ResourceType, int>
        {
            { ResourceType.Money, 1500 },
            { ResourceType.StandardAlloy, 50 }
        };
        
        // 高级技术可能需要更多资源
        var techSystem = SystemManager.GetSystem<TechTreeSystem>();
        if (techSystem.GetTechLevel(techToResearch) > 2)
        {
            researchCost[ResourceType.RareMetal] = 30;
            
            if (techSystem.GetTechLevel(techToResearch) > 3)
            {
                researchCost[ResourceType.EnergyCrystal] = 20;
            }
        }
        
        // 检查资源是否足够
        foreach (var resource in researchCost)
        {
            if (_aiResources[resource.Key] < resource.Value)
                return;
        }
        
        // 消耗资源
        foreach (var resource in researchCost)
        {
            _aiResources[resource.Key] -= resource.Value;
        }
        
        // 触发AI研究事件
        EventManager.TriggerEvent(new AIResearchStartedEvent
        {
            TechId = techToResearch,
            EstimatedTurns = techSystem.GetTechLevel(techToResearch) * 2
        });
    }
    
    /// <summary>
    /// 决定研究方向
    /// </summary>
    private string DecideResearchTech()
    {
        var techSystem = SystemManager.GetSystem<TechTreeSystem>();
        var availableTechs = techSystem.GetResearchableTechs();
        
        if (availableTechs.Count == 0)
            return null;
        
        // 根据当前战略需求选择技术
        List<string> priorityTechs = new List<string>();
        
        // 如果防御压力大，优先防御技术
        if (_defensePriority > 0.7f)
        {
            priorityTechs.AddRange(availableTechs.Where(t => t.Contains("defense") || t.Contains("shield")));
        }
        
        // 如果进攻倾向强，优先攻击技术
        if (_aggressionLevel > 0.7f)
        {
            priorityTechs.AddRange(availableTechs.Where(t => t.Contains("weapon") || t.Contains("attack")));
        }
        
        // 如果扩张倾向强，优先生产和资源技术
        if (_expansionPriority > 0.7f)
        {
            priorityTechs.AddRange(availableTechs.Where(t => t.Contains("production") || t.Contains("resource")));
        }
        
        // 如果有优先技术，从中随机选择一个
        if (priorityTechs.Count > 0)
        {
            return priorityTechs[UnityEngine.Random.Range(0, priorityTechs.Count)];
        }
        
        // 否则从所有可研究技术中随机选择
        return availableTechs[UnityEngine.Random.Range(0, availableTechs.Count)];
    }
    
    /// <summary>
    /// 管理AI单位生产
    /// </summary>
    private void ManageProduction()
    {
        // 计算需要生产的单位类型
        Dictionary<UnitType, int> desiredUnits = new Dictionary<UnitType, int>();
        
        // 基础单位需求
        desiredUnits[UnitType.Tank] = 5;
        desiredUnits[UnitType.Aircraft] = 3;
        desiredUnits[UnitType.MassProdRobot] = 6;
        
        // 高级单位需求（根据当前回合数增加）
        int currentTurn = GameManager.Instance.CurrentTurn;
        if (currentTurn > 20)
        {
            desiredUnits[UnitType.Gundam] = 1;
            desiredUnits[UnitType.SolarPowered] = 1;
        }
        if (currentTurn > 40)
        {
            desiredUnits[UnitType.Gundam] = 2;
            desiredUnits[UnitType.SolarPowered] = 2;
            desiredUnits[UnitType.Getter] = 1;
        }
        if (currentTurn > 60)
        {
            desiredUnits[UnitType.Gundam] = 3;
            desiredUnits[UnitType.SolarPowered] = 3;
            desiredUnits[UnitType.Getter] = 2;
            desiredUnits[UnitType.BioAdaptive] = 1;
        }
        
        // 统计当前单位数量
        Dictionary<UnitType, int> currentUnits = new Dictionary<UnitType, int>();
        foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
        {
            currentUnits[type] = 0;
        }
        
        foreach (var unit in _aiUnits)
        {
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            if (statsComp != null)
            {
                currentUnits[statsComp.UnitType]++;
            }
        }
        
        // 找出需要生产的单位类型
        List<UnitType> unitsToProduce = new List<UnitType>();
        foreach (var unitType in desiredUnits.Keys)
        {
            if (currentUnits.TryGetValue(unitType, out int count) && count < desiredUnits[unitType])
            {
                // 需要生产的数量
                int needToProduce = desiredUnits[unitType] - count;
                for (int i = 0; i < needToProduce; i++)
                {
                    unitsToProduce.Add(unitType);
                }
            }
        }
        
        // 按优先级排序生产列表
        unitsToProduce.Sort((a, b) => {
            // 超级机器人优先级较低（除非回合数较高）
            bool aIsSuper = a == UnitType.Gundam || a == UnitType.SolarPowered || 
                           a == UnitType.Getter || a == UnitType.BioAdaptive ||
                           a == UnitType.Transformer;
            
            bool bIsSuper = b == UnitType.Gundam || b == UnitType.SolarPowered || 
                           b == UnitType.Getter || b == UnitType.BioAdaptive ||
                           b == UnitType.Transformer;
            
            if (aIsSuper && !bIsSuper) return currentTurn > 40 ? -1 : 1;
            if (!aIsSuper && bIsSuper) return currentTurn > 40 ? 1 : -1;
            
            // 同类型按ID排序
            return a.CompareTo(b);
        });
        
        // 在有生产设施的基地生产单位
        foreach (var unitType in unitsToProduce)
        {
            ProductionItem.ProductionType prodType = ProductionItem.ProductionType.Unit;
            string templateId = GetUnitTemplateId(unitType);
            
            if (string.IsNullOrEmpty(templateId))
                continue;
            
            // 查找生产基地
            var baseEntity = FindProductionBase();
            if (baseEntity == null)
                continue;
            
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null || !baseComp.IsOperational)
                continue;
            
            // 检查生产队列长度
            if (baseComp.ProductionQueue.Count >= 5)
                continue;
            
            // 获取生产成本和时间
            var productionSystem = SystemManager.GetSystem<ProductionSystem>();
            var (cost, turns) = productionSystem.GetUnitProductionInfo(templateId);
            
            // 检查资源是否足够
            bool resourcesSufficient = true;
            foreach (var resource in cost)
            {
                if (_aiResources[resource.Key] < resource.Value)
                {
                    resourcesSufficient = false;
                    break;
                }
            }
            
            if (!resourcesSufficient)
                continue;
            
            // 创建生产项目
            var productionItem = productionSystem.CreateProductionItem(prodType, templateId);
            
            // 消耗资源
            foreach (var resource in cost)
            {
                _aiResources[resource.Key] -= resource.Value;
            }
            
            // 添加到生产队列
            baseComp.ProductionQueue.Add(productionItem);
            
            // 触发生产开始事件
            EventManager.TriggerEvent(new AIProductionStartedEvent
            {
                BaseId = baseComp.BaseId,
                ItemId = templateId,
                ItemType = prodType,
                TurnsRequired = turns
            });
        }
    }
    
    /// <summary>
    /// 获取单位模板ID
    /// </summary>
    private string GetUnitTemplateId(UnitType unitType)
    {
        // 简化版，真实实现需要从数据库中查找
        switch (unitType)
        {
            case UnitType.Tank: return "tank_basic";
            case UnitType.Aircraft: return "aircraft_basic";
            case UnitType.Ship: return "ship_basic";
            case UnitType.MassProdRobot: return "mass_robot_basic";
            case UnitType.Gundam: return "gundam_basic";
            case UnitType.SolarPowered: return "mazinger_basic";
            case UnitType.Getter: return "getter_basic";
            case UnitType.BioAdaptive: return "eva_basic";
            case UnitType.Transformer: return "transformer_basic";
            default: return null;
        }
    }
    
    /// <summary>
    /// 查找可用的生产基地
    /// </summary>
    private GameEntity FindProductionBase()
    {
        // 优先使用生产工厂类型的基地
        var productionPlants = _aiBases.Where(b => {
            var baseComp = b.GetComponent<BaseComponent>();
            return baseComp != null && 
                   baseComp.IsOperational && 
                   baseComp.BaseType == BaseType.ProductionPlant &&
                   baseComp.ProductionQueue.Count < 5;
        }).ToList();
        
        if (productionPlants.Count > 0)
        {
            return productionPlants[UnityEngine.Random.Range(0, productionPlants.Count)];
        }
        
        // 其次使用总部
        var headquarters = _aiBases.Where(b => {
            var baseComp = b.GetComponent<BaseComponent>();
            return baseComp != null && 
                   baseComp.IsOperational && 
                   baseComp.BaseType == BaseType.Headquarters &&
                   baseComp.ProductionQueue.Count < 5;
        }).ToList();
        
        if (headquarters.Count > 0)
        {
            return headquarters[UnityEngine.Random.Range(0, headquarters.Count)];
        }
        
        return null;
    }
    
    /// <summary>
    /// 移动AI单位
    /// </summary>
    private void MoveUnits()
    {
        // 根据当前目标移动单位
        switch (_currentMainTarget)
        {
            case AITargetType.Attack:
                MoveUnitsToTarget(SelectUnitsForAttack(), _targetPosition);
                break;
            case AITargetType.Defense:
                MoveUnitsToTarget(SelectUnitsForDefense(), _targetPosition);
                break;
            case AITargetType.Expansion:
                // 将单位分散到各个基地附近进行巡逻
                DistributeUnitsForPatrol();
                break;
            case AITargetType.Research:
                // 研发阶段保持防御态势
                DistributeUnitsForDefense();
                break;
        }
    }
    
    /// <summary>
    /// 将单位移动到目标位置
    /// </summary>
    private void MoveUnitsToTarget(List<GameEntity> units, Vector2Int targetPosition)
    {
        // 尝试为每个单位找到接近目标但不重叠的位置
        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            var posComp = unit.GetComponent<PositionComponent>();
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            
            if (posComp == null || statsComp == null)
                continue;
            
            // 计算到目标的方向
            Vector2 direction = targetPosition - posComp.Position;
            float distance = direction.magnitude;
            
            // 如果已经足够接近目标，不再移动
            if (distance < 3f)
                continue;
            
            // 归一化方向
            if (distance > 0)
                direction /= distance;
            
            // 计算每个单位的稍微不同的目标位置（避免堆叠）
            Vector2 offset = new Vector2(
                UnityEngine.Random.Range(-2f, 2f),
                UnityEngine.Random.Range(-2f, 2f)
            );
            
            // 限制移动距离不超过单位的移动范围
            float moveDistance = Mathf.Min(statsComp.MovementRange, distance);
            
            // 计算新位置
            Vector2 newPosition = posComp.Position + direction * moveDistance + offset;
            
            // 确保新位置在地图范围内
            newPosition.x = Mathf.Clamp(newPosition.x, 0, 99);
            newPosition.y = Mathf.Clamp(newPosition.y, 0, 99);
            
            // 更新单位位置
            posComp.SetPosition(new Vector2Int(
                Mathf.RoundToInt(newPosition.x),
                Mathf.RoundToInt(newPosition.y)
            ));
            
            // 触发单位移动事件
            EventManager.TriggerEvent(new AIUnitMovedEvent
            {
                UnitId       = unit.EntityId,
                FromPosition = posComp.PreviousPosition,
                ToPosition   = posComp.Position
            });
        }
    }
    
    /// <summary>
    /// 分散单位进行巡逻
    /// </summary>
    private void DistributeUnitsForPatrol()
    {
        // 收集所有AI基地位置
        List<Vector2Int> basePositions = new List<Vector2Int>();
        foreach (var baseEntity in _aiBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null && baseComp.IsOperational)
            {
                basePositions.Add(baseComp.Position);
            }
        }
        
        if (basePositions.Count == 0)
            return;
        
        // 将单位分配到各个基地进行巡逻
        for (int i = 0; i < _aiUnits.Count; i++)
        {
            // 循环分配到各个基地
            Vector2Int patrolCenter = basePositions[i % basePositions.Count];
            
            // 在基地周围随机巡逻点
            Vector2Int patrolPoint = new Vector2Int(
                patrolCenter.x + UnityEngine.Random.Range(-5, 6),
                patrolCenter.y + UnityEngine.Random.Range(-5, 6)
            );
            
            // 确保巡逻点在地图范围内
            patrolPoint.x = Mathf.Clamp(patrolPoint.x, 0, 99);
            patrolPoint.y = Mathf.Clamp(patrolPoint.y, 0, 99);
            
            // 移动单位到巡逻点
            var unit = _aiUnits[i];
            var posComp = unit.GetComponent<PositionComponent>();
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            
            if (posComp == null || statsComp == null)
                continue;
            
            // 计算到巡逻点的方向
            Vector2 direction = patrolPoint - posComp.Position;
            float distance = direction.magnitude;
            
            // 如果已经在巡逻点附近，不移动
            if (distance < 2f)
                continue;
            
            // 归一化方向
            if (distance > 0)
                direction /= distance;
            
            // 限制移动距离不超过单位的移动范围
            float moveDistance = Mathf.Min(statsComp.MovementRange, distance);
            
            // 计算新位置
            Vector2 newPosition = posComp.Position + direction * moveDistance;
            
            // 更新单位位置
            posComp.SetPosition(new Vector2Int(
                Mathf.RoundToInt(newPosition.x),
                Mathf.RoundToInt(newPosition.y)
            ));
            
            // 触发单位移动事件
            EventManager.TriggerEvent(new AIUnitMovedEvent
            {
                UnitId = unit.EntityId,
                FromPosition = posComp.PreviousPosition,
                ToPosition = posComp.Position
            });
        }
    }
    
    /// <summary>
    /// 分散单位进行防御
    /// </summary>
    private void DistributeUnitsForDefense()
    {
        // 收集所有AI基地位置
        Dictionary<Vector2Int, int> baseData = new Dictionary<Vector2Int, int>();
        foreach (var baseEntity in _aiBases)
        {
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null && baseComp.IsOperational)
            {
                // 记录基地位置和重要性（以防御强度的倒数作为权重）
                float importance = 1f / (baseComp.DefenseStrength + 10f);
                baseData[baseComp.Position] = Mathf.CeilToInt(importance * 10);
            }
        }
        
        if (baseData.Count == 0)
            return;
        
        // 创建加权位置列表
        List<Vector2Int> weightedPositions = new List<Vector2Int>();
        foreach (var basePos in baseData.Keys)
        {
            // 根据重要性添加多次
            for (int i = 0; i < baseData[basePos]; i++)
            {
                weightedPositions.Add(basePos);
            }
        }
        
        // 将单位分配到各个基地进行防御
        for (int i = 0; i < _aiUnits.Count; i++)
        {
            // 随机选择一个基地（权重高的更可能被选中）
            Vector2Int defenseCenter = weightedPositions[UnityEngine.Random.Range(0, weightedPositions.Count)];
            
            // 在基地周围随机防御点
            Vector2Int defensePoint = new Vector2Int(
                defenseCenter.x + UnityEngine.Random.Range(-3, 4),
                defenseCenter.y + UnityEngine.Random.Range(-3, 4)
            );
            
            // 确保防御点在地图范围内
            defensePoint.x = Mathf.Clamp(defensePoint.x, 0, 99);
            defensePoint.y = Mathf.Clamp(defensePoint.y, 0, 99);
            
            // 移动单位到防御点
            var unit = _aiUnits[i];
            var posComp = unit.GetComponent<PositionComponent>();
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            
            if (posComp == null || statsComp == null)
                continue;
            
            // 计算到防御点的方向
            Vector2 direction = defensePoint - posComp.Position;
            float distance = direction.magnitude;
            
            // 如果已经在防御点附近，不移动
            if (distance < 2f)
                continue;
            
            // 归一化方向
            if (distance > 0)
                direction /= distance;
            
            // 限制移动距离不超过单位的移动范围
            float moveDistance = Mathf.Min(statsComp.MovementRange, distance);
            
            // 计算新位置
            Vector2 newPosition = posComp.Position + direction * moveDistance;
            
            // 更新单位位置
            posComp.SetPosition(new Vector2Int(
                Mathf.RoundToInt(newPosition.x),
                Mathf.RoundToInt(newPosition.y)
            ));
            
            // 触发单位移动事件
            EventManager.TriggerEvent(new AIUnitMovedEvent
            {
                UnitId = unit.EntityId,
                FromPosition = posComp.PreviousPosition,
                ToPosition = posComp.Position
            });
        }
    }
    
    /// <summary>
    /// 判断单位是否属于AI
    /// </summary>
    private bool IsAIUnit(GameEntity unit)
    {
        return _aiUnits.Contains(unit);
    }
    
    /// <summary>
    /// 事件处理程序
    /// </summary>
    private void OnTurnPhaseChanged(TurnPhaseChangedEvent evt)
    {
        // 在AI阶段执行AI行动
        if (evt.NewPhase == TurnPhase.Enemy)
        {
            ProcessAITurn();
        }
    }
    
    private void OnAIBaseDestroyed(AIBaseDestroyedEvent evt)
    {
        // 从AI基地列表中移除
        var baseToRemove = _aiBases.FirstOrDefault(b => {
            var baseComp = b.GetComponent<BaseComponent>();
            return baseComp != null && baseComp.BaseId == evt.BaseId;
        });
        
        if (baseToRemove != null)
        {
            _aiBases.Remove(baseToRemove);
        }
        
        // 提高攻击性（反击倾向）
        _aggressionLevel = Mathf.Min(_aggressionLevel + 0.1f, 1.0f);
    }
    
    private void OnAIUnitDestroyed(AIUnitDestroyedEvent evt)
    {
        // 从AI单位列表中移除
        var unitToRemove = _aiUnits.FirstOrDefault(u => {
            var statsComp = u.GetComponent<UnitStatsComponent>();
            return statsComp != null && u.EntityId == evt.UnitId;
        });
        
        if (unitToRemove != null)
        {
            _aiUnits.Remove(unitToRemove);
        }
    }
    
    public override void Cleanup()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<TurnPhaseChangedEvent>(OnTurnPhaseChanged);
        EventManager.Instance.Unsubscribe<AIBaseDestroyedEvent>(OnAIBaseDestroyed);
        EventManager.Instance.Unsubscribe<AIUnitDestroyedEvent>(OnAIUnitDestroyed);
    }
}
}
