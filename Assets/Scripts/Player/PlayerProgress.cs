using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Player runtime progress resources (M5.3, leveled since M9.1): XP and Gold
    /// accumulation plus a minimal level state. Separate from PlayerStats
    /// (character attributes) by design: this is runtime progression state.
    ///
    /// M9.1: AddXP accumulates XP, and when the current threshold is reached the
    /// player levels up — XP carry-over is kept and one AddXP can cross multiple
    /// levels, publishing <see cref="PlayerLevelUp"/> once per level.
    ///
    /// NOTE: the XP requirement below is an M9.1 PLACEHOLDER parameter, NOT a
    /// GAME_DESIGN value (the design does not specify level/XP formulas).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerProgress : MonoBehaviour
    {
        private int _currentXP;
        private int _currentGold;
        private int _level = 1;

        public int CurrentXP => _currentXP;
        public int CurrentGold => _currentGold;
        public int Level => _level;

        /// <summary>M9.1 placeholder threshold: 100 × level (not final game design).</summary>
        public int XPToNextLevel => 100 * _level;

        /// <summary>Adds XP; negative or zero amounts are ignored. Levels up with carry-over.</summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            _currentXP += amount;
            while (_currentXP >= XPToNextLevel) // threshold grows with level; loop always terminates
            {
                _currentXP -= XPToNextLevel;
                _level++;
                EventBus.Publish(new PlayerLevelUp(_level));
            }
        }

        /// <summary>Adds Gold; negative or zero amounts are ignored.</summary>
        public void AddGold(int amount)
        {
            if (amount > 0) _currentGold += amount;
        }
    }
}
