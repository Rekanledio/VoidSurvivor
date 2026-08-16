using UnityEngine;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Player-side weapon container (M6.1): manages exactly 4 weapon slots.
    /// Equip/Unequip/GetSlot/GetWeapon with bounds checks; no weapon AI, no
    /// auto-attack, no shop/upgrade logic. Distinct from PlayerController
    /// (movement) and PlayerAttack (single attack entry) by responsibility.
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponManager : MonoBehaviour
    {
        private const int MaxSlots = 4;

        private readonly WeaponSlot[] _slots = new WeaponSlot[MaxSlots];

        public int SlotCount => _slots.Length;

        private void Awake()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new WeaponSlot();
            }
        }

        /// <summary>Equips a weapon into the given slot; false if out of range.</summary>
        public bool Equip(int slotIndex, WeaponController weapon)
        {
            if (!IsValidIndex(slotIndex)) return false;

            _slots[slotIndex].Equip(weapon);
            return true;
        }

        /// <summary>Unequips the given slot; false if out of range.</summary>
        public bool Unequip(int slotIndex)
        {
            if (!IsValidIndex(slotIndex)) return false;

            _slots[slotIndex].Unequip();
            return true;
        }

        /// <summary>Returns the slot at the index, or null if out of range.</summary>
        public WeaponSlot GetSlot(int slotIndex)
        {
            return IsValidIndex(slotIndex) ? _slots[slotIndex] : null;
        }

        /// <summary>Returns the weapon in the slot, or null if empty/out of range.</summary>
        public WeaponController GetWeapon(int slotIndex)
        {
            var slot = GetSlot(slotIndex);
            return slot != null ? slot.Weapon : null;
        }

        private static bool IsValidIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < MaxSlots;
        }
    }
}
