using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldMapStrategyKit;

namespace SuperRobot
{
    public interface IMapManager
    {
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int end);
        void HighlightCells(List<Vector2Int> cells);
        void ClearHighlightCells();
        void ShowPath(List<Vector2Int> path);
        void ClearPath();
        List<Vector2Int> GetMoveRange(Vector2Int cellPos, int range);
        Vector3 GetCellWorldPosition(Vector2Int cellPos);
        string GetCellType(Vector2Int cellPos);
        Vector2Int GetMapCenterPosition();

        void SetEnableTouch(bool enable);

        void HighlightCells(List<Vector2Int> cells, Color color);
        void ClearHighlights(List<Vector2Int> cells);
        Vector2Int ScreenToCell(Vector2 screenPosition);
        bool IsValidCell(Vector2Int cellPosition);
        float GetMovementCost(Vector2Int cellPosition, UnitType unitType);
    }

    public class WMSKMap : MonoBehaviour, IMapManager
    {
        WMSK map;

        public static WMSKMap Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            map = WMSK.instance;
        }

        // FindPath
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            var path = map.FindRoute(start, end);
            return path.Select(p => new Vector2Int((int)p.x, (int)p.y)).ToList();
        }

        private List<int> _highlightedCells = new List<int>();

        public void HighlightCells(List<Vector2Int> cells)
        {
            ClearHighlightCells();
            foreach (var cell in cells)
            {
                _highlightedCells.Add(map.GetCellIndex(cell.x, cell.y));
            }
            map.CellBlink(_highlightedCells, Color.blue, 1f);
        }

        public void ClearHighlightCells()
        {
            map.CellFadeOut(_highlightedCells, Color.blue, 1f);
            _highlightedCells.Clear();
        }

        public void ShowPath(List<Vector2Int> path)
        {
            ClearHighlightCells();
            foreach (var cell in path)
            {
                _highlightedCells.Add(map.GetCellIndex(cell.x, cell.y));
            }
            map.CellBlink(_highlightedCells, Color.green, 1f);
        }

        public void ClearPath()
        {
            map.CellFadeOut(_highlightedCells, Color.green, 1f);
            _highlightedCells.Clear();
        }

        public Vector3 GetCellWorldPosition(Vector2Int cellPos)
        {
            return map.GetCellWorldPosition(map.GetCellIndex(cellPos.x, cellPos.y));
        }

        public List<Vector2Int> GetMoveRange(Vector2Int cellPos, int range)
        {
            var cells = map.GetCellNeighbours(map.GetCellIndex(cellPos.x, cellPos.y), range);
            var moveRange = new List<Vector2Int>();
            foreach (var cellIndex in cells)
            {
                var cell = map.GetCell(cellIndex);
                moveRange.Add(new Vector2Int(cell.column, cell.row));
            }
            return moveRange;
        }

        public string GetCellType(Vector2Int cellPos)
        {
            var cell = map.GetCell(map.GetCellIndex(cellPos.x, cellPos.y));
            return cell.attrib?.GetField("type")?.str;
        }

        public Vector2Int GetMapCenterPosition()
        {
            var city = map.GetCity("Chengdu", "China");
            var cityIndex = map.GetCityIndex(city);
            var cellIndex = map.GetCityPosition(cityIndex);
            var cell = map.GetCell(cellIndex);
            return new Vector2Int(cell.column, cell.row);
        }

        public void SetEnableTouch(bool enable)
        {
        }

        public void HighlightCells(List<Vector2Int> cells, Color color)
        {
            throw new NotImplementedException();
        }

        public void ClearHighlights(List<Vector2Int> cells)
        {
            throw new NotImplementedException();
        }

        public Vector2Int ScreenToCell(Vector2 screenPosition)
        {
            throw new NotImplementedException();
        }

        public bool IsValidCell(Vector2Int cellPosition)
        {
            throw new NotImplementedException();
        }

        public float GetMovementCost(Vector2Int cellPosition, UnitType unitType)
        {
            throw new NotImplementedException();
        }
    }
}