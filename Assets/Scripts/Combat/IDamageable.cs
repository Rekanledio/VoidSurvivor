namespace VoidSurvivor.Combat
{
    /// <summary>
    /// Minimal damage target abstraction (M5.1): anything that can receive damage.
    /// Implemented by PlayerHealth and EnemyHealth. Keeps Combat decoupled from
    /// concrete health types — no Player/Enemy branches inside the damage pipeline.
    /// </summary>
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(float damage);
    }
}
