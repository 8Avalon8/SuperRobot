using UnityEngine;

namespace SuperRobot
{
    public class GameInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // 设置默认的转换参数（通常只需要在游戏初始化时设置一次）
            HexGridConverter.SetDefaultConversionParameters(
                HexGridConverter.HexOrientation.FlatTop, 
                HexGridConverter.OffsetType.Odd
            );
            
            // 初始化游戏管理器（如果不存在）
            if (GameManager.Instance == null)
            {
                GameObject managerObject = new GameObject("GameManager");
                managerObject.AddComponent<GameManager>();
                DontDestroyOnLoad(managerObject);

                // 添加额外所需组件
                managerObject.AddComponent<SaveSystem>();

                // 添加线程任务系统（如需要）
                //managerObject.AddComponent<ThreadedTaskSystem>();
            }
        }
    }
}
