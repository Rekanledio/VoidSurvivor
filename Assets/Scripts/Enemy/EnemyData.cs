using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Static configuration for an enemy type (M4.1). Consumed read-only by
    /// <see cref="EnemyStats"/>; runtime state lives in EnemyHealth / AI scripts.
    /// One asset per enemy type (Chaser / Runner / Shooter / Tank / Boss later).
    /// No combat behavior here — this is pure data.
    /// </summary>
    [CreateAssetMenu(menuName = "VoidSurvivor/Enemy Data", fileName = "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Combat")]
        [SerializeField, Tooltip("Starting/max hit points.")]
        private float maxHP = 30f;
        [SerializeField, Tooltip("Damage dealt to the player on contact/attack.")]
        private float damage = 10f;
        [SerializeField, Tooltip("Distance within which the enemy can engage.")]
        private float attackRange = 1.5f;
        [SerializeField, Tooltip("Seconds between attacks.")]
        private float attackCooldown = 1f;

        [Header("Movement")]
        [SerializeField, Tooltip("Movement speed in world units per second.")]
        private float moveSpeed = 3f;

        public float MaxHP => maxHP;
        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float MoveSpeed => moveSpeed;
    }
}
