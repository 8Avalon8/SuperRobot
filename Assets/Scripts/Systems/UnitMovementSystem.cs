using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    public class UnitMovementSystem : GameSystem
    {
        // 地图引用
        private IMapManager _mapData => GameManager.Instance.MapManager;

        // 当前移动中的单位
        private Dictionary<int, MovementData> _movingUnits = new Dictionary<int, MovementData>();

        // 移动速度（每秒移动的单位数）
        private float _moveSpeed = 1f;

        public override void Initialize()
        {
            // 订阅事件
            EventManager.Subscribe<UnitMoveRequestEvent>(OnUnitMoveRequest);
        }

        public override void Execute()
        {
            // 更新所有移动中的单位
            UpdateMovingUnits();
        }

        /// <summary>
        /// 处理单位移动请求
        /// </summary>
        private void OnUnitMoveRequest(UnitMoveRequestEvent evt)
        {
            // 获取单位
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var unit = unitSystem.GetUnit(evt.UnitId);

            if (unit == null)
            {
                Debug.LogWarning($"Unit with ID {evt.UnitId} not found");
                return;
            }

            // 检查单位是否已有行动点
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            if (statsComp == null || statsComp.CurrentActionPoints <= 0)
            {
                Debug.LogWarning($"Unit {evt.UnitId} has no action points left");
                return;
            }

            // 获取单位当前位置
            var posComp = unit.GetComponent<PositionComponent>();
            if (posComp == null)
            {
                Debug.LogWarning($"Unit {evt.UnitId} has no position component");
                return;
            }

            // 找到路径
            var path = _mapData.FindPath(posComp.Position, evt.TargetPosition);

            if (path.Count == 0)
            {
                // 触发路径找不到事件
                EventManager.TriggerEvent(new UnitPathNotFoundEvent { UnitId = evt.UnitId });
                return;
            }

            Debug.Log($"Path found: {string.Join(", ", path)}");

            // 开始移动单位
            StartUnitMovement(unit, path);

            // 消耗行动点
            statsComp.CurrentActionPoints--;

            // 触发移动开始事件
            EventManager.TriggerEvent(new UnitMovementStartedEvent
            {
                UnitId = evt.UnitId,
                Path = path
            });
        }

        /// <summary>
        /// 开始单位移动
        /// </summary>
        private void StartUnitMovement(GameEntity unit, List<Vector2Int> path)
        {
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            if (statsComp == null)
                return;

            int unitId = unit.EntityId;
            
            // 获取单位当前位置
            var posComp = unit.GetComponent<PositionComponent>();
            if (posComp == null)
                return;
        
            // 如果路径只有一个点（目标位置），则添加起始位置作为第一个点
            if (path.Count == 1)
            {
                var newPath = new List<Vector2Int> { posComp.Position };
                newPath.AddRange(path);
                path = newPath;
            }

            // 如果单位已经在移动，更新其路径
            if (_movingUnits.ContainsKey(unitId))
            {
                _movingUnits[unitId].Path = path;
                _movingUnits[unitId].CurrentPathIndex = 0;
                _movingUnits[unitId].MoveProgress = 0f;
            }
            else
            {
                // 创建新的移动数据
                _movingUnits[unitId] = new MovementData
                {
                    Unit = unit,
                    Path = path,
                    CurrentPathIndex = 0,
                    MoveProgress = 0f
                };
            }
        }

        /// <summary>
        /// 更新所有移动中的单位
        /// </summary>
        private void UpdateMovingUnits()
        {
            // 使用临时列表存储已完成移动的单位ID
            List<int> completedUnits = new List<int>();

            // 更新每个移动中的单位
            foreach (var kvp in _movingUnits)
            {
                int unitId = kvp.Key;
                var movementData = kvp.Value;

                // 更新移动进度
                movementData.MoveProgress += Time.deltaTime * _moveSpeed;

                // 如果当前路径段完成
                if (movementData.MoveProgress >= 1f)
                {
                    // 重置进度并移动到下一个路径点
                    movementData.MoveProgress = 0f;

                    // 更新单位位置
                    var posComp = movementData.Unit.GetComponent<PositionComponent>();
                    if (posComp != null)
                    {
                        posComp.SetPosition(movementData.Path[movementData.CurrentPathIndex]);

                        // 更新单位视觉位置
                        UpdateUnitVisualPosition(movementData.Unit);

                        // 触发单位位置更新事件
                        EventManager.TriggerEvent(new UnitPositionUpdatedEvent
                        {
                            UnitId = unitId,
                            Position = posComp.Position
                        });
                        Debug.Log($"Unit {unitId} moved to {posComp.Position}");
                    }

                    // 移动到下一个路径点
                    movementData.CurrentPathIndex++;

                    // 检查是否到达终点
                    if (movementData.CurrentPathIndex >= movementData.Path.Count)
                    {
                        // 移动完成
                        completedUnits.Add(unitId);

                        // 触发移动完成事件
                        EventManager.TriggerEvent(new UnitMovementCompletedEvent
                        {
                            UnitId = unitId,
                            FinalPosition = posComp.Position
                        });
                    }
                }
                else
                {
                    // 在路径点之间进行插值
                    UpdateUnitVisualPosition(movementData.Unit, movementData);
                }
            }

            // 移除已完成移动的单位
            foreach (int unitId in completedUnits)
            {
                _movingUnits.Remove(unitId);
            }
        }

        /// <summary>
        /// 更新单位视觉位置
        /// </summary>
        private void UpdateUnitVisualPosition(GameEntity unit, MovementData movementData = null)
        {
            // 获取单位的Transform组件
            var unitObject = unit.Model;
            if (unitObject == null)
                return;

            var posComp = unit.GetComponent<PositionComponent>();
            if (posComp == null)
                return;

            if (movementData == null)
            {
                // 直接设置位置
                Vector3 worldPos = _mapData.GetCellWorldPosition(posComp.Position);
                unitObject.position = worldPos;
            }
            else
            {
                // 在路径点之间进行插值
                Vector2Int fromPos = movementData.CurrentPathIndex > 0
                    ? movementData.Path[movementData.CurrentPathIndex - 1]
                    : posComp.Position;

                var toPos = movementData.Path[movementData.CurrentPathIndex];

                Vector3 fromWorld = _mapData.GetCellWorldPosition(fromPos);
                Vector3 toWorld   = _mapData.GetCellWorldPosition(toPos);

                // 插值计算当前位置
                Vector3 currentPos = Vector3.Lerp(fromWorld, toWorld, movementData.MoveProgress);

                // 设置单位位置
                unitObject.transform.position = currentPos;

                // 计算朝向
                Vector3 moveDirection = toWorld - fromWorld;
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    unitObject.transform.rotation = Quaternion.Slerp(
                        unitObject.transform.rotation,
                        targetRotation,
                        Time.deltaTime * 10f
                    );
                }
            }
        }

        /// <summary>
        /// 取消单位移动
        /// </summary>
        public void CancelUnitMovement(int unitId)
        {
            if (_movingUnits.ContainsKey(unitId))
            {
                _movingUnits.Remove(unitId);

                // 触发移动取消事件
                EventManager.TriggerEvent(new UnitMovementCancelledEvent { UnitId = unitId });
            }
        }

        /// <summary>
        /// 检查单位是否在移动中
        /// </summary>
        public bool IsUnitMoving(int unitId)
        {
            return _movingUnits.ContainsKey(unitId);
        }

        public override void Cleanup()
        {
            // 取消订阅事件
            EventManager.Instance.Unsubscribe<UnitMoveRequestEvent>(OnUnitMoveRequest);

            // 清空移动单位字典
            _movingUnits.Clear();
        }

        // 移动数据类
        private class MovementData
        {
            public GameEntity Unit;
            public List<Vector2Int> Path;
            public int CurrentPathIndex;
            public float MoveProgress;
        }
    }
}
