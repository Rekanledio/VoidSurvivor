using UnityEngine;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Boomerang static config (M6.4): extends <see cref="WeaponData"/> with the
    /// out-and-return flight parameters (max outward distance, outward speed,
    /// return speed). Base WeaponData untouched; read-only at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "BoomerangData", menuName = "VoidSurvivor/Boomerang Data")]
    public class BoomerangData : WeaponData
    {
        [SerializeField] private float maxDistance = 6f;
        [SerializeField] private float outSpeed = 8f;
        [SerializeField] private float returnSpeed = 10f;

        public float MaxDistance => maxDistance;
        public float OutSpeed => outSpeed;
        public float ReturnSpeed => returnSpeed;
    }
}
