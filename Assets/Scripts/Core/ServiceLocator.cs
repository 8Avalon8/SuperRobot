using UnityEngine;

namespace SuperRobot.Core
{
    /// <summary>
    /// 临时的服务定位器，用于向后兼容
    /// 这个类将逐步被依赖注入替代
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceContainer _container;
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化服务定位器
        /// </summary>
        public static void Initialize(IServiceContainer container)
        {
            _container = container;
            _isInitialized = true;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (!_isInitialized)
            {
                Debug.LogError("ServiceLocator is not initialized. Call Initialize() first.");
                return null;
            }

            return _container?.GetService<T>();
        }

        /// <summary>
        /// 检查服务是否可用
        /// </summary>
        public static bool HasService<T>() where T : class
        {
            return _isInitialized && _container?.HasService<T>() == true;
        }

        /// <summary>
        /// 清理服务定位器
        /// </summary>
        public static void Cleanup()
        {
            _container = null;
            _isInitialized = false;
        }
    }
}