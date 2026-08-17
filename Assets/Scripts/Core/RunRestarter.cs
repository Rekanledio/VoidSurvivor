using UnityEngine;
using VoidSurvivor.Enemy;
using VoidSurvivor.Player;
using VoidSurvivor.Weapons;

namespace VoidSurvivor.Core
{
    /// <summary>
    /// Minimal run-restart entry (M11.3). Resets every per-run runtime system
    /// and enters a fresh Playing state. Owns no UI, no state machine — panels
    /// (and only panels) call into it; the reset logic lives here (Core), NOT
    /// inside the UI panels, per the M11.3 rule.
    ///
    /// M14 P9/P10: every "start a new run" entry funnels through
    /// <see cref="ResetForNewRun"/> — used by <see cref="RestartRun"/>
    /// (GameOver/Victory panels) and <see cref="StartNewRun"/> (MainMenu Play).
    /// Both paths end in the same clean run state: player full HP / alive,
    /// stats / progress / weapons reset, position at the arena center, wave 1,
    /// and no active enemy left over from a previous run (P9).
    /// </summary>
    public static class RunRestarter
    {
        /// <summary>Restart entry used by the GameOver / Victory result panels.</summary>
        public static void RestartRun()
        {
            ResetForNewRun();
        }

        /// <summary>
        /// M14 P10 fix: unified "start a new run" entry used by the MainMenu
        /// Play button. Identical to <see cref="RestartRun"/> — works from
        /// MainMenu (first run, or after GameOver → MainMenu) and from
        /// GameOver / Victory, so MainMenu → Playing always yields a clean
        /// run state (never a zombie player with HP=0 / IsDead=true).
        /// </summary>
        public static void StartNewRun()
        {
            ResetForNewRun();
        }

        private static void ResetForNewRun()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // P9: no enemy from a previous run may survive into the fresh run.
            // GameOver only stops the wave (WaveManager), it does not clear
            // enemies — release them all back to their pools here so the next
            // run starts with a clean field (covers Restart AND MainMenu→Play).
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            if (spawner != null) spawner.ReleaseAllActiveEnemies();

            // Player health: alive again with full HP (a fresh run never starts dead).
            var health = Object.FindFirstObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.ResetForRun();
                // M14 regression fix: a fresh run starts at the arena center, so
                // a player who died near the bounds never resumes outside them.
                health.transform.position = Vector3.zero;
            }

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
