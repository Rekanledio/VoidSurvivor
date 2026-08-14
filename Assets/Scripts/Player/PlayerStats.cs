using UnityEngine;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Base player stats as defined in GAME_DESIGN.md (section 6).
    /// M3 scope: plain base values + read accessors only.
    /// Stat modifiers are introduced by the Roguelite milestone (M9).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats (GAME_DESIGN.md)")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float hpRegen = 0f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float critChance = 0.05f;
        [SerializeField] private float critDamage = 1.5f;
        [SerializeField] private float range = 1f;
        [SerializeField] private float pickupRange = 2f;
        [SerializeField] private float armor = 0f;

        public float MaxHP => maxHP;
        public float HPRegen => hpRegen;
        public float MoveSpeed => moveSpeed;
        public float Damage => damage;
        public float AttackSpeed => attackSpeed;
        public float CritChance => critChance;
        public float CritDamage => critDamage;
        public float Range => range;
        public float PickupRange => pickupRange;
        public float Armor => armor;
    }
}
