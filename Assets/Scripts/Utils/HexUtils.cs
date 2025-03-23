using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    public static class HexUtils
    {
        // 计算两个六边形坐标之间的距离
        public static int Distance(HexCoord a, HexCoord b)
        {
            return Mathf.Max(
                Mathf.Abs(a.q - b.q),
                Mathf.Abs(a.r - b.r),
                Mathf.Abs(a.s - b.s)
            );
        }
        
        public static int Distance(Vector2Int a, Vector2Int b)
        {
            return Distance(HexGridConverter.ToHexCoord(a), HexGridConverter.ToHexCoord(b));
        }

        // 获取六个方向向量
        public static readonly HexCoord[] Directions = new HexCoord[]
        {
            new HexCoord(1, 0),  // 0: 东
            new HexCoord(1, -1), // 1: 东北
            new HexCoord(0, -1), // 2: 西北
            new HexCoord(-1, 0), // 3: 西
            new HexCoord(-1, 1), // 4: 西南
            new HexCoord(0, 1)   // 5: 东南
        };

        // 获取指定方向的相邻格
        public static HexCoord Neighbor(HexCoord center, int direction)
        {
            direction = ((direction % 6) + 6) % 6; // 确保direction在0-5范围内
            HexCoord dir = Directions[direction];
            return new HexCoord(center.q + dir.q, center.r + dir.r);
        }
        
        // 获得两个坐标的距离
        public static int GetDistance(HexCoord a, HexCoord b)
        {
            return Mathf.Max(
                Mathf.Abs(a.q - b.q),
                Mathf.Abs(a.r - b.r),
                Mathf.Abs(a.s - b.s)
            );
        }

        // 获取从某个坐标特定范围内的所有坐标
        public static List<HexCoord> GetHexesInRange(HexCoord center, int range)
        {
            List<HexCoord> results = new List<HexCoord>();

            for (int q = -range; q <= range; q++)
            {
                int r1 = Mathf.Max(-range, -q - range);
                int r2 = Mathf.Min(range, -q  + range);

                for (int r = r1; r <= r2; r++)
                {
                    results.Add(new HexCoord(center.q + q, center.r + r));
                }
            }

            return results;
        }

        // 六边形坐标到世界坐标的转换
        public static Vector3 HexToWorld(HexCoord hex, float size)
        {
            float x = size * (3f                 / 2f * hex.q);
            float z = size * (Mathf.Sqrt(3) / 2f * hex.q + Mathf.Sqrt(3) * hex.r);
            return new Vector3(x, 0, z);
        }

        // 世界坐标到六边形坐标的转换
        public static HexCoord WorldToHex(Vector3 position, float size)
        {
            float q = (2f       / 3f * position.x)                              / size;
            float r = (-1f / 3f * position.x + Mathf.Sqrt(3) / 3f * position.z) / size;

            return HexRound(new FractionalHex(q, r));
        }

        // 用于将浮点六边形坐标转为整数坐标
        private struct FractionalHex
        {
            public float q;
            public float r;
            public float s;

            public FractionalHex(float q, float r)
            {
                this.q = q;
                this.r = r;
                this.s = -q - r;
            }
        }

        private static HexCoord HexRound(FractionalHex h)
        {
            int qi = Mathf.RoundToInt(h.q);
            int ri = Mathf.RoundToInt(h.r);
            int si = Mathf.RoundToInt(h.s);

            float q_diff = Mathf.Abs(qi - h.q);
            float r_diff = Mathf.Abs(ri - h.r);
            float s_diff = Mathf.Abs(si - h.s);

            if (q_diff > r_diff && q_diff > s_diff)
                qi = -ri - si;
            else if (r_diff > s_diff)
                ri = -qi - si;
            else
                si = -qi - ri;

            return new HexCoord(qi, ri);
        }
    }

    /// <summary>
    /// 提供Vector2Int和HexCoord之间的转换方法
    /// </summary>
    public static class HexGridConverter
    {
        /// <summary>
        /// 六边形网格方向枚举
        /// </summary>
        public enum HexOrientation
        {
            PointyTop, // 尖端朝上的六边形
            FlatTop    // 平顶朝上的六边形
        }

        /// <summary>
        /// 偏移方式枚举
        /// </summary>
        public enum OffsetType
        {
            Odd, // 奇数行/列偏移
            Even // 偶数行/列偏移
        }

        #region 尖端朝上(Pointy Top)的六边形转换

        /// <summary>
        /// 从Vector2Int转换为HexCoord (尖端朝上的六边形，奇数行偏移)
        /// </summary>
        public static HexCoord OddRowToHexCoord(Vector2Int offset)
        {
            int q = offset.x - (offset.y - (offset.y & 1)) / 2;
            int r = offset.y;
            return new HexCoord(q, r);
        }

        /// <summary>
        /// 从HexCoord转换为Vector2Int (尖端朝上的六边形，奇数行偏移)
        /// </summary>
        public static Vector2Int HexCoordToOddRow(HexCoord hex)
        {
            int col = hex.q + (hex.r - (hex.r & 1)) / 2;
            int row = hex.r;
            return new Vector2Int(col, row);
        }

        /// <summary>
        /// 从Vector2Int转换为HexCoord (尖端朝上的六边形，偶数行偏移)
        /// </summary>
        public static HexCoord EvenRowToHexCoord(Vector2Int offset)
        {
            int q = offset.x - (offset.y + (offset.y & 1)) / 2;
            int r = offset.y;
            return new HexCoord(q, r);
        }

        /// <summary>
        /// 从HexCoord转换为Vector2Int (尖端朝上的六边形，偶数行偏移)
        /// </summary>
        public static Vector2Int HexCoordToEvenRow(HexCoord hex)
        {
            int col = hex.q + (hex.r + (hex.r & 1)) / 2;
            int row = hex.r;
            return new Vector2Int(col, row);
        }

        #endregion

        #region 平顶朝上(Flat Top)的六边形转换

        /// <summary>
        /// 从Vector2Int转换为HexCoord (平顶朝上的六边形，奇数列偏移)
        /// </summary>
        public static HexCoord OddColumnToHexCoord(Vector2Int offset)
        {
            int q = offset.x;
            int r = offset.y - (offset.x - (offset.x & 1)) / 2;
            return new HexCoord(q, r);
        }

        /// <summary>
        /// 从HexCoord转换为Vector2Int (平顶朝上的六边形，奇数列偏移)
        /// </summary>
        public static Vector2Int HexCoordToOddColumn(HexCoord hex)
        {
            int col = hex.q;
            int row = hex.r + (hex.q - (hex.q & 1)) / 2;
            return new Vector2Int(col, row);
        }

        /// <summary>
        /// 从Vector2Int转换为HexCoord (平顶朝上的六边形，偶数列偏移)
        /// </summary>
        public static HexCoord EvenColumnToHexCoord(Vector2Int offset)
        {
            int q = offset.x;
            int r = offset.y - (offset.x + (offset.x & 1)) / 2;
            return new HexCoord(q, r);
        }

        /// <summary>
        /// 从HexCoord转换为Vector2Int (平顶朝上的六边形，偶数列偏移)
        /// </summary>
        public static Vector2Int HexCoordToEvenColumn(HexCoord hex)
        {
            int col = hex.q;
            int row = hex.r + (hex.q + (hex.q & 1)) / 2;
            return new Vector2Int(col, row);
        }

        #endregion

        #region 通用转换方法

        /// <summary>
        /// 从Vector2Int转换为HexCoord
        /// </summary>
        /// <param name="offset">二维坐标</param>
        /// <param name="orientation">六边形方向</param>
        /// <param name="offsetType">偏移类型</param>
        public static HexCoord Vector2IntToHexCoord(Vector2Int offset, HexOrientation orientation,
                                                    OffsetType offsetType)
        {
            if (orientation == HexOrientation.PointyTop)
            {
                return offsetType == OffsetType.Odd
                    ? OddRowToHexCoord(offset)
                    : EvenRowToHexCoord(offset);
            }
            else
            {
                return offsetType == OffsetType.Odd
                    ? OddColumnToHexCoord(offset)
                    : EvenColumnToHexCoord(offset);
            }
        }

        /// <summary>
        /// 从HexCoord转换为Vector2Int
        /// </summary>
        /// <param name="hex">六边形坐标</param>
        /// <param name="orientation">六边形方向</param>
        /// <param name="offsetType">偏移类型</param>
        public static Vector2Int HexCoordToVector2Int(HexCoord hex, HexOrientation orientation, OffsetType offsetType)
        {
            if (orientation == HexOrientation.PointyTop)
            {
                return offsetType == OffsetType.Odd
                    ? HexCoordToOddRow(hex)
                    : HexCoordToEvenRow(hex);
            }
            else
            {
                return offsetType == OffsetType.Odd
                    ? HexCoordToOddColumn(hex)
                    : HexCoordToEvenColumn(hex);
            }
        }

        #endregion

        #region 主要推荐使用的转换方法

        // 默认使用尖端朝上，奇数行偏移的六边形布局
        private static HexOrientation _defaultOrientation = HexOrientation.FlatTop;
        private static OffsetType     _defaultOffsetType  = OffsetType.Odd;

        /// <summary>
        /// 设置默认转换参数
        /// </summary>
        public static void SetDefaultConversionParameters(HexOrientation orientation, OffsetType offsetType)
        {
            _defaultOrientation = orientation;
            _defaultOffsetType  = offsetType;
        }

        /// <summary>
        /// 从Vector2Int转换为HexCoord (使用默认参数)
        /// </summary>
        public static HexCoord ToHexCoord(Vector2Int vector2Int)
        {
            return Vector2IntToHexCoord(vector2Int, _defaultOrientation, _defaultOffsetType);
        }

        /// <summary>
        /// 从HexCoord转换为Vector2Int (使用默认参数)
        /// </summary>
        public static Vector2Int ToVector2Int(HexCoord hexCoord)
        {
            return HexCoordToVector2Int(hexCoord, _defaultOrientation, _defaultOffsetType);
        }

        #endregion
    }
}