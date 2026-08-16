using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Pulse Gun projectile (M6.2, pooled since M7.2.1): single-target, straight
    /// flight toward the direction captured at spawn, fixed speed, lifetime
    /// expiry; on contact routes a DamageRequest through <see cref="CombatSystem"/>
    /// with the player as Source. Skips its own Source object. No pierce/explode.
    ///
    /// Pooling (M7.2.1): one static <see cref="ObjectPool{T}"/> is shared by
    /// every spawner (Pulse Gun and Scatter Blaster both use this type, so they
    /// reuse the same pool). Lifetime/hit despawn RELEASES back to the pool
    /// instead of destroying. OnDespawn resets all runtime state so a recycled
    /// instance starts clean; Init still injects the per-shot parameters.
    /// </summary>
    [DisallowMultipleComponent]
    public class PulseProjectile : MonoBehaviour, IPoolable
    {
        private const float Lifetime = 3f;
        private const int DefaultPoolCapacity = 16;

        private static ObjectPool<PulseProjectile> _pool;

        private Rigidbody2D _body;
        private ObjectPool<PulseProjectile> _myPool;
        private GameObject _source;
        private Vector2 _direction;
        private float _damage;
        private bool _initialized;
        private float _lifetimeLeft = Lifetime;

        /// <summary>Resets the static pool on each play session (static fields persist across plays).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticPool()
        {
            _pool = null;
        }

        /// <summary>Returns the shared pool, creating it once with the given prefab.</summary>
        public static ObjectPool<PulseProjectile> EnsurePool(GameObject prefab, int capacity = DefaultPoolCapacity)
        {
            if (_pool == null)
            {
                _pool = new ObjectPool<PulseProjectile>(prefab != null ? prefab.GetComponent<PulseProjectile>() : null, capacity);
            }
            return _pool;
        }

        /// <summary>Gets a projectile from the shared pool and launches it.</summary>
        public static PulseProjectile Spawn(GameObject prefab, Vector2 position, GameObject source, Vector2 direction, float speed, float damage)
        {
            var projectile = EnsurePool(prefab).Get();
            projectile.transform.position = position;
            projectile._myPool = EnsurePool(prefab);
            projectile.Init(source, direction, speed, damage);
            return projectile;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Sets source, flight direction, speed and damage; called once right after spawn.</summary>
        public void Init(GameObject source, Vector2 direction, float speed, float damage)
        {
            _source = source;
            _direction = direction.normalized;
            _damage = damage;
            _body.linearVelocity = _direction * speed;
            _initialized = true;
        }

        public void OnSpawn()
        {
            // Per-shot state is injected by Init right after Get; nothing extra needed.
        }

        public void OnDespawn()
        {
            // Stop the projectile completely and clear runtime state so a
            // recycled instance cannot move, collide or deal damage.
            _initialized = false;
            _body.linearVelocity = Vector2.zero;
            _source = null;
            _direction = Vector2.zero;
            _damage = 0f;
            _lifetimeLeft = Lifetime;
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            _lifetimeLeft -= Time.fixedDeltaTime;
            if (_lifetimeLeft <= 0f)
            {
                DespawnSelf();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized) return; // never deal damage after release
            if (other.gameObject == _source) return;

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
