using System.Collections.Generic;
using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Boomerang projectile (M6.4, pooled since M7.2.1): flies outward toward the
    /// initial direction until it reaches the configured max distance, then
    /// returns toward the player's CURRENT world position (recomputed every
    /// physics frame) and is released back to the pool when close enough.
    /// Each throw hits a given enemy at most once (per-target HashSet).
    ///
    /// Pooling (M7.2.1): ActiveCount is maintained at the two explicit flight
    /// points — Spawn() increments (entering active flight), DespawnSelf()
    /// decrements (ending the flight) — NOT in OnDespawn, because pool warmup
    /// also runs Release/OnDespawn. OnDespawn clears every piece of runtime
    /// state so a recycled instance starts clean.
    /// </summary>
    [DisallowMultipleComponent]
    public class BoomerangProjectile : MonoBehaviour, IPoolable
    {
        private const float ReturnPickupDistance = 0.5f;
        private const float Lifetime = 15f;
        private const int DefaultPoolCapacity = 4;

        private enum Phase
        {
            Outbound,
            Return,
        }

        private static ObjectPool<BoomerangProjectile> _pool;

        private Rigidbody2D _body;
        private ObjectPool<BoomerangProjectile> _myPool;
        private GameObject _source;
        private Transform _returnTarget;
        private Vector2 _outboundDirection;
        private Vector2 _origin;
        private float _outSpeed;
        private float _returnSpeed;
        private float _maxDistance;
        private float _damage;
        private Phase _phase;
        private bool _initialized;
        private float _lifetimeLeft = Lifetime;
        private readonly HashSet<GameObject> _hitTargets = new();

        /// <summary>Live count of boomerangs in active flight (single-flight rule).</summary>
        public static int ActiveCount;

        /// <summary>Resets the static pool and counter on each play session.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _pool = null;
            ActiveCount = 0;
        }

        /// <summary>Returns the shared pool, creating it once with the given prefab.</summary>
        public static ObjectPool<BoomerangProjectile> EnsurePool(GameObject prefab, int capacity = DefaultPoolCapacity)
        {
            if (_pool == null)
            {
                _pool = new ObjectPool<BoomerangProjectile>(prefab != null ? prefab.GetComponent<BoomerangProjectile>() : null, capacity);
            }
            return _pool;
        }

        /// <summary>Gets a boomerang from the shared pool, marks it active and throws it.</summary>
        public static BoomerangProjectile Spawn(GameObject prefab, Vector2 position, GameObject source, Transform returnTarget, Vector2 direction, float outSpeed, float returnSpeed, float maxDistance, float damage)
        {
            var pool = EnsurePool(prefab);
            var boomerang = pool.Get();
            ActiveCount++; // entering active flight
            boomerang._myPool = pool;
            boomerang.transform.position = position;
            boomerang.Init(source, returnTarget, direction, outSpeed, returnSpeed, maxDistance, damage);
            return boomerang;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Sets source, return target, direction and flight params; called once right after spawn.</summary>
        public void Init(GameObject source, Transform returnTarget, Vector2 direction, float outSpeed, float returnSpeed, float maxDistance, float damage)
        {
            _source = source;
            _returnTarget = returnTarget;
            _outboundDirection = direction.normalized;
            _outSpeed = outSpeed;
            _returnSpeed = returnSpeed;
            _maxDistance = maxDistance;
            _damage = damage;
            _origin = _body.position;
            _phase = Phase.Outbound;
            _initialized = true;
        }

        public void OnSpawn()
        {
            // Per-throw state is injected by Init right after Get; nothing extra needed.
        }

        public void OnDespawn()
        {
            // Stop the flight completely and clear ALL runtime state so a
            // recycled instance cannot move, collide, deal damage or keep
            // stale per-throw values (origin/phase/source/direction/timer/hits).
            _initialized = false;
            _source = null;
            _returnTarget = null;
            _outboundDirection = Vector2.zero;
            _origin = Vector2.zero;
            _outSpeed = 0f;
            _returnSpeed = 0f;
            _maxDistance = 0f;
            _damage = 0f;
            _phase = Phase.Outbound;
            _lifetimeLeft = Lifetime;
            _hitTargets.Clear();
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            if (_phase == Phase.Outbound)
            {
                Vector2 next = _body.position + _outboundDirection * (_outSpeed * Time.fixedDeltaTime);
                _body.MovePosition(next);

                if ((_body.position - _origin).magnitude >= _maxDistance)
                {
                    _phase = Phase.Return;
                }
            }
            else
            {
                if (_returnTarget == null)
                {
                    DespawnSelf();
                    return;
                }

                Vector2 toPlayer = (Vector2)_returnTarget.position - _body.position;
                float distance = toPlayer.magnitude;
                if (distance <= ReturnPickupDistance)
                {
                    DespawnSelf();
                    return;
                }

                Vector2 next = _body.position + toPlayer.normalized * (_returnSpeed * Time.fixedDeltaTime);
                _body.MovePosition(next);
            }

            _lifetimeLeft -= Time.fixedDeltaTime;
            if (_lifetimeLeft <= 0f)
            {
                DespawnSelf();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized) return; // never hit after release
            if (other.gameObject == _source) return;
            if (_hitTargets.Contains(other.gameObject)) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                _hitTargets.Add(other.gameObject); // once per enemy per throw
                CombatSystem.ApplyDamage(new DamageRequest(_source, other.gameObject, _damage));
            }
        }

        private void DespawnSelf()
        {
            ActiveCount--; // ending active flight (only ever called on an active flight)
            if (_myPool != null)
            {
                _myPool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
