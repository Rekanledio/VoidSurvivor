using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Shooter projectile (M5.1, pooled since M7.2.1): flies in a fixed direction
    /// at a fixed speed, expires after a lifetime, and on contact routes a
    /// damage request through <see cref="CombatSystem"/> to whatever
    /// <see cref="IDamageable"/> the hit object exposes. No direct PlayerHealth
    /// coupling; the unified combat pipeline owns damage application.
    ///
    /// Pooling (M7.2.1): spawned from a shared <see cref="ObjectPool{T}"/>;
    /// lifetime/hit despawn RELEASES back to the pool. OnDespawn resets runtime
    /// state; Init still injects per-shot parameters. Damage/speed/lifetime
    /// semantics unchanged.
    /// </summary>
    [DisallowMultipleComponent]
    public class Projectile : MonoBehaviour, IPoolable
    {
        private const float Speed = 8f;
        private const float Lifetime = 3f;
        private const int DefaultPoolCapacity = 16;

        private static ObjectPool<Projectile> _pool;

        private Rigidbody2D _body;
        private ObjectPool<Projectile> _myPool;
        private float _damage;
        private GameObject _source;
        private bool _initialized;
        private float _lifetimeLeft = Lifetime;

        /// <summary>Resets the static pool on each play session.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticPool()
        {
            _pool = null;
        }

        /// <summary>Returns the shared pool, creating it once with the given prefab.</summary>
        public static ObjectPool<Projectile> EnsurePool(GameObject prefab, int capacity = DefaultPoolCapacity)
        {
            if (_pool == null)
            {
                _pool = new ObjectPool<Projectile>(prefab != null ? prefab.GetComponent<Projectile>() : null, capacity);
            }
            return _pool;
        }

        /// <summary>Gets a projectile from the shared pool and launches it.</summary>
        public static Projectile Spawn(GameObject prefab, Vector2 position, Vector2 direction, float damage, GameObject source)
        {
            var pool = EnsurePool(prefab);
            var projectile = pool.Get();
            projectile._myPool = pool;
            projectile.transform.position = position;
            projectile.Init(direction, damage, source);
            return projectile;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Sets flight direction, damage and source; called once right after spawn.</summary>
        public void Init(Vector2 direction, float damage, GameObject source)
        {
            _damage = damage;
            _source = source;
            _body.linearVelocity = direction.normalized * Speed;
            _initialized = true;
        }

        public void OnSpawn()
        {
            // Per-shot state is injected by Init right after Get; nothing extra needed.
        }

        public void OnDespawn()
        {
            _initialized = false;
            _body.linearVelocity = Vector2.zero;
            _damage = 0f;
            _source = null;
            _lifetimeLeft = Lifetime;
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            // Lifetime expiry (moves via Rigidbody2D.velocity automatically).
            _lifetimeLeft -= Time.fixedDeltaTime;
            if (_lifetimeLeft <= 0f)
            {
                DespawnSelf();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized) return; // never deal damage after release

            // Route damage through the unified combat entry (event-driven, not a hot path).
            if (other.TryGetComponent(out IDamageable damageable))
            {
                CombatSystem.ApplyDamage(new DamageRequest(_source, other.gameObject, _damage));
                DespawnSelf();
            }
        }

        private void DespawnSelf()
        {
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
