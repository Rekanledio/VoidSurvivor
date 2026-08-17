using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;

namespace VoidSurvivor.Audio
{
    /// <summary>
    /// Core gameplay SFX wiring (M12.2): subscribes to gameplay events and
    /// forwards them to the single AudioManager entry via SfxLibrary clips.
    /// Event -> SFX handler -> AudioManager.PlaySfx. No gameplay system ever
    /// finds an AudioSource itself.
    ///
    /// High-frequency guard: DamageApplied is rate-limited by a small cooldown
    /// (0.05s) so bursts of hits do not pile up into noise.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameplaySfx : MonoBehaviour
    {
        private const float HitCooldown = 0.05f;

        private float _lastHitTime = -1f;

        /// <summary>Total SFX plays issued (debug/verification helper).</summary>
        public int PlayCount { get; private set; }

        private void Awake()
        {
            EventBus.Subscribe<DamageApplied>(OnDamageApplied);
            EventBus.Subscribe<EnemyDied>(OnEnemyDied);
            EventBus.Subscribe<PickupCollected>(OnPickupCollected);
            EventBus.Subscribe<PlayerLevelUp>(OnPlayerLevelUp);
            EventBus.Subscribe<BossSpawned>(OnBossSpawned);
            EventBus.Subscribe<BossDefeated>(OnBossDefeated);
            EventBus.Subscribe<PlayerDied>(OnPlayerDied);
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DamageApplied>(OnDamageApplied);
            EventBus.Unsubscribe<EnemyDied>(OnEnemyDied);
            EventBus.Unsubscribe<PickupCollected>(OnPickupCollected);
            EventBus.Unsubscribe<PlayerLevelUp>(OnPlayerLevelUp);
            EventBus.Unsubscribe<BossSpawned>(OnBossSpawned);
            EventBus.Unsubscribe<BossDefeated>(OnBossDefeated);
            EventBus.Unsubscribe<PlayerDied>(OnPlayerDied);
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void Play(SfxType type, float volume = 1f)
        {
            var lib = SfxLibrary.Instance;
            var audio = AudioManager.Instance;
            if (lib == null || audio == null) return;

            var clip = lib.Clip(type);
            if (clip == null) return;

            audio.PlaySfx(clip, volume);
            PlayCount++;
        }

        private void OnDamageApplied(DamageApplied e)
        {
            // Rate-limit high-frequency hits (same-frame bursts -> 1 sound).
            if (Time.time - _lastHitTime < HitCooldown) return;
            _lastHitTime = Time.time;

            Play(SfxType.Hit, 0.6f);
        }

        private void OnEnemyDied(EnemyDied e) => Play(SfxType.EnemyDeath);

        private void OnPickupCollected(PickupCollected e) => Play(SfxType.Pickup, 0.7f);

        private void OnPlayerLevelUp(PlayerLevelUp e) => Play(SfxType.LevelUp);

        private void OnBossSpawned(BossSpawned e) => Play(SfxType.BossSpawn);

        private void OnBossDefeated(BossDefeated e) => Play(SfxType.BossDefeat);

        private void OnPlayerDied(PlayerDied e) => Play(SfxType.PlayerDeath);

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (e.To == GameState.Victory) Play(SfxType.Victory);
        }
    }
}
