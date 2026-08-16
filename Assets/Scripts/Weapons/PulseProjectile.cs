using UnityEngine;
using VoidSurvivor.Combat;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Pulse Gun projectile (M6.2): single-target, straight flight toward the
    /// direction captured at spawn, fixed speed, lifetime expiry; on contact
    /// routes a DamageRequest through <see cref="CombatSystem"/> with the player
    /// as Source. Skips its own Source object. No pierce/explode/split.
    /// </summary>
    [DisallowMultipleComponent]
    public class PulseProjectile : MonoBehaviour
    {
        private const float Lifetime = 3f;

        private Rigidbody2D _body;
        private GameObject _source;
        private Vector2 _direction;
        private float _damage;
        private bool _initialized;
        private float _lifetimeLeft = Lifetime;

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

        private void FixedUpdate()
        {
            if (!_initialized) return;

            _lifetimeLeft -= Time.fixedDeltaTime;
            if (_lifetimeLeft <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Never hit the shooter itself.
            if (other.gameObject == _source) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                CombatSystem.ApplyDamage(new DamageRequest(_source, other.gameObject, _damage));
                Destroy(gameObject);
            }
        }
    }
}
