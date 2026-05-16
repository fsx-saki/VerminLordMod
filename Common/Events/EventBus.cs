using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Events
{
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
