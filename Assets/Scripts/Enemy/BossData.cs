using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Boss static configuration (M8.3): a minimal <see cref="EnemyData"/>
    /// subclass — all stats live in the BossData asset (large MaxHP, higher
    /// Damage, low MoveSpeed). No extra fields; the M8.2 WaveMultiplier scales
    /// it automatically like any other enemy.
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "VoidSurvivor/Boss Data")]
    public class BossData : EnemyData
    {
    }
}
