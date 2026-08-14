using UnityEngine;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Player runtime progress resources (M5.3): XP and Gold accumulation only.
    /// No levels, thresholds, upgrades or shop logic — those belong to the
    /// Roguelite/Shop systems. Separate from PlayerStats (character attributes)
    /// by design: this is runtime progression state, not a character stat.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerProgress : MonoBehaviour
    {
        private int _currentXP;
        private int _currentGold;

        public int CurrentXP => _currentXP;
        public int CurrentGold => _currentGold;

        /// <summary>Adds XP; negative or zero amounts are ignored.</summary>
        public void AddXP(int amount)
        {
            if (amount > 0) _currentXP += amount;
        }

        /// <summary>Adds Gold; negative or zero amounts are ignored.</summary>
        public void AddGold(int amount)
        {
            if (amount > 0) _currentGold += amount;
        }
    }
}
