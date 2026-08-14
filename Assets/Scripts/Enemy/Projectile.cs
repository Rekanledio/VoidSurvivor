using UnityEngine;
using VoidSurvivor.Player;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Minimal M4.4 projectile used only to prove the Shooter's ranged attack.
    /// Flies in a fixed direction at a fixed speed, expires after a lifetime,
    /// and applies its damage to the player on contact. This is NOT the M5
    /// Combat/Projectile system — M5 will replace it with the unified pipeline.
    /// </summary>
    [DisallowMultipleComponent]
    public class Projectile : MonoBehaviour
    {
        private const float Speed = 8f;
        private const float Lifetime = 3f;

        private Rigidbody2D _body;
        private float _damage;
        private bool _initialized;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Sets flight direction and damage; called once right after spawn.</summary>
        public void Init(Vector2 direction, float damage)
        {
            _damage = damage;
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
            // Contact damage against the player. Minimal M4.4 path — M5 Combat
            // will own damage rules; GetComponent here is event-driven, not a hot path.
            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }

        private float _lifetimeLeft = Lifetime;
    }
}
