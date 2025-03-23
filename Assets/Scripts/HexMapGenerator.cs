using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{
    public class HexMapGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject _hexPrefab;
        [SerializeField] private float      _hexSize   = 1f;
        [SerializeField] private int        _mapRadius = 20;

        private HexMapData _hexMapData;

        // 存储地图数据
        private Dictionary<HexCoord, TerrainType> _terrainMap = new Dictionary<HexCoord, TerrainType>();
        private Dictionary<HexCoord, GameObject>  _hexObjects = new Dictionary<HexCoord, GameObject>();

        [Button("生成地图")]
        public void GenerateMap()
        {
            ClearMap();
            GenerateTerrainData();
            InstantiateHexes();
        }

        public HexMapData GenerateMapData()
        {
            if (_hexMapData == null)
            {
                _hexMapData = new GameHexMap(_mapRadius, _terrainMap, _hexObjects);
            }
            return _hexMapData;
        }

        public Vector3 GetHexPosition(HexCoord hex)
        {
            return HexUtils.HexToWorld(hex, _hexSize);
        }

        private void ClearMap()
        {
            // 清除所有现有的六边形物体
            foreach (var hexObj in _hexObjects.Values)
            {
                if (Application.isPlaying)
                    Destroy(hexObj);
                else
                    DestroyImmediate(hexObj);
            }

            _hexObjects.Clear();
            _terrainMap.Clear();
        }

        private void GenerateTerrainData()
        {
            // 获取地图范围内的所有坐标（使用2D网格坐标，然后转换为HexCoord）
            List<HexCoord> allHexes = new List<HexCoord>();
            
            // 确保使用与GameInitializer相同的坐标系统（FlatTop, Odd）
            for (int x = -_mapRadius; x <= _mapRadius; x++)
            {
                for (int y = -_mapRadius; y <= _mapRadius; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    HexCoord hexCoord = HexGridConverter.ToHexCoord(gridPos);
                    
                    // 检查是否在半径范围内
                    if (HexUtils.Distance(hexCoord, new HexCoord(0, 0)) <= _mapRadius)
                    {
                        allHexes.Add(hexCoord);
                    }
                }
            }

            // 为每个坐标分配地形类型
            foreach (var hex in allHexes)
            {
                // 简单地形生成逻辑 - 可以使用噪声函数等更复杂方法
                float perlin = Mathf.PerlinNoise(
                    hex.q * 0.1f + 0.5f,
                    hex.r * 0.1f + 0.5f
                );

                TerrainType terrainType;

                if (perlin < 0.3f)
                    terrainType = TerrainType.Water;
                else if (perlin < 0.5f)
                    terrainType = TerrainType.Plain;
                else if (perlin < 0.7f)
                    terrainType = TerrainType.Forest;
                else
                    terrainType = TerrainType.Mountain;

                _terrainMap[hex] = terrainType;
            }
        }

        private void InstantiateHexes()
        {
            foreach (var entry in _terrainMap)
            {
                var hex = entry.Key;
                var terrainType = entry.Value;

                // 计算世界坐标 - 确保使用HexUtils提供的方法计算世界坐标
                Vector3 position = HexUtils.HexToWorld(hex, _hexSize);

                // 实例化六边形
                GameObject hexObject = Instantiate(_hexPrefab, position, Quaternion.identity);
                hexObject.name = $"Hex_{hex.q}_{hex.r}";
                hexObject.transform.parent = transform;

                // 设置材质颜色以表示地形
                var renderer = hexObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = GetTerrainColor(terrainType);
                }

                // 添加六边形数据组件
                var hexData = hexObject.AddComponent<HexCell>();
                hexData.Coordinates = hex;
                hexData.TerrainType = terrainType;

                _hexObjects[hex] = hexObject;
            }
        }

        private Color GetTerrainColor(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Plain:
                    return new Color(0.7f, 0.9f, 0.5f);
                case TerrainType.Forest:
                    return new Color(0.0f, 0.6f, 0.2f);
                case TerrainType.Mountain:
                    return new Color(0.5f, 0.5f, 0.5f);
                case TerrainType.Water:
                    return new Color(0.0f, 0.5f, 0.9f);
                case TerrainType.Desert:
                    return new Color(0.9f, 0.9f, 0.5f);
                case TerrainType.Urban:
                    return new Color(0.7f, 0.7f, 0.7f);
                case TerrainType.Space:
                    return new Color(0.1f, 0.1f, 0.2f);
                default:
                    return Color.white;
            }
        }

        // 获取特定坐标的地形类型
        public TerrainType GetTerrainTypeAt(HexCoord coord)
        {
            if (_terrainMap.TryGetValue(coord, out var terrain))
                return terrain;

            return TerrainType.Plain; // 默认返回平原
        }
    }

// 六边形格子组件
    
}