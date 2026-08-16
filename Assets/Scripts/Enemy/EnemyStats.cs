using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Read-only runtime view over an <see cref="EnemyData"/> configuration asset,
    /// plus a runtime wave multiplier (M8.2). Keeps runtime state (HP, death) in
    /// EnemyHealth instead, so configuration assets are never mutated at runtime.
    /// Mirrors the PlayerStats pattern.
    ///
    /// M8.2: <see cref="WaveMultiplier"/> is a plain non-serialized runtime field
    /// injected on every spawn (EnemyController.Spawn). It scales MaxHP / Damage /
    /// MoveSpeed; AttackRange and AttackCooldown stay at the EnemyData values.
    /// Since every AI reads these properties live every frame, one change here
    /// scales all four enemy types with no per-AI logic.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyStats : MonoBehaviour
    {
        [SerializeField, Tooltip("Enemy type configuration. Assigned on the prefab.")]
        private EnemyData data;

        /// <summary>Runtime wave difficulty multiplier (M8.2); injected per spawn, default 1.</summary>
        [System.NonSerialized] public float WaveMultiplier = 1f;

        public float MaxHP => data != null ? data.MaxHP * WaveMultiplier : 0f;
        public float Damage => data != null ? data.Damage * WaveMultiplier : 0f;
        public float MoveSpeed => data != null ? data.MoveSpeed * WaveMultiplier : 0f;
        public float AttackRange => data != null ? data.AttackRange : 0f;
        public float AttackCooldown => data != null ? data.AttackCooldown : 0f;
    }
}
