using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    /// <summary>
    /// 移动类型枚举
    /// </summary>
    public enum MovementType
    {
        Ground,     // 地面移动
        Air,        // 空中移动
        Amphibious, // 水陆两栖
        Space       // 太空移动
    }

    /// <summary>
    /// 移动能力组件 - 统一的移动系统
    /// </summary>
    public class MovementComponent : IComponent
    {
        public MovementType MovementType { get; set; } = MovementType.Ground;
        public int MovementRange { get; set; }
        public int RemainingMovement { get; set; }
        public List<Vector2Int> PlannedPath { get; private set; }
        public bool HasMoved { get; set; } = false;

        // 移动限制
        public bool CanMoveOverWater { get; set; } = false;
        public bool CanMoveOverMountains { get; set; } = false;
        public bool IgnoresTerrainCost { get; set; } = false;

        public void Initialize()
        {
            MovementRange = 3;
            RemainingMovement = MovementRange;
            PlannedPath = new List<Vector2Int>();
        }

        public void Initialize(MovementType movementType, int movementRange)
        {
            MovementType = movementType;
            MovementRange = movementRange;
            RemainingMovement = MovementRange;
            PlannedPath = new List<Vector2Int>();
            
            // 根据移动类型设置能力
            SetMovementCapabilities();
        }

        public void Cleanup()
        {
            PlannedPath.Clear();
        }

        private void SetMovementCapabilities()
        {
            switch (MovementType)
            {
                case MovementType.Ground:
                    CanMoveOverWater = false;
                    CanMoveOverMountains = false;
                    break;
                case MovementType.Air:
                    CanMoveOverWater = true;
                    CanMoveOverMountains = true;
                    IgnoresTerrainCost = true;
                    break;
                case MovementType.Amphibious:
                    CanMoveOverWater = true;
                    CanMoveOverMountains = false;
                    break;
                case MovementType.Space:
                    IgnoresTerrainCost = true;
                    CanMoveOverWater = true;
                    CanMoveOverMountains = true;
                    break;
            }
        }

        /// <summary>
        /// 重置移动范围（每回合开始时调用）
        /// </summary>
        public void ResetMovement()
        {
            RemainingMovement = MovementRange;
            PlannedPath.Clear();
            HasMoved = false;
        }

        /// <summary>
        /// 设置移动路径
        /// </summary>
        public void SetPath(List<Vector2Int> path)
        {
            PlannedPath.Clear();
            PlannedPath.AddRange(path);
        }

        /// <summary>
        /// 检查是否能移动到指定位置
        /// </summary>
        public bool CanMoveTo(Vector2Int position, int cost = 1)
        {
            return RemainingMovement >= cost && !HasMoved;
        }

        /// <summary>
        /// 执行移动
        /// </summary>
        public bool ExecuteMove(int cost = 1)
        {
            if (CanMoveTo(Vector2Int.zero, cost))
            {
                RemainingMovement -= cost;
                HasMoved = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取移动范围内的所有位置
        /// </summary>
        public List<Vector2Int> GetMovablePositions(Vector2Int currentPosition)
        {
            var positions = new List<Vector2Int>();
            
            // 简单的方形范围计算（实际实现中应该考虑地形和路径寻找）
            for (int x = -MovementRange; x <= MovementRange; x++)
            {
                for (int y = -MovementRange; y <= MovementRange; y++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(y);
                    if (distance <= MovementRange && distance > 0)
                    {
                        positions.Add(currentPosition + new Vector2Int(x, y));
                    }
                }
            }
            
            return positions;
        }

        /// <summary>
        /// 检查是否能在指定地形上移动
        /// </summary>
        public bool CanMoveOnTerrain(TerrainType terrainType)
        {
            return terrainType switch
            {
                TerrainType.Water => CanMoveOverWater,
                TerrainType.Mountain => CanMoveOverMountains,
                _ => true
            };
        }

        /// <summary>
        /// 获取在指定地形上的移动消耗
        /// </summary>
        public int GetMovementCost(TerrainType terrainType)
        {
            if (IgnoresTerrainCost)
                return 1;

            return terrainType switch
            {
                TerrainType.Plain => 1,
                TerrainType.Forest => 2,
                TerrainType.Mountain => CanMoveOverMountains ? 3 : int.MaxValue,
                TerrainType.Water => CanMoveOverWater ? 2 : int.MaxValue,
                TerrainType.Desert => 2,
                TerrainType.Urban => 1,
                _ => 1
            };
        }
    }

    /// <summary>
    /// 移动历史组件 - 记录移动轨迹
    /// </summary>
    public class MovementHistoryComponent : IComponent
    {
        public List<Vector2Int> MovementHistory { get; private set; }
        public Vector2Int LastPosition { get; set; }
        public int MaxHistorySize { get; set; } = 10;

        public void Initialize()
        {
            MovementHistory = new List<Vector2Int>();
        }

        public void Cleanup()
        {
            MovementHistory.Clear();
        }

        public void RecordMovement(Vector2Int from, Vector2Int to)
        {
            LastPosition = from;
            MovementHistory.Add(to);
            
            // 限制历史记录大小
            if (MovementHistory.Count > MaxHistorySize)
            {
                MovementHistory.RemoveAt(0);
            }
        }

        public Vector2Int GetPreviousPosition()
        {
            return MovementHistory.Count > 1 ? MovementHistory[MovementHistory.Count - 2] : LastPosition;
        }
    }
}