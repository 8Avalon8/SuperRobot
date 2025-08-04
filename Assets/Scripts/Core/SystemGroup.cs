using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot.Core
{
    /// <summary>
    /// 系统执行阶段枚举
    /// </summary>
    public enum SystemPhase
    {
        Initialization,  // 初始化阶段
        PreUpdate,      // 更新前阶段
        Update,         // 主更新阶段
        PostUpdate,     // 更新后阶段
        Render,         // 渲染阶段
        Cleanup         // 清理阶段
    }

    /// <summary>
    /// 系统优先级枚举
    /// </summary>
    public enum SystemPriority
    {
        Critical = 0,   // 关键系统（输入、物理等）
        High = 1,       // 高优先级（游戏逻辑）
        Normal = 2,     // 普通优先级（大部分系统）
        Low = 3,        // 低优先级（UI、音效等）
        Background = 4  // 后台系统（保存、统计等）
    }

    /// <summary>
    /// 系统信息
    /// </summary>
    public class SystemInfo
    {
        public GameSystem System { get; set; }
        public Type SystemType { get; set; }
        public SystemPhase Phase { get; set; }
        public SystemPriority Priority { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsInitialized { get; set; } = false;
        public float LastExecutionTime { get; set; } = 0f;
        public int ExecutionCount { get; set; } = 0;

        public SystemInfo(GameSystem system, SystemPhase phase = SystemPhase.Update, SystemPriority priority = SystemPriority.Normal)
        {
            System = system;
            SystemType = system.GetType();
            Phase = phase;
            Priority = priority;
        }
    }

    /// <summary>
    /// 系统组 - 管理同一阶段的系统执行
    /// </summary>
    public class SystemGroup
    {
        public SystemPhase Phase { get; private set; }
        private List<SystemInfo> _systems = new List<SystemInfo>();
        private List<SystemInfo> _enabledSystems = new List<SystemInfo>();
        private bool _needsSorting = false;

        public SystemGroup(SystemPhase phase)
        {
            Phase = phase;
        }

        public void AddSystem(SystemInfo systemInfo)
        {
            if (systemInfo.Phase != Phase)
            {
                Debug.LogWarning($"System {systemInfo.SystemType.Name} has phase {systemInfo.Phase} but is being added to {Phase} group");
                systemInfo.Phase = Phase;
            }

            _systems.Add(systemInfo);
            if (systemInfo.IsEnabled)
            {
                _enabledSystems.Add(systemInfo);
            }
            _needsSorting = true;
        }

        public bool RemoveSystem(Type systemType)
        {
            var systemInfo = _systems.Find(s => s.SystemType == systemType);
            if (systemInfo != null)
            {
                _systems.Remove(systemInfo);
                _enabledSystems.Remove(systemInfo);
                return true;
            }
            return false;
        }

        public void EnableSystem(Type systemType)
        {
            var systemInfo = _systems.Find(s => s.SystemType == systemType);
            if (systemInfo != null && !systemInfo.IsEnabled)
            {
                systemInfo.IsEnabled = true;
                _enabledSystems.Add(systemInfo);
                _needsSorting = true;
            }
        }

        public void DisableSystem(Type systemType)
        {
            var systemInfo = _systems.Find(s => s.SystemType == systemType);
            if (systemInfo != null && systemInfo.IsEnabled)
            {
                systemInfo.IsEnabled = false;
                _enabledSystems.Remove(systemInfo);
            }
        }

        public SystemInfo GetSystemInfo(Type systemType)
        {
            return _systems.Find(s => s.SystemType == systemType);
        }

        public void ExecuteSystems()
        {
            if (_needsSorting)
            {
                SortSystems();
                _needsSorting = false;
            }

            foreach (var systemInfo in _enabledSystems)
            {
                if (systemInfo.IsEnabled)
                {
                    var startTime = Time.realtimeSinceStartup;
                    
                    try
                    {
                        systemInfo.System.Execute();
                        systemInfo.ExecutionCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error executing system {systemInfo.SystemType.Name}: {ex.Message}");
                        Debug.LogException(ex);
                    }
                    
                    systemInfo.LastExecutionTime = Time.realtimeSinceStartup - startTime;
                }
            }
        }

        private void SortSystems()
        {
            _enabledSystems.Sort((a, b) => 
            {
                // 首先按优先级排序
                int priorityComparison = a.Priority.CompareTo(b.Priority);
                if (priorityComparison != 0)
                    return priorityComparison;
                
                // 优先级相同时按系统类型名称排序以保持一致性
                return string.Compare(a.SystemType.Name, b.SystemType.Name, StringComparison.Ordinal);
            });
        }

        public List<SystemInfo> GetAllSystems()
        {
            return new List<SystemInfo>(_systems);
        }

        public List<SystemInfo> GetEnabledSystems()
        {
            return new List<SystemInfo>(_enabledSystems);
        }

        public void CleanupAllSystems()
        {
            foreach (var systemInfo in _systems)
            {
                try
                {
                    systemInfo.System.Cleanup();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error cleaning up system {systemInfo.SystemType.Name}: {ex.Message}");
                }
            }
        }
    }
}