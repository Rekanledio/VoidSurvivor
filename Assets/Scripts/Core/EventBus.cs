using System;
using System.Collections.Generic;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Lightweight, type-safe event bus for decoupled cross-system communication.
    /// Subscribe with a struct event type + handler; Unsubscribe with the same handler.
    /// Designed to run on the Unity main thread only.
    /// </summary>
    /// <typeparam name="TEvent">Must be a struct to avoid boxing allocations.</typeparam>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new();

        /// <summary>Registers a handler. Duplicate subscriptions are ignored.</summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            if (handler == null) return;

            Type key = typeof(TEvent);
            if (Handlers.TryGetValue(key, out Delegate existing))
            {
                Handlers[key] = Delegate.Combine(existing, handler);
            }
            else
            {
                Handlers[key] = handler;
            }
        }

        /// <summary>Removes a previously registered handler. Safe to call when not subscribed.</summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            if (handler == null) return;

            Type key = typeof(TEvent);
            if (!Handlers.TryGetValue(key, out Delegate existing)) return;

            Delegate combined = Delegate.Remove(existing, handler);
            if (combined == null)
            {
                Handlers.Remove(key);
            }
            else
            {
                Handlers[key] = combined;
            }
        }

        /// <summary>Invokes every handler registered for TEvent. No-op when nobody is subscribed.</summary>
        public static void Publish<TEvent>(in TEvent evt) where TEvent : struct
        {
            if (!Handlers.TryGetValue(typeof(TEvent), out Delegate d)) return;

            // The delegate stored under typeof(TEvent) is always Action<TEvent> by construction.
            ((Action<TEvent>)d)?.Invoke(evt);
        }

        /// <summary>Drops all subscriptions. Useful for tests or full resets.</summary>
        public static void Clear()
        {
            Handlers.Clear();
        }
    }
}
