using UnityEngine;
using VoidSurvivor.Enemy;
using VoidSurvivor.Player;
using VoidSurvivor.Weapons;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Minimal run-restart entry (M11.3). Resets every per-run runtime system
    /// and enters a fresh Playing state. Owns no UI, no state machine — panels
    /// (and only panels) call <see cref="RestartRun"/>; the reset logic lives
    /// here (Core), NOT inside the result panels, per the M11.3 rule.
    ///
    /// WaveManager already resets itself on GameOver/Victory (next Playing
    /// starts a fresh run at wave 1), so we only reset player/weapon state and
    /// then walk GameOver/Victory -> MainMenu -> Playing through the single
    /// GameManager state entry.
    /// </summary>
    public static class RunRestarter
    {
        public static void RestartRun()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Player progression: XP = 0, Level = 1, Gold = 0.
            var progress = Object.FindFirstObjectByType<PlayerProgress>();
            if (progress != null) progress.ResetForRun();

            // Player stats: zero every runtime upgrade bonus.
            var stats = Object.FindFirstObjectByType<PlayerStats>();
            if (stats != null) stats.ResetForRun();

            // Weapon runtime upgrades: WeaponLevel = 1, all bonuses = 0.
            var wm = Object.FindFirstObjectByType<WeaponManager>();
            if (wm != null)
            {
                for (int i = 0; i < wm.SlotCount; i++)
                {
                    var weapon = wm.GetWeapon(i);
                    if (weapon != null) weapon.ResetWeaponUpgrades();
                }
            }

            // Fresh run: GameOver/Victory -> MainMenu -> Playing.
            // (Playing entry also makes WaveManager start wave 1 automatically.)
            if (gm.CurrentState != GameState.MainMenu)
                gm.TryChangeState(GameState.MainMenu);
            gm.TryChangeState(GameState.Playing);
        }
    }
}
