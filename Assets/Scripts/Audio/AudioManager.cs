using UnityEngine;

namespace VoidSurvivor.Audio
{
    /// <summary>
    /// Minimal SFX foundation (M12.1): a single persistent AudioManager with one
    /// reusable AudioSource. Owns no event subscriptions — gameplay systems call
    /// <see cref="PlaySfx"/> directly (M12.2 wires events to it). Volumes are
    /// runtime-only (default 1.0); no settings UI, no PlayerPrefs, no mixer.
    /// Created by GameBootstrap on the persistent GameManager object
    /// (DontDestroyOnLoad), so exactly one instance exists for the app lifetime.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public float MasterVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;

        private AudioSource _sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = GetComponent<AudioSource>();
            if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
        }

        /// <summary>Plays a SFX clip at the default volume.</summary>
        public void PlaySfx(AudioClip clip)
        {
            PlaySfx(clip, 1f);
        }

        /// <summary>
        /// Plays a SFX clip scaled by the given volume AND Master/Sfx volumes
        /// (clamped 0..1). Uses PlayOneShot so overlapping SFX can sound
        /// simultaneously. Null clip / missing source are no-ops (no errors).
        /// </summary>
        public void PlaySfx(AudioClip clip, float volume)
        {
            if (clip == null || _sfxSource == null) return;

            float v = Mathf.Clamp01(volume * MasterVolume * SfxVolume);
            _sfxSource.PlayOneShot(clip, v);
        }
    }
}
