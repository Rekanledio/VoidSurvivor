using UnityEngine;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Arc Blade static config (M6.5): close-range area weapon. Reuses the base
    /// <see cref="WeaponData"/> fields only — Range doubles as the attack
    /// radius, damage/cooldown as usual. No extra fields needed.
    /// </summary>
    [CreateAssetMenu(fileName = "ArcBladeData", menuName = "VoidSurvivor/Arc Blade Data")]
    public class ArcBladeData : WeaponData
    {
    }
}
