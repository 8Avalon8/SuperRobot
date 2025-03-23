using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperRobot
{
    public class BattleSystem : GameSystem
{
    private BattleState _currentBattle;
    private Queue<BattleAction> _actionQueue = new Queue<BattleAction>();
    
    // 战斗地图信息
    private Dictionary<Vector2Int, TerrainType> _terrainMap = new Dictionary<Vector2Int, TerrainType>();
    private Dictionary<Vector2Int, List<GameEntity>> _unitPositions = new Dictionary<Vector2Int, List<GameEntity>>();
    
    public override void Initialize()
    {
        EventManager.Subscribe<BattleStartEvent>(OnBattleStart);
        EventManager.Subscribe<BattleEndEvent>(OnBattleEnd);
        EventManager.Subscribe<UnitActionRequestEvent>(OnUnitActionRequest);
    }

    public class BattleState
    {
        public string BattleId;
        public string MapId;
        public List<int> PlayerUnits;
        public List<int> EnemyUnits;
        public int CurrentTurn;
        public BattlePhase CurrentPhase;
        public List<int> DeployedUnits;
        public Dictionary<Vector2Int, List<GameEntity>> UnitPositions;
        // 地图数据
        public Dictionary<Vector2Int, TerrainType> MapData;
    }

    public class BattleAction
    {
        public int UnitId;
        public Vector2Int TargetPosition;
        public BattleActionType ActionType;
        public int AttackerUnitId;
        public int TargetUnitId;
        public int WeaponIndex;
        public bool IsHit;
        public int Damage;
    }

    public enum BattleActionType
    {
        Move,
        Attack,
        Build,
        Upgrade
    }

    public enum BattlePhase
    {
        PlayerDeployment,
        PlayerTurn,
        EnemyTurn,
        BattleEnd
    }
    
    
    
    private void OnBattleStart(BattleStartEvent evt)
    {
        // 初始化战斗状态
        _currentBattle = new BattleState
        {
            BattleId = evt.BattleId,
            MapData = LoadBattleMap(evt.MapId),
            PlayerUnits = evt.PlayerUnits,
            EnemyUnits = evt.EnemyUnits,
            CurrentTurn = 1,
            CurrentPhase = BattlePhase.PlayerDeployment
        };
        
        // 初始化地形图
        InitializeTerrainMap(_currentBattle.MapData);
        
        // 进入部署阶段
        StartDeploymentPhase();
    }

    // LoadBattleMap
    private Dictionary<Vector2Int, TerrainType> LoadBattleMap(string mapId)
    {
        // 从地图数据库中加载地图数据
        return new Dictionary<Vector2Int, TerrainType>();
    }

    // InitializeTerrainMap
    private void InitializeTerrainMap(Dictionary<Vector2Int, TerrainType> terrainMap)
    {
        // 初始化地形图
        _terrainMap = terrainMap;
    }
    

    private void OnBattleEnd(BattleEndEvent evt)
    {
        // 处理战斗结束逻辑
        // 例如：更新玩家资源、显示胜利/失败界面等
        // 触发胜利/失败事件
        EventManager.TriggerEvent(new BattleEndedEvent
        {
            BattleId = _currentBattle.BattleId,
            Victory = evt.Victory
        });
    }

    private void OnUnitActionRequest(UnitActionRequestEvent evt)
    {
        // 处理单位行动请求
        // 例如：移动单位、攻击目标等
        // 触发单位行动事件
        EventManager.TriggerEvent(new UnitActionCompletedEvent
        {
            UnitId = evt.UnitId,
            ActionType = evt.ActionType,
            TargetPosition = evt.TargetPosition
        });
    }
    
    
    
    
    private void StartDeploymentPhase()
    {
        // 设置部署区域
        HighlightDeploymentZone();
        
        // 通知UI系统显示部署界面
        EventManager.TriggerEvent(new ShowDeploymentUIEvent
        {
            AvailableUnits = _currentBattle.PlayerUnits,
            DeploymentZones = GetDeploymentZones()
        });
    }

    // GetDeploymentZones
    private List<Vector2Int> GetDeploymentZones()
    {
        // 获取部署区域
        return new List<Vector2Int>();
    }
    
    
    public void DeployUnit(int unitId, Vector2Int position)
    {
        if (_currentBattle.CurrentPhase != BattlePhase.PlayerDeployment)
            return;
            
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var unit = unitSystem.GetUnit(unitId);
        
        if (unit == null || !IsValidDeployPosition(position, unit))
            return;
            
        // 设置单位位置
        var posComp = unit.GetComponent<PositionComponent>();
        posComp.Position = position;
        
        // 更新单位位置字典
        if (!_unitPositions.ContainsKey(position))
            _unitPositions[position] = new List<GameEntity>();
            
        _unitPositions[position].Add(unit);
        
        // 标记单位已部署
        _currentBattle.DeployedUnits.Add(unitId);
        
        // 检查是否所有单位都已部署
        if (_currentBattle.DeployedUnits.Count == _currentBattle.PlayerUnits.Count)
        {
            // 自动进入下一阶段
            EndDeploymentPhase();
        }
    }

    // IsValidDeployPosition
    private bool IsValidDeployPosition(Vector2Int position, GameEntity unit)
    {
        // 检查是否可以部署单位
        return true;
    }
    
    private void EndDeploymentPhase()
    {
        _currentBattle.CurrentPhase = BattlePhase.PlayerTurn;
        
        // 部署敌方单位
        DeployEnemyUnits();
        
        // 开始玩家回合
        StartPlayerTurn();
    }

    // DeployEnemyUnits
    private void DeployEnemyUnits()
    {
        // 部署敌方单位
        // 例如：在地图上随机部署敌方单位
    }

    // HighlightDeploymentZone
    private void HighlightDeploymentZone()
    {
        // 高亮部署区域
        // 例如：在地图上显示可部署区域
    }
    
    
    private void StartPlayerTurn()
    {
        // 重置所有玩家单位的行动点
        ResetUnitActionPoints(_currentBattle.PlayerUnits);
        
        // 通知UI系统显示战斗控制界面
        EventManager.TriggerEvent(new BattleTurnStartEvent
        {
            IsPlayerTurn = true,
            TurnNumber = _currentBattle.CurrentTurn
        });
    }

    // ResetUnitActionPoints
    private void ResetUnitActionPoints(List<int> unitIds)
    {
        // 重置所有单位行动点
        foreach (var unitId in unitIds)
        {
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var unit = unitSystem.GetUnit(unitId);
            
            if (unit != null)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();
                if (statsComp != null)
                {
                    statsComp.CurrentActionPoints = statsComp.MaxActionPoints;
                }
            }
        }
    }
    
    public List<Vector2Int> GetMovementRange(int unitId)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var unit = unitSystem.GetUnit(unitId);
        
        if (unit == null)
            return new List<Vector2Int>();
            
        var posComp = unit.GetComponent<PositionComponent>();
        var statsComp = unit.GetComponent<UnitStatsComponent>();
        
        if (posComp == null || statsComp == null)
            return new List<Vector2Int>();
            
        int movementRange = statsComp.MovementRange;
        
        // 使用广度优先搜索计算可移动范围
        return CalculateMovementRange(posComp.Position, movementRange, unit);
    }
    
    public void ExecuteUnitMove(int unitId, Vector2Int targetPosition)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var unit = unitSystem.GetUnit(unitId);
        
        if (unit == null)
            return;
            
        var posComp = unit.GetComponent<PositionComponent>();
        var statsComp = unit.GetComponent<UnitStatsComponent>();
        
        if (posComp == null || statsComp == null)
            return;
            
        // 检查是否有足够的行动点
        if (statsComp.CurrentActionPoints < 1)
            return;
            
        // 检查目标位置是否在移动范围内
        var movementRange = GetMovementRange(unitId);
        if (!movementRange.Contains(targetPosition))
            return;
            
        // 从当前位置字典中移除
        _unitPositions[posComp.Position].Remove(unit);
        
        // 更新单位位置
        posComp.Position = targetPosition;
        
        // 添加到新位置
        if (!_unitPositions.ContainsKey(targetPosition))
            _unitPositions[targetPosition] = new List<GameEntity>();
            
        _unitPositions[targetPosition].Add(unit);
        
        // 消耗行动点
        statsComp.CurrentActionPoints--;
        
        // 触发移动完成事件
        EventManager.TriggerEvent(new UnitMovedEvent
        {
            UnitId = unitId,
            FromPosition = posComp.Position,
            ToPosition = targetPosition
        });
    }
    
    public List<GameEntity> GetUnitsInAttackRange(int unitId)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var unit       = unitSystem.GetUnit(unitId);
    
        if (unit == null)
            return new List<GameEntity>();
        
        var posComp = unit.GetComponent<PositionComponent>();
        var weapons = unit.GetComponents<WeaponComponent>();
    
        if (posComp == null || weapons.Count == 0)
            return new List<GameEntity>();
        
        // 获取最大射程
        int maxRange = weapons.Max(w => w.Range);
    
        // 获取范围内的所有位置
        var hexesInRange = HexUtils.GetHexesInRange(posComp.HexPosition, maxRange);
    
        // 获取范围内的所有敌方单位
        List<GameEntity> targetsInRange = new List<GameEntity>();
        foreach (var hex in hexesInRange)
        {
            var unitsAtHex = GetUnitsAtPosition(hex);
            foreach (var targetUnit in unitsAtHex)
            {
                if (IsEnemyUnit(unit, targetUnit))
                {
                    targetsInRange.Add(targetUnit);
                }
            }
        }
    
        return targetsInRange;
    }

    /// <summary>
    /// 获取指定位置的所有单位
    /// </summary>
    private List<GameEntity> GetUnitsAtPosition(HexCoord position)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var allUnits   = unitSystem.GetAllUnits();
    
        return allUnits.Where(u => {
            var posComp = u.GetComponent<PositionComponent>();
            return posComp != null && posComp.Position.Equals(position);
        }).ToList();
    }

    // CalculateAttackRange
    private List<Vector2Int> CalculateAttackRange(Vector2Int position, int maxRange)
    {
        // 计算攻击范围
        return new List<Vector2Int>();
    }

    // IsEnemyUnit
    private bool IsEnemyUnit(GameEntity unit, GameEntity targetUnit)
    {
        // 检查是否是敌方单位
        return false;
    }
    
    
    public void ExecuteAttack(int attackerUnitId, int targetUnitId, int weaponIndex)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var attacker = unitSystem.GetUnit(attackerUnitId);
        var target = unitSystem.GetUnit(targetUnitId);
        
        if (attacker == null || target == null)
            return;
            
        var attackerStats = attacker.GetComponent<UnitStatsComponent>();
        var attackerPos = attacker.GetComponent<PositionComponent>();
        var targetPos = target.GetComponent<PositionComponent>();
        
        if (attackerStats == null || attackerPos == null || targetPos == null)
            return;
            
        // 检查是否有足够的行动点
        if (attackerStats.CurrentActionPoints < 1)
            return;
            
        // 获取武器
        var weapons = attacker.GetComponents<WeaponComponent>();
        if (weaponIndex >= weapons.Count)
            return;
            
        var weapon = weapons[weaponIndex];
        
        // 检查射程
        int distance = CalculateDistance(attackerPos.Position, targetPos.Position);
        if (distance > weapon.Range)
            return;
            
        // 计算命中率
        float hitChance = CalculateHitChance(attacker, target, weapon);
        
        // 掷骰决定是否命中
        bool isHit = UnityEngine.Random.value <= hitChance;
        
        // 创建战斗动作
        BattleAction action = new BattleAction
        {
            ActionType = BattleActionType.Attack,
            AttackerUnitId = attackerUnitId,
            TargetUnitId = targetUnitId,
            WeaponIndex = weaponIndex,
            IsHit = isHit
        };
        
        if (isHit)
        {
            // 计算伤害
            int damage = CalculateDamage(attacker, target, weapon);
            action.Damage = damage;
        }
        
        // 添加到动作队列
        _actionQueue.Enqueue(action);
        
        // 消耗行动点
        attackerStats.CurrentActionPoints--;
        
        // 开始执行战斗动画
        ExecuteBattleAnimation(action);
    }

    //CalculateDistance
    private int CalculateDistance(Vector2Int position1, Vector2Int position2)
    {
        // 计算距离
        return 0;
    }

    // CalculateMovementRange
    private List<Vector2Int> CalculateMovementRange(Vector2Int startPosition, int movementRange, GameEntity unit)
    {
        // 计算移动范围
        return new List<Vector2Int>();
    }

    // CalculateHitChance
    private float CalculateHitChance(GameEntity attacker, GameEntity target, WeaponComponent weapon)
    {
        // 计算命中率
        return 1;
    }
    
    // CalculateDamage
    private int CalculateDamage(GameEntity attacker, GameEntity target, WeaponComponent weapon)
    {
        // 计算伤害
        return 1;
    }
    
    
    private void ExecuteBattleAnimation(BattleAction action)
    {
        // 通知UI系统播放战斗动画
        EventManager.TriggerEvent(new PlayBattleAnimationEvent
        {
            BattleAction = action,
            OnComplete = () => {
                // 动画完成后应用伤害
                ApplyBattleActionResults(action);
                
                // 处理下一个动作
                if (_actionQueue.Count > 0)
                {
                    ExecuteBattleAnimation(_actionQueue.Dequeue());
                }
                else
                {
                    // 所有动作执行完毕，检查战斗是否结束
                    CheckBattleEnd();
                }
            }
        });
    }
    
    private void ApplyBattleActionResults(BattleAction action)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        var target = unitSystem.GetUnit(action.TargetUnitId);
        
        if (target == null)
            return;
            
        var targetStats = target.GetComponent<UnitStatsComponent>();
        
        if (targetStats == null)
            return;
            
        if (action.IsHit)
        {
            // 应用伤害
            targetStats.CurrentHealth -= action.Damage;
            
            // 检查单位是否被击毁
            if (targetStats.CurrentHealth <= 0)
            {
                DestroyUnit(action.TargetUnitId);
            }
        }
    }

    private void DestroyUnit(int unitId)
    {
        var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
        //unitSystem.DestroyUnit(unitId);
    }
    
    private void CheckBattleEnd()
    {
        // 检查是否所有敌方单位都被消灭
        bool allEnemiesDefeated = true;
        foreach (var enemyId in _currentBattle.EnemyUnits)
        {
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var enemy = unitSystem.GetUnit(enemyId);
            
            if (enemy != null)
            {
                allEnemiesDefeated = false;
                break;
            }
        }
        
        if (allEnemiesDefeated)
        {
            // 玩家胜利
            EndBattle(true);
            return;
        }
        
        // 检查是否所有玩家单位都被消灭
        bool allPlayerUnitsDefeated = true;
        foreach (var playerId in _currentBattle.PlayerUnits)
        {
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var playerUnit = unitSystem.GetUnit(playerId);
            
            if (playerUnit != null)
            {
                allPlayerUnitsDefeated = false;
                break;
            }
        }
        
        if (allPlayerUnitsDefeated)
        {
            // 玩家失败
            EndBattle(false);
            return;
        }
    }

    private void EndBattle(bool victory)
    {
        // 处理战斗结束逻辑
        // 例如：更新玩家资源、显示胜利/失败界面等
        // 触发胜利/失败事件
        EventManager.TriggerEvent(new BattleEndedEvent
        {
            BattleId = _currentBattle.BattleId,
            Victory = victory
        });
    }
    
    

        public override void Execute()
        {
        }
    }
}
