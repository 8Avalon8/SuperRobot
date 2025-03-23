using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    public interface HexMapData
    {
        bool IsWithinBounds(HexCoord position);
        TerrainType GetTerrainAt(HexCoord position);
        bool IsOccupiedByOtherUnit(HexCoord position, int excludeUnitId);
    }

    // 实现该接口
    public class GameHexMap : HexMapData
    {
        public static GameHexMap Instance;
        private int _mapWidth = 50;
        private int _mapHeight = 50;

        // 存储地图数据
        private Dictionary<HexCoord, TerrainType> _terrainData = new Dictionary<HexCoord, TerrainType>();
        private Dictionary<HexCoord, GameObject>  _hexObjects = new Dictionary<HexCoord, GameObject>();

        public GameHexMap(int mapRadius, Dictionary<HexCoord, TerrainType> terrainData, Dictionary<HexCoord, GameObject> hexObjects)
        {
            Instance = this;
            _mapWidth = mapRadius * 2;
            _mapHeight = mapRadius * 2;
            _terrainData = terrainData;
            _hexObjects = hexObjects;
        }

        public Dictionary<HexCoord, GameObject> Hexes => _hexObjects;

        public HexCoord GetHexCoord(Vector2Int gridPosition)
        {
            return HexGridConverter.ToHexCoord(gridPosition);
        }

        public Vector2Int GetGridPosition(HexCoord hexCoord)
        {
            return HexGridConverter.ToVector2Int(hexCoord);
        }

        public GameObject GetHexObject(HexCoord hexCoord)
        {
            return _hexObjects[hexCoord];
        }

        public Vector3 GetHexWorldPosition(HexCoord hexCoord)
        {
            return HexUtils.HexToWorld(hexCoord, 1f);
        }   

        public bool IsWithinBounds(HexCoord position)
        {
            // 转换为Vector2Int来检查边界
            Vector2Int gridPos = HexGridConverter.ToVector2Int(position);
            return gridPos.x >= 0 && gridPos.x < _mapWidth && gridPos.y >= 0 && gridPos.y < _mapHeight;
        }

        public TerrainType GetTerrainAt(HexCoord position)
        {
            if (_terrainData.TryGetValue(position, out TerrainType terrain))
                return terrain;

            // 默认地形
            return TerrainType.Plain;
        }

        public bool IsOccupiedByOtherUnit(HexCoord position, int excludeUnitId)
        {
            // 使用UnitManagementSystem检查位置是否被其他单位占据
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var units = unitSystem.GetAllUnits();

            foreach (var unit in units)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();
                if (statsComp == null || unit.EntityId == excludeUnitId)
                    continue;

                var posComp = unit.GetComponent<PositionComponent>();
                if (posComp != null && posComp.Position.Equals(position))
                    return true;
            }

            return false;
        }

        // 设置地形数据，通常在地图生成时调用
        public void SetTerrainData(Dictionary<HexCoord, TerrainType> terrainData)
        {
            _terrainData = terrainData;
        }
    }
}