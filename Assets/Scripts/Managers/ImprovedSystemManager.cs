using System;
using System.Collections.Generic;
using UnityEngine;
using SuperRobot.Core;
using SystemInfo = SuperRobot.Core.SystemInfo;

namespace SuperRobot
{
    /// <summary>
    /// 改进的系统管理器 - 支持分组执行和优先级
    /// </summary>
    public class ImprovedSystemManager
    {
        // 系统组按阶段分类
        private Dictionary<SystemPhase, SystemGroup> _systemGroups = new Dictionary<SystemPhase, SystemGroup>();
        
        // 系统信息存储
        private Dictionary<Type, SystemInfo> _systemInfos = new Dictionary<Type, SystemInfo>();
        
        // 服务容器引用
        private IServiceContainer _serviceContainer;

        // 性能统计
        public bool EnableProfiling { get; set; } = false;
        private Dictionary<SystemPhase, float> _phaseExecutionTimes = new Dictionary<SystemPhase, float>();

        public ImprovedSystemManager()
        {
            // 初始化系统组
            foreach (SystemPhase phase in Enum.GetValues(typeof(SystemPhase)))
            {
                _systemGroups[phase] = new SystemGroup(phase);
                _phaseExecutionTimes[phase] = 0f;
            }
        }

        /// <summary>
        /// 设置服务容器
        /// </summary>
        public void SetServiceContainer(IServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
        }

        /// <summary>
        /// 注册系统
        /// </summary>
        public T RegisterSystem<T>(T system, SystemPhase phase = SystemPhase.Update, SystemPriority priority = SystemPriority.Normal) where T : GameSystem
        {
            Type systemType = typeof(T);
            
            if (_systemInfos.ContainsKey(systemType))
            {
                Debug.LogWarning($"System of type {systemType.Name} already registered.");
                return (T)_systemInfos[systemType].System;
            }

            // 创建系统信息
            var systemInfo = new SystemInfo(system, phase, priority);
            _systemInfos[systemType] = systemInfo;

            // 添加到对应的系统组
            _systemGroups[phase].AddSystem(systemInfo);

            // 注入服务依赖
            if (_serviceContainer != null)
            {
                system.InjectServices(_serviceContainer);
            }

            // 初始化系统
            try
            {
                system.Initialize();
                systemInfo.IsInitialized = true;
                Debug.Log($"System {systemType.Name} initialized successfully in {phase} phase with {priority} priority");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize system {systemType.Name}: {ex.Message}");
                Debug.LogException(ex);
                systemInfo.IsEnabled = false;
            }

            return system;
        }

        /// <summary>
        /// 获取系统
        /// </summary>
        public T GetSystem<T>() where T : GameSystem
        {
            Type systemType = typeof(T);
            
            if (_systemInfos.TryGetValue(systemType, out var systemInfo))
            {
                return (T)systemInfo.System;
            }
            
            return null;
        }

        /// <summary>
        /// 启用系统
        /// </summary>
        public void EnableSystem<T>() where T : GameSystem
        {
            Type systemType = typeof(T);
            
            if (_systemInfos.TryGetValue(systemType, out var systemInfo))
            {
                _systemGroups[systemInfo.Phase].EnableSystem(systemType);
                Debug.Log($"System {systemType.Name} enabled");
            }
        }

        /// <summary>
        /// 禁用系统
        /// </summary>
        public void DisableSystem<T>() where T : GameSystem
        {
            Type systemType = typeof(T);
            
            if (_systemInfos.TryGetValue(systemType, out var systemInfo))
            {
                _systemGroups[systemInfo.Phase].DisableSystem(systemType);
                Debug.Log($"System {systemType.Name} disabled");
            }
        }

        /// <summary>
        /// 执行指定阶段的系统
        /// </summary>
        public void ExecutePhase(SystemPhase phase)
        {
            if (_systemGroups.TryGetValue(phase, out var systemGroup))
            {
                float startTime = 0f;
                if (EnableProfiling)
                {
                    startTime = Time.realtimeSinceStartup;
                }

                systemGroup.ExecuteSystems();

                if (EnableProfiling)
                {
                    _phaseExecutionTimes[phase] = Time.realtimeSinceStartup - startTime;
                }
            }
        }

        /// <summary>
        /// 执行所有系统（按阶段顺序）
        /// </summary>
        public void ExecuteAllSystems()
        {
            ExecutePhase(SystemPhase.PreUpdate);
            ExecutePhase(SystemPhase.Update);
            ExecutePhase(SystemPhase.PostUpdate);
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        public SystemInfo GetSystemInfo<T>() where T : GameSystem
        {
            Type systemType = typeof(T);
            _systemInfos.TryGetValue(systemType, out var systemInfo);
            return systemInfo;
        }

        /// <summary>
        /// 获取阶段执行时间
        /// </summary>
        public float GetPhaseExecutionTime(SystemPhase phase)
        {
            return _phaseExecutionTimes.TryGetValue(phase, out var time) ? time : 0f;
        }

        /// <summary>
        /// 获取所有阶段的系统信息
        /// </summary>
        public Dictionary<SystemPhase, List<SystemInfo>> GetAllSystemInfos()
        {
            var result = new Dictionary<SystemPhase, List<SystemInfo>>();
            
            foreach (var kvp in _systemGroups)
            {
                result[kvp.Key] = kvp.Value.GetAllSystems();
            }
            
            return result;
        }

        /// <summary>
        /// 清理所有系统
        /// </summary>
        public void CleanupAllSystems()
        {
            foreach (var systemGroup in _systemGroups.Values)
            {
                systemGroup.CleanupAllSystems();
            }
            
            _systemInfos.Clear();
            Debug.Log("All systems cleaned up");
        }

        /// <summary>
        /// 重置所有系统
        /// </summary>
        public void ResetAllSystems()
        {
            foreach (var systemInfo in _systemInfos.Values)
            {
                try
                {
                    systemInfo.System.Cleanup();
                    systemInfo.System.Initialize();
                    systemInfo.IsInitialized = true;
                    systemInfo.ExecutionCount = 0;
                    systemInfo.LastExecutionTime = 0f;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to reset system {systemInfo.SystemType.Name}: {ex.Message}");
                    systemInfo.IsEnabled = false;
                }
            }
            
            Debug.Log("All systems reset");
        }

        /// <summary>
        /// 打印性能统计信息
        /// </summary>
        public void PrintPerformanceStats()
        {
            if (!EnableProfiling)
            {
                Debug.Log("Profiling is disabled. Enable profiling to see performance stats.");
                return;
            }

            Debug.Log("=== System Performance Stats ===");
            
            foreach (var phase in Enum.GetValues(typeof(SystemPhase)))
            {
                var phaseTime = GetPhaseExecutionTime((SystemPhase)phase);
                if (phaseTime > 0)
                {
                    Debug.Log($"{phase} Phase: {phaseTime * 1000:F2}ms");
                    
                    var systems = _systemGroups[(SystemPhase)phase].GetEnabledSystems();
                    foreach (var systemInfo in systems)
                    {
                        Debug.Log($"  - {systemInfo.SystemType.Name}: {systemInfo.LastExecutionTime * 1000:F2}ms (executed {systemInfo.ExecutionCount} times)");
                    }
                }
            }
        }
    }
}