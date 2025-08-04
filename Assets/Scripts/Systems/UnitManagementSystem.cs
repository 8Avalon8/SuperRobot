using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperRobot.Core;

namespace SuperRobot
{
    public class UnitManagementSystem : GameSystem
    {
        private Dictionary<int, GameEntity>      _units         = new Dictionary<int, GameEntity>();
        private Dictionary<string, UnitTemplate> _unitTemplates = new Dictionary<string, UnitTemplate>();
        private IMapManager                      _mapManager => GetService<IMapManagerService>()?.MapManager;

        public override void Initialize()
        {
            // 加载单位模板数据
            var unitData = Resources.Load<UnitDatabase>("Data/UnitDatabase");
            foreach (var template in unitData.UnitTemplates)
            {
                _unitTemplates[template.UnitId.ToString()] = template;
            }

            // 订阅事件
            EventManager.Subscribe<UnitProductionCompletedEvent>(OnUnitProductionCompleted);
            EventManager.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);
        }

        public IEnumerable<GameEntity> GetAllUnits()
        {
            return _units.Values;
        }

        public GameEntity GetUnit(int unitId)
        {
            _units.TryGetValue(unitId, out var unit);
            return unit;
        }

        public GameEntity CreateUnit(string templateId, Vector2Int position)
        {
            if (!_unitTemplates.TryGetValue(templateId, out var template))
                return null;

            // 创建单位实体
            var unitEntity = EntityManager.CreateEntity("Unit");

            // 使用组件工厂创建标准组件集合
            ComponentFactory.CreateUnitComponents(unitEntity, template);

            // 设置位置
            var positionComp = unitEntity.GetComponent<PositionComponent>();
            positionComp.SetPosition(position);

            _units[unitEntity.EntityId] = unitEntity;

            // 创建模型
            Debug.Log($"加载模型：{template.ModelPrefabPath}");
            var model         = Resources.Load<GameObject>($"{template.ModelPrefabPath}");
            var worldPosition = _mapManager.GetCellWorldPosition(position);
            var modelObj      = GameObject.Instantiate(model, worldPosition, Quaternion.identity);
            modelObj.transform.SetParent(unitEntity.transform);
            modelObj.transform.localScale = new Vector3(3f, 3f, 3f);
            modelObj.AddComponent<UnitVisualController>().Bind(unitEntity.EntityId);
            unitEntity.SetModel(modelObj.transform);

            // 触发单位创建事件
            EventManager.TriggerEvent(new UnitCreatedEvent { UnitId = unitEntity.EntityId.ToString() });

            return unitEntity;
        }

        public void AssignPilotToUnit(int unitId, int pilotId)
        {
            if (!_units.TryGetValue(unitId, out var unit))
                return;

            var pilotComp = unit.GetComponent<PilotComponent>();
            if (pilotComp == null)
                return;

            pilotComp.PilotId = pilotId;

            // 更新单位属性基于驾驶员
            RecalculateUnitStats(unit);
        }



        // OnUnitProductionCompleted
        private void OnUnitProductionCompleted(UnitProductionCompletedEvent evt)
        {
            var unit = CreateUnit(evt.UnitTemplateId, evt.Position);
        }

        // OnUnitDestroyed
        private void OnUnitDestroyed(UnitDestroyedEvent evt)
        {
            _units.Remove(evt.UnitId);
        }

        public override void Execute()
        {
            
        }

        /// <summary>
        /// 获取单位移动范围
        /// </summary>
        public List<Vector2Int> GetMovementRange(int unitId)
        {
            var unit = GetUnit(unitId);

            if (unit == null)
                return new List<Vector2Int>();

            var posComp   = unit.GetComponent<PositionComponent>();
            var statsComp = unit.GetComponent<UnitStatsComponent>();

            if (posComp == null || statsComp == null)
                return new List<Vector2Int>();

            int movementRange = statsComp.MovementRange;

            // 使用广度优先搜索计算可移动范围
            return CalculateMovementRange(posComp.Position, movementRange, unit);
        }

        /// <summary>
/// 计算单位移动范围
/// </summary>
private List<Vector2Int> CalculateMovementRange(Vector2Int startPos, int range, GameEntity unit)
{
    List<HexCoord> result = new List<HexCoord>();
    HashSet<HexCoord> visited = new HashSet<HexCoord>();
    Queue<(HexCoord, int)> queue = new Queue<(HexCoord, int)>();
    HexCoord startHex = HexGridConverter.ToHexCoord(startPos);
    queue.Enqueue((startHex, 0));
    visited.Add(startHex);
    
    while (queue.Count > 0)
    {
        var (pos, distance) = queue.Dequeue();
        result.Add(pos);
        
        if (distance < range)
        {
            // 枚举六个方向的相邻格子
            for (int i = 0; i < 6; i++)
            {
                HexCoord neighbor = HexUtils.Neighbor(pos, i);
                
                // 检查新位置是否有效且可移动
                if (!visited.Contains(neighbor) && IsPositionValidForMovement(neighbor, unit))
                {
                    queue.Enqueue((neighbor, distance + 1));
                    visited.Add(neighbor);
                }
            }
        }
    }
    
    // 移除起始位置
    result.Remove(startHex);
    
    return result.Select(h => HexGridConverter.ToVector2Int(h)).ToList();
}

/// <summary>
/// 检查位置是否可移动
/// </summary>
private bool IsPositionValidForMovement(HexCoord position, GameEntity unit)
{
    // 检查地图边界 (这里需要根据实际地图大小定义边界)
    if (position.q < 0 || position.r < 0 || position.q >= 100 || position.r >= 100)
        return false;
    
    // 检查是否有其他单位占据
    if (IsPositionOccupied(position))
        return false;
    
    // 检查地形是否可通行（根据单位类型）
    var statsComp = unit.GetComponent<UnitStatsComponent>();
    if (statsComp != null)
    {
        var terrainType = GetTerrainType(HexGridConverter.ToVector2Int(position));
        
        // 根据单位类型判断地形可通行性
        switch (statsComp.UnitType)
        {
            case UnitType.Aircraft:
                // 飞行单位可以通过除了太空之外的所有地形
                return terrainType != TerrainType.Space;
                
            case UnitType.Ship:
                // 船只只能在水域移动
                return terrainType == TerrainType.Water;
                
            default:
                // 其他单位不能通过水域和太空
                return terrainType != TerrainType.Water && terrainType != TerrainType.Space;
        }
    }
    
    return true;
}

/// <summary>
/// 获取地形类型
/// </summary>
private TerrainType GetTerrainType(Vector2Int position)
{
    // 简化版：假设有一个地形数据数组
    // 实际实现应该使用地图系统获取地形数据
    return TerrainType.Plain; // 默认平原
}

/// <summary>
/// 检查位置是否被占据
/// </summary>
private bool IsPositionOccupied(HexCoord position)
{
    foreach (var unit in _units.Values)
    {
        var posComp = unit.GetComponent<PositionComponent>();
        if (posComp != null && posComp.Position.Equals(position))
        {
            return true;
        }
    }
    
    return false;
}


        /// <summary>
        /// 获取区域内的所有单位
        /// </summary>
        public List<GameEntity> GetUnitsInArea(Vector2Int center, int radius)
        {
            List<GameEntity> unitsInArea = new List<GameEntity>();

            foreach (var unit in _units.Values)
            {
                var posComp = unit.GetComponent<PositionComponent>();
                if (posComp != null)
                {
                    float distance = HexUtils.Distance(center, posComp.Position);
                    if (distance <= radius)
                    {
                        unitsInArea.Add(unit);
                    }
                }
            }

            return unitsInArea;
        }

        /// <summary>
        /// 销毁单位
        /// </summary>
        public void DestroyUnit(int unitId)
        {
            if (!_units.TryGetValue(unitId, out var unit))
                return;

            // 触发单位销毁事件
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            EventManager.TriggerEvent(new UnitDestroyedEvent
            {
                UnitId   = unitId,
                UnitType = statsComp?.UnitType                              ?? UnitType.Tank,
                Position = unit.GetComponent<PositionComponent>()?.Position ?? Vector2Int.zero
            });

            // 从单位字典中移除
            _units.Remove(unitId);

            // 销毁实体
            EntityManager.DestroyEntity(unit.EntityId);
        }

        /// <summary>
        /// 重新计算单位属性
        /// </summary>
        public void RecalculateUnitStats(GameEntity unit)
        {
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            var pilotComp = unit.GetComponent<PilotComponent>();

            if (statsComp == null)
                return;

            // 重置到基础值
            statsComp.MovementBonus     = 0;
            statsComp.RangedAttackBonus = 0;
            statsComp.MeleeAttackBonus  = 0;

            // 如果有驾驶员，应用驾驶员加成
            if (pilotComp != null && pilotComp.PilotId != -1)
            {
                var pilotSystem = SystemManager.GetSystem<PilotManagementSystem>();
                var pilot       = pilotSystem.GetPilot(pilotComp.PilotId);

                if (pilot != null)
                {
                    var pilotStats = pilot.GetComponent<PilotStatsComponent>();
                    if (pilotStats != null)
                    {
                        // 应用基础加成
                        statsComp.MovementBonus     += (int)(pilotStats.Reaction * 0.5f);
                        statsComp.RangedAttackBonus += (int)(pilotStats.Shooting * 0.75f);
                        statsComp.MeleeAttackBonus  += (int)(pilotStats.Melee    * 0.75f);

                        // 应用兼容性加成
                        float compatBonus = (pilotComp.CompatibilityRate - 1.0f) * 100f;
                        statsComp.MovementBonus     += (int)(compatBonus);
                        statsComp.RangedAttackBonus += (int)(compatBonus * 1.5f);
                        statsComp.MeleeAttackBonus  += (int)(compatBonus * 1.5f);

                        // NT能力加成
                        if (pilotStats.NTLevel > 0)
                        {
                            var superComp = unit.GetComponent<SuperRobotComponent>();
                            if (superComp != null)
                            {
                                superComp.NTBonus = pilotStats.NTLevel * 5;
                            }
                        }
                    }
                }
            }

            // 应用装备加成
            // ...

            // 应用临时效果
            var effects = unit.GetComponents<TemporaryEffectComponent>();
            foreach (var effect in effects)
            {
                if (effect.EffectType == EffectType.Boost)
                {
                    // 根据效果类型应用不同加成
                    statsComp.MovementBonus     += effect.GetCurrentEffectValue() / 3;
                    statsComp.RangedAttackBonus += effect.GetCurrentEffectValue() / 2;
                    statsComp.MeleeAttackBonus  += effect.GetCurrentEffectValue() / 2;
                }
            }
        }
    }
}
