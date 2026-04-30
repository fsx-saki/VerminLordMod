using System;
using System.Collections.Generic;

namespace VerminLordMod.Common.Events
{
    /// <summary>
    /// 轻量级事件总线（D-03 自建实现）
    /// 
    /// MVA 阶段：最简字典实现，线程不安全。
    ///   - Publish<T>(T) : 发布事件，同步调用所有订阅者
    ///   - Subscribe<T>(Action<T>) : 订阅事件
    ///   - Unsubscribe<T>(Action<T>) : 取消订阅
    /// 
    /// P1 扩展计划：
    ///   - 订阅优先级排序
    ///   - 错误隔离（单个订阅者异常不影响其他订阅者）
    ///   - 异步/延迟事件队列
    ///   - 事件追踪/日志
    /// 
    /// 使用示例：
    ///   EventBus.Subscribe<NPCDeathEvent>(OnNPCDeath);
    ///   EventBus.Publish(new NPCDeathEvent { NPCType = 1, ... });
    /// </summary>
    public static class EventBus
    {
        // 事件类型 -> 订阅者列表
        private static readonly Dictionary<Type, Delegate> _subscribers = new();

        /// <summary> 发布事件，自动填充 Tick 和 SourceFaction </summary>
        public static void Publish<T>(T eventData) where T : GuWorldEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var del))
            {
                if (del is Action<T> action)
                {
                    action.Invoke(eventData);
                }
            }
        }

        /// <summary> 订阅事件 </summary>
        public static void Subscribe<T>(Action<T> handler) where T : GuWorldEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var del))
            {
                _subscribers[type] = Delegate.Combine(del, handler);
            }
            else
            {
                _subscribers[type] = handler;
            }
        }

        /// <summary> 取消订阅 </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : GuWorldEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var del))
            {
                var newDel = Delegate.Remove(del, handler);
                if (newDel == null)
                {
                    _subscribers.Remove(type);
                }
                else
                {
                    _subscribers[type] = newDel;
                }
            }
        }

        /// <summary> 清空所有订阅（用于世界卸载时重置） </summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }
    }
}
