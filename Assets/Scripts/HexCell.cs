using UnityEngine;

namespace SuperRobot
{
    public class HexCell : MonoBehaviour
    {
        public HexCoord    Coordinates;
        public TerrainType TerrainType;

        private Color _originalColor;

        private void Start()
        {
            _originalColor = GetComponent<Renderer>().material.color;
        }

        public void Highlight(Color color)
        {
            GetComponent<Renderer>().material.color = color;
            Debug.Log($"Hex highlighted: {Coordinates.q}, {Coordinates.r}");
        }

        public void ResetColor()
        {
            GetComponent<Renderer>().material.color = _originalColor;
        }

        private void OnMouseDown()
        {
            // 可以在这里添加点击处理，如选择格子
            Debug.Log($"Hex clicked: {Coordinates.q}, {Coordinates.r} - {TerrainType}");

            // 触发格子选择事件
            EventManager.Instance.TriggerEvent(new TerrainSelectedEvent
            {
                GridPosition = Coordinates.ToVector2Int(),
            });
        }
    }
}