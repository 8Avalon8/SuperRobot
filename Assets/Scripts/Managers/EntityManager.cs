using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    // 实体组件接口
    public interface IComponent
    {
        void Initialize();
        void Cleanup();
    }

    public class EntityManager
    {
        // 实体存储
        private Dictionary<int, GameEntity> _entities = new Dictionary<int, GameEntity>();
        private int _nextEntityId = 1;
        
        // 用于快速查询的索引
        private Dictionary<Type, HashSet<int>> _componentTypeToEntities = new Dictionary<Type, HashSet<int>>();
        
        /// <summary>
        /// 创建一个新的实体
        /// </summary>
        public GameEntity CreateEntity(string entityType)
        {
            // 创建GameObject作为实体容器
            GameObject entityObject = new GameObject($"{entityType}_{_nextEntityId}");
            GameEntity entity       = entityObject.AddComponent<GameEntity>();
            entity.Initialize(_nextEntityId);
    
            // 添加可视化组件
            var visualizer = entityObject.AddComponent<EntityVisualizer>();
            visualizer.Initialize(entity, _nextEntityId, entityType);
    
            // 添加引用组件
            var reference = entityObject.AddComponent<EntityReference>();
            reference.EntityId = _nextEntityId;
    
            // 注册实体
            _entities[_nextEntityId] = entity;
    
            // 递增ID
            _nextEntityId++;
    
            return entity;
        }
        
        /// <summary>
        /// 销毁指定ID的实体
        /// </summary>
        public bool DestroyEntity(int entityId)
        {
            if (!_entities.TryGetValue(entityId, out var entity))
                return false;
            
            // 从所有组件索引中移除
            foreach (var componentType in entity.GetComponentTypes())
            {
                if (_componentTypeToEntities.TryGetValue(componentType, out var entitiesWithType))
                {
                    entitiesWithType.Remove(entityId);
                }
            }
            
            // 从实体字典中移除
            _entities.Remove(entityId);
            
            // 销毁GameObject
            GameObject.Destroy(entity.gameObject);
            
            return true;
        }
        
        /// <summary>
        /// 获取指定ID的实体
        /// </summary>
        public GameEntity GetEntity(int entityId)
        {
            if (_entities.TryGetValue(entityId, out var entity))
                return entity;
            
            return null;
        }
        
        /// <summary>
        /// 注册组件类型与实体的关联
        /// </summary>
        public void RegisterComponentForEntity<T>(int entityId) where T : IComponent
        {
            Type componentType = typeof(T);
            
            if (!_componentTypeToEntities.TryGetValue(componentType, out var entities))
            {
                entities = new HashSet<int>();
                _componentTypeToEntities[componentType] = entities;
            }
            
            entities.Add(entityId);
        }
        
        /// <summary>
        /// 移除组件类型与实体的关联
        /// </summary>
        public void UnregisterComponentForEntity<T>(int entityId) where T : IComponent
        {
            Type componentType = typeof(T);
            
            if (_componentTypeToEntities.TryGetValue(componentType, out var entities))
            {
                entities.Remove(entityId);
            }
        }
        
        /// <summary>
        /// 获取拥有指定组件的所有实体
        /// </summary>
        public List<GameEntity> GetEntitiesWithComponent<T>() where T : IComponent
        {
            Type componentType = typeof(T);
            List<GameEntity> result = new List<GameEntity>();
            
            if (_componentTypeToEntities.TryGetValue(componentType, out var entityIds))
            {
                foreach (var id in entityIds)
                {
                    if (_entities.TryGetValue(id, out var entity))
                    {
                        result.Add(entity);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取同时拥有多个指定组件的所有实体
        /// </summary>
        public List<GameEntity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            if (componentTypes.Length == 0)
                return new List<GameEntity>();
            
            // 获取第一个组件类型的实体
            Type firstType = componentTypes[0];
            HashSet<int> resultIds = new HashSet<int>();
            
            if (_componentTypeToEntities.TryGetValue(firstType, out var firstEntities))
            {
                resultIds.UnionWith(firstEntities);
            }
            else
            {
                return new List<GameEntity>();
            }
            
            // 与其他组件类型的实体集合求交集
            for (int i = 1; i < componentTypes.Length; i++)
            {
                if (_componentTypeToEntities.TryGetValue(componentTypes[i], out var entities))
                {
                    resultIds.IntersectWith(entities);
                }
                else
                {
                    return new List<GameEntity>();
                }
            }
            
            // 转换为实体列表
            List<GameEntity> result = new List<GameEntity>();
            foreach (var id in resultIds)
            {
                if (_entities.TryGetValue(id, out var entity))
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取所有实体
        /// </summary>
        public List<GameEntity> GetAllEntities()
        {
            return new List<GameEntity>(_entities.Values);
        }
        
        /// <summary>
        /// 清除所有实体
        /// </summary>
        public void ClearAllEntities()
        {
            foreach (var entity in _entities.Values)
            {
                GameObject.Destroy(entity.gameObject);
            }
            
            _entities.Clear();
            _componentTypeToEntities.Clear();
        }


        // 获取所有在某个位置的实体
        public List<GameEntity> GetEntitiesAtPosition(Vector2Int position)
        {
            var entities = GetEntitiesWithComponent<PositionComponent>();
            List<GameEntity> result = new List<GameEntity>();
            foreach (var entity in entities)
            {
                if (entity.GetComponent<PositionComponent>().Position == position)
                {
                    result.Add(entity);
                }
            }
            return result;
        }
    }
}