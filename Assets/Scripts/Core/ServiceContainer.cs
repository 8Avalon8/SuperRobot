using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot.Core
{
    /// <summary>
    /// 简单的服务容器实现
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void RegisterService<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"Cannot register null service for type {typeof(T).Name}");
                return;
            }

            Type serviceType = typeof(T);
            
            if (_services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"Service {serviceType.Name} is already registered. Overwriting...");
            }
            
            _services[serviceType] = service;
        }

        public T GetService<T>() where T : class
        {
            Type serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }
            
            Debug.LogError($"Service {serviceType.Name} is not registered");
            return null;
        }

        public bool HasService<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public void RemoveService<T>() where T : class
        {
            Type serviceType = typeof(T);
            _services.Remove(serviceType);
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}