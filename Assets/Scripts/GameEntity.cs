using System;
using System.Collections.Generic;
using UnityEngine;
using SuperRobot.Core;

namespace SuperRobot
{
    // 实体基类
    public class GameEntity : MonoBehaviour
    {
        private          int                          _entityId;
        private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();

        public int EntityId => _entityId;

        public void Initialize(int entityId)
        {
            _entityId = entityId;
        }

        public Transform Model;

        public void SetModel(Transform model)
        {
            Model = model;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public T AddComponent<T>() where T : IComponent, new()
        {
            Type componentType = typeof(T);

            if (_components.ContainsKey(componentType))
            {
                return (T)_components[componentType];
            }

            T component = new T();
            _components[componentType] = component;
            component.Initialize();

            // 通过服务定位器注册到EntityManager
            var entityManagerService = ServiceLocator.GetService<IEntityManagerService>();
            entityManagerService?.EntityManager.RegisterComponentForEntity<T>(_entityId);

            return component;
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        public T GetComponent<T>() where T : IComponent
        {
            Type componentType = typeof(T);

            if (_components.TryGetValue(componentType, out var component))
            {
                return (T)component;
            }

            return default;
        }

        /// <summary>
        /// 获取实体的所有组件类型
        /// </summary>
        public IEnumerable<Type> GetComponentTypes()
        {
            return _components.Keys;
        }

        /// <summary>
        /// 获取实体的所有组件
        /// </summary>
        public List<T> GetComponents<T>() where T : IComponent
        {
            List<T> result = new List<T>();

            foreach (var component in _components.Values)
            {
                if (component is T typedComponent)
                {
                    result.Add(typedComponent);
                }
            }

            return result;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        public bool RemoveComponent<T>() where T : IComponent
        {
            Type componentType = typeof(T);

            if (_components.TryGetValue(componentType, out var component))
            {
                component.Cleanup();
                _components.Remove(componentType);

                // 通过服务定位器从EntityManager移除注册
                var entityManagerService = ServiceLocator.GetService<IEntityManagerService>();
                entityManagerService?.EntityManager.UnregisterComponentForEntity<T>(_entityId);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否拥有组件
        /// </summary>
        public bool HasComponent<T>() where T : IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 检查是否拥有所有指定组件
        /// </summary>
        public bool HasComponents(params Type[] componentTypes)
        {
            foreach (var type in componentTypes)
            {
                if (!_components.ContainsKey(type))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 清除所有组件
        /// </summary>
        public void ClearComponents()
        {
            foreach (var component in _components.Values)
            {
                component.Cleanup();
            }

            _components.Clear();
        }

        private void OnDestroy()
        {
            ClearComponents();
        }
    }
}