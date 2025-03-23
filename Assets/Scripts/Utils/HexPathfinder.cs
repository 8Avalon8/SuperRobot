using System.Collections.Generic;
using UnityEngine;
using System;

namespace SuperRobot
{
    public class HexPathfinder
    {
        // 网格引用
        private HexMapData _mapData;

        public HexPathfinder(HexMapData mapData)
        {
            _mapData = mapData;
        }

        /// <summary>
        /// 计算从起点到终点的路径
        /// </summary>
        public List<HexCoord> FindPath(HexCoord start, HexCoord goal, GameEntity unit)
        {
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            if (statsComp == null)
                return new List<HexCoord>();

            // 最大移动距离限制
            int maxDistance = statsComp.MovementRange;

            // 开放列表和关闭列表
            var openSet = new PriorityQueue<HexNode>();
            var closedSet = new HashSet<HexCoord>();
            var costSoFar = new Dictionary<HexCoord, int>();
            var cameFrom = new Dictionary<HexCoord, HexCoord>();

            // 初始化
            openSet.Enqueue(new HexNode(start, 0, HexUtils.Distance(start, goal)));
            costSoFar[start] = 0;

            while (!openSet.IsEmpty)
            {
                var current = openSet.Dequeue();

                // 到达目标
                if (current.Position.Equals(goal))
                    return ReconstructPath(cameFrom, start, goal);

                closedSet.Add(current.Position);

                // 探索所有相邻六边形
                for (int i = 0; i < 6; i++)
                {
                    HexCoord neighbor = HexUtils.Neighbor(current.Position, i);

                    // 跳过已经在关闭列表中的节点
                    if (closedSet.Contains(neighbor))
                        continue;

                    // 检查节点是否可通行
                    if (!IsPositionValid(neighbor, unit))
                        continue;

                    // 计算从起点经过该邻居的总花费
                    int newCost = costSoFar[current.Position] + GetMovementCost(neighbor, unit);

                    // 如果超过最大移动距离，跳过
                    if (newCost > maxDistance)
                        continue;

                    // 如果找到更优路径或节点未被探索
                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        int priority = newCost + HexUtils.Distance(neighbor, goal);
                        openSet.Enqueue(new HexNode(neighbor, newCost, priority));
                        cameFrom[neighbor] = current.Position;
                    }
                }
            }

            // 无法找到路径
            return new List<HexCoord>();
        }

        /// <summary>
        /// 重建路径
        /// </summary>
        private List<HexCoord> ReconstructPath(Dictionary<HexCoord, HexCoord> cameFrom, HexCoord start, HexCoord goal)
        {
            var path = new List<HexCoord>();
            HexCoord current = goal;

            while (!current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// 检查位置是否有效且可通行
        /// </summary>
        private bool IsPositionValid(HexCoord position, GameEntity unit)
        {
            // 检查是否在地图范围内
            if (!_mapData.IsWithinBounds(position))
                return false;

            // 检查地形是否可通行
            TerrainType terrain = _mapData.GetTerrainAt(position);
            var statsComp = unit.GetComponent<UnitStatsComponent>();

            if (statsComp != null)
            {
                switch (statsComp.UnitType)
                {
                    case UnitType.Aircraft:
                        // 飞行单位可以通过除了太空之外的所有地形
                        return terrain != TerrainType.Space;

                    case UnitType.Ship:
                        // 船只只能在水域移动
                        return terrain == TerrainType.Water;

                    default:
                        // 其他单位不能通过水域和太空
                        return terrain != TerrainType.Water && terrain != TerrainType.Space;
                }
            }

            // 检查是否被其他单位占据
            return !_mapData.IsOccupiedByOtherUnit(position, unit.EntityId);
        }

        /// <summary>
        /// 获取移动到该位置的成本
        /// </summary>
        private int GetMovementCost(HexCoord position, GameEntity unit)
        {
            TerrainType terrain = _mapData.GetTerrainAt(position);

            // 根据地形类型返回移动成本
            switch (terrain)
            {
                case TerrainType.Plain:
                    return 1;
                case TerrainType.Forest:
                    return 2;
                case TerrainType.Mountain:
                    return 3;
                case TerrainType.Desert:
                    return 2;
                case TerrainType.Urban:
                    return 1;
                case TerrainType.Water:
                    // 水域只有船只和飞行单位可以通过
                    var statsComp = unit.GetComponent<UnitStatsComponent>();
                    return statsComp?.UnitType == UnitType.Ship ? 1 : 2;
                default:
                    return 1;
            }
        }

        // 路径节点类
        private class HexNode : IComparable<HexNode>
        {
            public HexCoord Position { get; }
            public int Cost { get; }
            public int Priority { get; }

            public HexNode(HexCoord position, int cost, int priority)
            {
                Position = position;
                Cost = cost;
                Priority = priority;
            }

            public int CompareTo(HexNode other)
            {
                return Priority.CompareTo(other.Priority);
            }
        }
    }

    // 简单的优先队列实现
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> _data;

        public bool IsEmpty => _data.Count == 0;

        public PriorityQueue()
        {
            _data = new List<T>();
        }

        public void Enqueue(T item)
        {
            _data.Add(item);
            int childIndex = _data.Count - 1;

            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;

                if (_data[childIndex].CompareTo(_data[parentIndex]) >= 0)
                    break;

                // 交换元素
                T tmp = _data[childIndex];
                _data[childIndex] = _data[parentIndex];
                _data[parentIndex] = tmp;

                childIndex = parentIndex;
            }
        }

        public T Dequeue()
        {
            int lastIndex = _data.Count - 1;
            T frontItem = _data[0];
            _data[0] = _data[lastIndex];
            _data.RemoveAt(lastIndex);

            lastIndex--;
            int parentIndex = 0;

            while (true)
            {
                int childIndex = parentIndex * 2 + 1;

                if (childIndex > lastIndex)
                    break;

                int rightChild = childIndex + 1;

                if (rightChild <= lastIndex && _data[rightChild].CompareTo(_data[childIndex]) < 0)
                    childIndex = rightChild;

                if (_data[parentIndex].CompareTo(_data[childIndex]) <= 0)
                    break;

                // 交换元素
                T tmp = _data[parentIndex];
                _data[parentIndex] = _data[childIndex];
                _data[childIndex] = tmp;

                parentIndex = childIndex;
            }

            return frontItem;
        }
    }
}
