namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// A single weapon slot (M6.1): a plain runtime container holding one
    /// <see cref="WeaponController"/>. Equip replaces whatever is in the slot
    /// (explicit Equip semantics); Unequip empties it. No upgrades/rarity/UI.
    /// </summary>
    public class WeaponSlot
    {
        private WeaponController _weapon;

        public bool IsEmpty => _weapon == null;
        public WeaponController Weapon => _weapon;

        public void Equip(WeaponController weapon)
        {
            _weapon = weapon;
        }

        public void Unequip()
        {
            _weapon = null;
        }
    }
}
