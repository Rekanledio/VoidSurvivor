using System.Collections.Generic;
using UnityEngine;
using VoidSurvivor.Combat;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Boomerang projectile (M6.4): flies outward toward the initial direction
    /// until it reaches the configured max distance, then returns toward the
    /// player's CURRENT world position (recomputed every physics frame) and is
    /// destroyed when close enough. Each boomerang hits a given enemy at most
    /// once (per-target HashSet) while it may hit different enemies on the way
    /// out and back. Damage goes through CombatSystem with the player as Source.
    /// </summary>
    [DisallowMultipleComponent]
    public class BoomerangProjectile : MonoBehaviour
    {
        private const float ReturnPickupDistance = 0.5f;
        private const float Lifetime = 15f;

        private enum Phase
        {
            Outbound,
            Return,
        }

        private Rigidbody2D _body;
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

        /// <summary>Live count of active boomerangs (used by the weapon for the single-flight rule).</summary>
        public static int ActiveCount;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            ActiveCount++;
        }

        private void OnDestroy()
        {
            ActiveCount--;
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
                    Destroy(gameObject);
                    return;
                }

                Vector2 toPlayer = (Vector2)_returnTarget.position - _body.position;
                float distance = toPlayer.magnitude;
                if (distance <= ReturnPickupDistance)
                {
                    Destroy(gameObject);
                    return;
                }

                Vector2 next = _body.position + toPlayer.normalized * (_returnSpeed * Time.fixedDeltaTime);
                _body.MovePosition(next);
            }

            _lifetimeLeft -= Time.fixedDeltaTime;
            if (_lifetimeLeft <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized) return;
            if (other.gameObject == _source) return;
            if (_hitTargets.Contains(other.gameObject)) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                _hitTargets.Add(other.gameObject); // once per enemy per throw
                CombatSystem.ApplyDamage(new DamageRequest(_source, other.gameObject, _damage));
            }
        }
    }
}
