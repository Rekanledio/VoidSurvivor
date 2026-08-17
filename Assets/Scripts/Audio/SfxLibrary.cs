using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivor.Audio
{
    /// <summary>
    /// Core gameplay SFX types (M12.2). One clip per entry, loaded from
    /// Resources/Audio/SFX (see <see cref="SfxLibrary"/>).
    /// </summary>
    public enum SfxType
    {
        Hit,
        EnemyDeath,
        Pickup,
        LevelUp,
        BossSpawn,
        BossDefeat,
        PlayerDeath,
        Victory,
        UiClick
    }

    /// <summary>
    /// Minimal SFX library (M12.2): maps <see cref="SfxType"/> to AudioClips
    /// loaded once from Resources ("Audio/SFX/sfx_<name>"). Lives on the
    /// persistent GameManager object. AudioManager stays the single playback
    /// entry — this library only resolves clips. No data-driven framework.
    /// </summary>
    [DisallowMultipleComponent]
    public class SfxLibrary : MonoBehaviour
    {
        public static SfxLibrary Instance { get; private set; }

        private readonly Dictionary<SfxType, AudioClip> _clips = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            foreach (SfxType type in System.Enum.GetValues(typeof(SfxType)))
            {
                string resName = ResourceName(type);
                if (resName == null) continue;
                var clip = Resources.Load<AudioClip>(resName);
                if (clip == null)
                    Debug.LogWarning($"[SfxLibrary] Missing SFX resource: {resName}");
                _clips[type] = clip;
            }
        }

        /// <summary>Explicit Resources path per type (snake_case file names).</summary>
        private static string ResourceName(SfxType type)
        {
            switch (type)
            {
                case SfxType.Hit: return "Audio/SFX/sfx_hit";
                case SfxType.EnemyDeath: return "Audio/SFX/sfx_enemy_death";
                case SfxType.Pickup: return "Audio/SFX/sfx_pickup";
                case SfxType.LevelUp: return "Audio/SFX/sfx_level_up";
                case SfxType.BossSpawn: return "Audio/SFX/sfx_boss_spawn";
                case SfxType.BossDefeat: return "Audio/SFX/sfx_boss_defeat";
                case SfxType.PlayerDeath: return "Audio/SFX/sfx_player_death";
                case SfxType.Victory: return "Audio/SFX/sfx_victory";
                case SfxType.UiClick: return "Audio/SFX/sfx_ui_click";
                default: return null;
            }
        }

        /// <summary>Returns the clip for a type, or null when missing.</summary>
        public AudioClip Clip(SfxType type)
        {
            return _clips.TryGetValue(type, out var clip) ? clip : null;
        }

        /// <summary>
        /// Shared UI click sound (M12.2): one short click for every UI button.
        /// Safe to call from any UI panel; no-op when the library is absent.
        /// </summary>
        public static void PlayUiClick()
        {
            if (Instance == null || AudioManager.Instance == null) return;
            var clip = Instance.Clip(SfxType.UiClick);
            if (clip != null) AudioManager.Instance.PlaySfx(clip, 0.7f);
        }

        /// <summary>True when every expected SFX resource loaded.</summary>
        public bool AllClipsLoaded()
        {
            foreach (SfxType type in System.Enum.GetValues(typeof(SfxType)))
            {
                if (!_clips.TryGetValue(type, out var clip) || clip == null) return false;
            }
            return true;
        }
    }
}
