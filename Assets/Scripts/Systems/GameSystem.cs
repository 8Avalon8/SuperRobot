using UnityEngine;
using SuperRobot.Core;

namespace SuperRobot
{
    // 系统基类
    public abstract class GameSystem
    {
        private IServiceContainer _serviceContainer;
        
        // 缓存常用服务引用以提高性能
        private EntityManager         _entityManager;
        private ImprovedSystemManager _systemManager;
        private EventManager          _eventManager;
        
        protected EntityManager EntityManager => _entityManager ??= GetService<IEntityManagerService>()?.EntityManager;
        protected ImprovedSystemManager SystemManager => _systemManager ??= GetService<ISystemManagerService>()?.ImprovedSystemManager;
        protected EventManager EventManager => _eventManager ??= GetService<IEventManagerService>()?.EventManager;
        
        /// <summary>
        /// 注入服务容器
        /// </summary>
        public virtual void InjectServices(IServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
            
            // 清空缓存，强制重新获取服务
            _entityManager = null;
            _systemManager = null;
            _eventManager = null;
        }
        
        /// <summary>
        /// 获取服务
        /// </summary>
        protected T GetService<T>() where T : class
        {
            return _serviceContainer?.GetService<T>();
        }
        
        /// <summary>
        /// 系统初始化
        /// </summary>
        public virtual void Initialize() { }
        
        /// <summary>
        /// 系统执行逻辑
        /// </summary>
        public abstract void Execute();
        
        /// <summary>
        /// 系统清理
        /// </summary>
        public virtual void Cleanup() { }
        
        /// <summary>
        /// 获取其他系统
        /// </summary>
        protected T GetSystem<T>() where T : GameSystem
        {
            return SystemManager?.GetSystem<T>();
        }
    }
}