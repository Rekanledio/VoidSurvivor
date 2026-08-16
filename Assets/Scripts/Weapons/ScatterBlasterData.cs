using UnityEngine;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Scatter Blaster static config (M6.3): extends <see cref="WeaponData"/>
    /// with the scatter-specific fields (projectile count and total spread
    /// angle). Base WeaponData is untouched; read-only at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "ScatterBlasterData", menuName = "VoidSurvivor/Scatter Blaster Data")]
    public class ScatterBlasterData : WeaponData
    {
        [SerializeField] private int projectileCount = 5;
        [SerializeField] private float spreadAngle = 45f;

        public int ProjectileCount => projectileCount;
        public float SpreadAngle => spreadAngle;
    }
}
