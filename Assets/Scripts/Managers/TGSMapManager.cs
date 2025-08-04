using System;
using System.Collections.Generic;
using System.Linq;
using TGS;
using UnityEngine;
using WorldMapStrategyKit;

namespace SuperRobot
{
    public class TGSMapManager : MonoBehaviour,IMapManager
    {
        TerrainGridSystem map;

        public static TGSMapManager Instance { get; private set; }

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
            map = TerrainGridSystem.instance;
            map.Redraw();
            // 注册监听
            map.OnCellClick += onCellClick;
        }

        private void OnDestroy()
        {
            map.OnCellClick -= onCellClick;
        }

        private void onCellClick(TerrainGridSystem tgs, int cellIndex, int buttonIndex)
        {
            return;
            var cell = map.cells[cellIndex];
            var cellPos = new Vector2Int(cell.row, cell.column);
            // 发送事件
            EventManager.Instance.TriggerEvent(new TerrainSelectedEvent() { GridPosition = cellPos });
        }

        // FindPath
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            var startCellIndex = map.CellGetIndex(start.x, start.y);
            var endCellIndex = map.CellGetIndex(end.x, end.y);
            var path = map.FindPath(startCellIndex, endCellIndex);
            var pathList = new List<Vector2Int>();
            foreach (var cellIndex in path)
            {
                var cell = map.cells[cellIndex];
                pathList.Add(new Vector2Int(cell.row, cell.column));
            }
            return pathList;
        }   

        private List<int> _highlightedCells = new List<int>();

        public void HighlightCells(List<Vector2Int> cells)
        {
            ClearHighlightCells();
            foreach (var cell in cells)
            {
                _highlightedCells.Add(map.CellGetIndex(cell.x, cell.y));
            }
            map.CellBlink(_highlightedCells, Color.blue, 1f);
            
        }

        public void ClearHighlightCells()
        {
            //map.CellFadeOut(_highlightedCells, Color.blue, 1f);
            _highlightedCells.Clear();
        }

        public void ShowPath(List<Vector2Int> path)
        {
            ClearHighlightCells();
            foreach (var cell in path)
            {
                _highlightedCells.Add(map.CellGetIndex(cell.x, cell.y));
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
            return map.CellGetPosition(map.CellGetIndex(cellPos.x, cellPos.y));
        }

        public List<Vector2Int> GetMoveRange(Vector2Int cellPos, int range)
        {
            var cells = map.CellGetNeighbours(map.CellGetIndex(cellPos.x, cellPos.y), range);
            var moveRange = new List<Vector2Int>();
            foreach (var cellIndex in cells)
            {
                var cell = map.cells[cellIndex];
                moveRange.Add(new Vector2Int(cell.row, cell.column));
            }
            return moveRange;
        }

        public string GetCellType(Vector2Int cellPos)
        {
            var cell = map.cells[map.CellGetIndex(cellPos.x, cellPos.y)];
            return cell.attrib?.GetField("type")?.str;
        }

        public Vector2Int GetMapCenterPosition()
        {
            return new Vector2Int(map.rowCount / 2, map.columnCount / 2);
        }

        public void SetEnableTouch(bool enable)
        {
        }

        public void HighlightCells(List<Vector2Int> cells, Color color)
        {
            ClearHighlightCells();
            foreach (var cell in cells)
            {
                _highlightedCells.Add(map.CellGetIndex(cell.x, cell.y));
            }
            map.CellBlink(_highlightedCells, color, 1f);
        }

        public void ClearHighlights(List<Vector2Int> cells)
        {
            map.CellFadeOut(_highlightedCells, Color.blue, 1f);
            _highlightedCells.Clear();
        }

        public Vector2Int ScreenToCell(Vector2 screenPosition)
        {
            var cell = map.CellGetAtScreenPosition(screenPosition);
            if (cell == null)
                return new Vector2Int(-1, -1);
            return new Vector2Int(cell.row, cell.column);
        }

        public bool IsValidCell(Vector2Int cellPosition)
        {
            var cell = map.cells[map.CellGetIndex(cellPosition.x, cellPosition.y)];
            return cell != null;
        }

        public float GetMovementCost(Vector2Int cellPosition, UnitType unitType)
        {
            var cell = map.cells[map.CellGetIndex(cellPosition.x, cellPosition.y)];
            var costNum = cell.attrib?.GetField("movementCost");
            if (costNum == null)
                return 1f;
            return (float)costNum;
        }
    }
}