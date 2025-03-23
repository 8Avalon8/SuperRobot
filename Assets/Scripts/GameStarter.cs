using UnityEngine;

namespace SuperRobot
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private HexMapGenerator _mapGenerator;
        public void StartNewGame()
        {
            // 生成地图
            _mapGenerator.GenerateMap();
            // 生成地图数据
            var hexMapData = _mapGenerator.GenerateMapData();

            // 初始化玩家资源和单位
            // 开始第一回合
            TurnManager.SetCurrentTurn(1);
        }

        
    }
}