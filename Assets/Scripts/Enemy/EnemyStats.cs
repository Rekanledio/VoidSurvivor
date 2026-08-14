using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Read-only runtime view over an <see cref="EnemyData"/> configuration asset.
    /// Keeps runtime state (HP, death) in EnemyHealth instead, so configuration
    /// assets are never mutated at runtime. Mirrors the PlayerStats pattern.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyStats : MonoBehaviour
    {
        [SerializeField, Tooltip("Enemy type configuration. Assigned on the prefab.")]
        private EnemyData data;

        public float MaxHP => data != null ? data.MaxHP : 0f;
        public float Damage => data != null ? data.Damage : 0f;
        public float AttackRange => data != null ? data.AttackRange : 0f;
        public float AttackCooldown => data != null ? data.AttackCooldown : 0f;
        public float MoveSpeed => data != null ? data.MoveSpeed : 0f;
    }
}
