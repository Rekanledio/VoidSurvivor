namespace VoidSurvivor.Combat
{
    /// <summary>
    /// Minimal outcome of a damage request (M5.1): whether it was applied, the
    /// nominal damage value, and whether the target died as a result. No combat
    /// statistics — extended only by the systems that actually need more.
    /// </summary>
    public readonly struct DamageResult
    {
        public bool Applied { get; }
        public float Damage { get; }
        public bool TargetDead { get; }

        public DamageResult(bool applied, float damage, bool targetDead)
        {
            Applied = applied;
            Damage = damage;
            TargetDead = targetDead;
        }
    }
}
