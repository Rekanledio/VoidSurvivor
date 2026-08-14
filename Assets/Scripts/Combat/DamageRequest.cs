using UnityEngine;

namespace VoidSurvivor.Combat
{
    /// <summary>
    /// A damage request entering the combat pipeline (M5.1). Source may be null
    /// when the attacker is unknown (e.g. a projectile without an owner);
    /// Target is the object that must expose <see cref="IDamageable"/>.
    /// </summary>
    public readonly struct DamageRequest
    {
        public GameObject Source { get; }
        public GameObject Target { get; }
        public float Damage { get; }

        public DamageRequest(GameObject source, GameObject target, float damage)
        {
            Source = source;
            Target = target;
            Damage = damage;
        }
    }
}
