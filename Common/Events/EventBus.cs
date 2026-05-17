using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Events
{
    /// <summary>
    /// 事件总线 — 蛊世界事件发布/订阅系统
    /// 
    /// 基于类型的发布-订阅模式，各系统通过 Subscribe 注册事件处理器，
    /// 通过 Publish 发布事件。所有事件必须继承自 GuWorldEvent。
    /// 
    /// 使用示例：
    /// - 订阅：EventBus.Subscribe&lt;PlayerQiChangedEvent&gt;(OnQiChanged);
    /// - 发布：EventBus.Publish(new PlayerQiChangedEvent { ... });
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _subscribers = new();

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

        public static void Clear()
        {
            _subscribers.Clear();
        }
    }

    public class EventBusWorldHook : ModSystem
    {
        public override void OnWorldUnload()
        {
            EventBus.Clear();
        }
    }
}
