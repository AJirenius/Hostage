using System;
using System.Collections.Generic;

namespace Hostage.Core
{
    public class SignalBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscriptions.TryGetValue(type, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscriptions[type] = handlers;
            }
            handlers.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_subscriptions.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Publish<T>(T signal = default) where T : struct
        {
            var type = typeof(T);
            if (_subscriptions.TryGetValue(type, out var handlers))
            {
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    ((Action<T>)handlers[i]).Invoke(signal);
                }
            }
        }
    }
}
