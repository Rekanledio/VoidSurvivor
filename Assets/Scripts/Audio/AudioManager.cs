using UnityEngine;
using VoidSurvivor.Save;

namespace VoidSurvivor.Audio
{
    /// <summary>
    /// Minimal SFX foundation (M12.1): a single persistent AudioManager with one
    /// reusable AudioSource. Owns no event subscriptions — gameplay systems call
    /// <see cref="PlaySfx"/> directly (M12.2 wires events to it). Volumes are
    /// runtime-only (default 1.0); no settings UI, no PlayerPrefs, no mixer.
    /// Created by GameBootstrap on the persistent GameManager object
    /// (DontDestroyOnLoad), so exactly one instance exists for the app lifetime.
    ///
    /// M13.2: Master/Sfx volume persist across launches via SaveManager
    /// (JsonUtility + persistentDataPath). Volume is clamped 0..1 on every set;
    /// missing/corrupt save falls back to 1.0/1.0 defaults. AudioManager never
    /// touches JSON/File directly — it delegates to SaveManager.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public float MasterVolume { get; private set; } = 1f;
        public float SfxVolume { get; private set; } = 1f;

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

            // M13.2: apply persisted volumes before any SFX can play.
            // SaveManager is added to the same GO BEFORE AudioManager in
            // GameBootstrap, so its Awake (Instance) already ran.
            LoadSettings();
        }

        /// <summary>M13.2: sets master volume (clamped 0..1) and persists it.</summary>
        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            SaveSettings();
        }

        /// <summary>M13.2: sets SFX volume (clamped 0..1) and persists it.</summary>
        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            SaveSettings();
        }

        /// <summary>
        /// Loads persisted volumes from SaveManager. Missing/corrupt save
        /// yields default SaveData (1.0/1.0) via SaveManager.Load — no crash.
        /// </summary>
        private void LoadSettings()
        {
            if (SaveManager.Instance == null) return;

            var data = SaveManager.Instance.Load();
            MasterVolume = Mathf.Clamp01(data.masterVolume);
            SfxVolume = Mathf.Clamp01(data.sfxVolume);
        }

        /// <summary>
        /// Persists current volumes. Read-modify-write on the existing SaveData
        /// so unrelated fields (bestWave/bestLevel/bestGold, M13.3) are preserved.
        /// </summary>
        private void SaveSettings()
        {
            if (SaveManager.Instance == null) return;

            var data = SaveManager.Instance.Load();
            data.masterVolume = MasterVolume;
            data.sfxVolume = SfxVolume;
            SaveManager.Instance.Save(data);
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
