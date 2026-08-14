using UnityEngine;
using VoidSurvivor.Combat;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Projectile (M5.1): flies in a fixed direction at a fixed speed, expires
    /// after a lifetime, and on contact routes a damage request through
    /// <see cref="CombatSystem"/> to whatever <see cref="IDamageable"/> the hit
    /// object exposes. No direct PlayerHealth coupling and no damage logic of
    /// its own. The unified combat pipeline owns damage application.
    /// </summary>
    [DisallowMultipleComponent]
    public class Projectile : MonoBehaviour
    {
        private const float Speed = 8f;
        private const float Lifetime = 3f;

        private Rigidbody2D _body;
        private float _damage;
        private GameObject _source;
        private bool _initialized;

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

        private void FixedUpdate()
        {
            if (!_initialized) return;

            // Lifetime expiry (moves via Rigidbody2D.velocity automatically).
            _lifetimeLeft -= Time.fixedDeltaTime;
            if (_lifetimeLeft <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Route damage through the unified combat entry (event-driven, not a hot path).
            if (other.TryGetComponent(out IDamageable damageable))
            {
                CombatSystem.ApplyDamage(new DamageRequest(_source, other.gameObject, _damage));
                Destroy(gameObject);
            }
        }

        private float _lifetimeLeft = Lifetime;
    }
}
