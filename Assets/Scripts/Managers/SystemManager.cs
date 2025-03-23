using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    public class SystemManager
    {
        // 系统存储
    private Dictionary<Type, GameSystem> _systems = new Dictionary<Type, GameSystem>();
    
    // 更新顺序
    private List<GameSystem> _orderedSystems = new List<GameSystem>();
    
    // 系统启用状态
    private HashSet<Type> _enabledSystems = new HashSet<Type>();
    
    /// <summary>
    /// 注册系统
    /// </summary>
    public T RegisterSystem<T>(T system) where T : GameSystem
    {
        Type systemType = typeof(T);
        
        if (_systems.ContainsKey(systemType))
        {
            Debug.LogWarning($"System of type {systemType.Name} already registered.");
            return (T)_systems[systemType];
        }
        
        _systems[systemType] = system;
        _orderedSystems.Add(system);
        _enabledSystems.Add(systemType);
        
        // 初始化系统
        system.Initialize();
        
        return system;
    }
    
    /// <summary>
    /// 获取系统
    /// </summary>
    public T GetSystem<T>() where T : GameSystem
    {
        Type systemType = typeof(T);
        
        if (_systems.TryGetValue(systemType, out var system))
        {
            return (T)system;
        }
        
        return null;
    }
    
    /// <summary>
    /// 禁用系统
    /// </summary>
    public void DisableSystem<T>() where T : GameSystem
    {
        Type systemType = typeof(T);
        
        if (_enabledSystems.Contains(systemType))
        {
            _enabledSystems.Remove(systemType);
        }
    }
    
    /// <summary>
    /// 启用系统
    /// </summary>
    public void EnableSystem<T>() where T : GameSystem
    {
        Type systemType = typeof(T);
        
        if (_systems.ContainsKey(systemType) && !_enabledSystems.Contains(systemType))
        {
            _enabledSystems.Add(systemType);
        }
    }
    
    /// <summary>
    /// 执行所有启用的系统
    /// </summary>
    public void ExecuteActiveSystems()
    {
        foreach (var system in _orderedSystems)
        {
            Type systemType = system.GetType();
            
            if (_enabledSystems.Contains(systemType))
            {
                system.Execute();
            }
        }
    }
    
    /// <summary>
    /// 设置系统执行顺序
    /// </summary>
    public void SetSystemOrder(List<GameSystem> orderedSystems)
    {
        // 检查所有系统是否都注册了
        foreach (var system in orderedSystems)
        {
            Type systemType = system.GetType();
            
            if (!_systems.ContainsKey(systemType))
            {
                Debug.LogError($"System of type {systemType.Name} is not registered.");
                return;
            }
        }
        
        // 检查是否包含了所有系统
        if (orderedSystems.Count != _systems.Count)
        {
            Debug.LogError("Ordered systems list does not contain all registered systems.");
            return;
        }
        
        _orderedSystems = new List<GameSystem>(orderedSystems);
    }
    
    /// <summary>
    /// 清理所有系统
    /// </summary>
    public void CleanupAllSystems()
    {
        foreach (var system in _orderedSystems)
        {
            system.Cleanup();
        }
    }
    
    /// <summary>
    /// 重置所有系统
    /// </summary>
    public void ResetAllSystems()
    {
        foreach (var system in _orderedSystems)
        {
            system.Cleanup();
            system.Initialize();
        }
    }
    }
}