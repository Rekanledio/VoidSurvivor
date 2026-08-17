using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Minimal generic object pool (M7.1) for MonoBehaviour objects. Pre-warms
    /// instances, Get() activates + OnSpawn, Release() OnDespawn + deactivates.
    /// When the pool runs dry it grows on demand (never rejects). Double-release
    /// is guarded. No Update/FixedUpdate allocations; nothing here touches
    /// EventBus/CombatSystem — it is a standalone infrastructure utility.
    /// Existing game objects are NOT wired to this pool yet (M7.2 does that).
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _available = new();
        private readonly HashSet<T> _inPool = new();   // objects currently resting in the pool
        private readonly List<T> _all = new();         // every object this pool manages (for cleanup)

        public int AvailableCount => _available.Count;
        public int TotalCount => _all.Count;

        /// <summary>Creates the pool and pre-warms <paramref name="initialCapacity"/> instances.</summary>
        public ObjectPool(T prefab, int initialCapacity = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < initialCapacity; i++)
            {
                Release(CreateInstance());
            }
        }

        /// <summary>Takes an object from the pool (or creates one if empty) and spawns it.</summary>
        public T Get()
        {
            T obj = _available.Count > 0 ? _available.Pop() : CreateInstance();
            _inPool.Remove(obj); // leaving the pool
            obj.gameObject.SetActive(true);
            if (obj is IPoolable poolable)
            {
                poolable.OnSpawn();
            }
            return obj;
        }

        /// <summary>Returns an object to the pool; double-release is ignored.</summary>
        public void Release(T obj)
        {
            if (obj == null) return;
            if (_inPool.Contains(obj)) return; // already resting in the pool

            _inPool.Add(obj);
            if (!_all.Contains(obj))
            {
                _all.Add(obj);
            }

            if (obj is IPoolable poolable)
            {
                poolable.OnDespawn();
            }
            obj.gameObject.SetActive(false);
            _available.Push(obj);
        }

        /// <summary>
        /// M14 P9 fix: releases every currently-active managed object back to
        /// the pool (stops AI/physics, deactivates). Used at the start of a new
        /// run so no enemy from a previous run survives into the fresh run.
        /// Safe to call during iteration: Release mutates _inPool/_available but
        /// never _all, and double-release is guarded inside Release.
        /// </summary>
        public void ReleaseAllActive()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i] != null && _all[i].gameObject.activeInHierarchy)
                {
                    Release(_all[i]);
                }
            }
        }

        /// <summary>Destroys every managed object and empties the pool.</summary>
        public void Clear()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i] != null)
                {
                    Object.Destroy(_all[i].gameObject);
                }
            }
            _all.Clear();
            _available.Clear();
            _inPool.Clear();
        }

        private T CreateInstance()
        {
            T obj;
            if (_prefab != null)
            {
                obj = Object.Instantiate(_prefab, _parent);
            }
            else
            {
                obj = new GameObject($"PooledObject<{typeof(T).Name}>").AddComponent<T>();
            }
            _all.Add(obj);
            return obj;
        }
    }
}
