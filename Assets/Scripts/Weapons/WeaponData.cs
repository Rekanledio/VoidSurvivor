using UnityEngine;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Static weapon configuration (M6.1): identity and base combat values.
    /// Assets are never mutated at runtime — WeaponController reads them only.
    /// The four formal weapons (Pulse Gun / Scatter Blaster / Boomerang /
    /// Arc Blade) land in M6.2+; this asset is their shared configuration type.
    /// No crit/element/status/upgrade/rarity fields yet.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "VoidSurvivor/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string weaponName = "Unnamed Weapon";
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float range = 5f;

        public string WeaponName => weaponName;
        public float BaseDamage => baseDamage;
        public float AttackCooldown => attackCooldown;
        public float Range => range;
    }
}
