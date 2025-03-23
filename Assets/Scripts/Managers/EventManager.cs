using System;
using System.Collections.Generic;
using UnityEngine;
namespace SuperRobot
{
    public class EventManager
    {
        private static EventManager _instance;
        public static EventManager Instance => _instance ??= new EventManager();
        
        // 事件处理器映射
        private Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();
        
        // 延迟执行的事件队列
        private Queue<(Type eventType, object eventData)> _eventQueue = new Queue<(Type, object)>();
        private bool _isProcessingEvents = false;
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            
            // 防止重复订阅
            if (!_eventHandlers[eventType].Contains(handler))
            {
                _eventHandlers[eventType].Add(handler);
            }
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Type eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                return;
            }
            
            _eventHandlers[eventType].Remove(handler);
            
            // 如果没有更多处理器，移除整个键
            if (_eventHandlers[eventType].Count == 0)
            {
                _eventHandlers.Remove(eventType);
            }
        }
        
        /// <summary>
        /// 触发事件（立即执行）
        /// </summary>
        public void TriggerEvent<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                return;
            }
            
            // 创建处理器列表的副本，防止回调中修改列表
            var handlers = new List<Delegate>(_eventHandlers[eventType]);
            Debug.Log($"TriggerEvent: {eventType.Name}");
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler).Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking event handler for {eventType.Name}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
        
        /// <summary>
        /// 入队事件（延迟执行）
        /// </summary>
        public void QueueEvent<T>(T eventData) where T : struct
        {
            _eventQueue.Enqueue((typeof(T), eventData));
        }
        
        /// <summary>
        /// 处理事件队列
        /// </summary>
        public void ProcessEventQueue()
        {
            if (_isProcessingEvents)
                return;
            
            _isProcessingEvents = true;
            
            try
            {
                // 处理当前队列中的所有事件
                int initialCount = _eventQueue.Count;
                for (int i = 0; i < initialCount; i++)
                {
                    if (_eventQueue.Count > 0)
                    {
                        var (eventType, eventData) = _eventQueue.Dequeue();
                        TriggerEventInternal(eventType, eventData);
                    }
                }
            }
            finally
            {
                _isProcessingEvents = false;
            }
            
            // 如果处理事件时产生了新事件，继续处理
            if (_eventQueue.Count > 0)
            {
                ProcessEventQueue();
            }
        }
        
        /// <summary>
        /// 内部事件触发方法
        /// </summary>
        private void TriggerEventInternal(Type eventType, object eventData)
        {
            if (!_eventHandlers.ContainsKey(eventType))
            {
                return;
            }
            
            var handlers = new List<Delegate>(_eventHandlers[eventType]);
            
            foreach (var handler in handlers)
            {
                try
                {
                    // 使用反射调用处理器
                    handler.Method.Invoke(handler.Target, new[] { eventData });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking event handler for {eventType.Name}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
        
        /// <summary>
        /// 清除特定类型的所有事件处理器
        /// </summary>
        public void ClearEventHandlers<T>() where T : struct
        {
            Type eventType = typeof(T);
            
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers.Remove(eventType);
            }
        }
        
        /// <summary>
        /// 清除所有事件处理器
        /// </summary>
        public void ClearAllEventHandlers()
        {
            _eventHandlers.Clear();
        }
        
        /// <summary>
        /// 清空事件队列
        /// </summary>
        public void ClearEventQueue()
        {
            _eventQueue.Clear();
        }
        
        /// <summary>
        /// 获取某事件类型的订阅者数量
        /// </summary>
        public int GetSubscriberCount<T>() where T : struct
        {
            Type eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var handlers))
            {
                return handlers.Count;
            }
            
            return 0;
        }
    }
}