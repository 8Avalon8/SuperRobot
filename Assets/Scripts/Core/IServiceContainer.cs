using System;

namespace SuperRobot.Core
{
    /// <summary>
    /// 服务容器接口，用于依赖注入
    /// </summary>
    public interface IServiceContainer
    {
        void RegisterService<T>(T service) where T : class;
        T GetService<T>() where T : class;
        bool HasService<T>() where T : class;
        void RemoveService<T>() where T : class;
    }
}