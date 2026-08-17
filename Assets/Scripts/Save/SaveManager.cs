using System.IO;
using UnityEngine;

namespace VoidSurvivor.Save
{
    /// <summary>
    /// Local persistence service (M13.1). Pure object <-> JSON <-> file.
    ///
    /// Responsibilities:
    ///   - Save(SaveData)   : serialize to JSON and write to disk
    ///   - Load()           : read JSON from disk and deserialize
    ///   - HasSave()        : whether the save file exists
    ///   - DeleteSave()     : remove the save file (test/debug capability)
    ///
    /// Explicitly NOT responsible for gameplay: PlayerProgress / PlayerStats /
    /// WeaponController / WaveManager / UI are outside its scope. Run data
    /// (XP, gold, level, stat bonuses, weapon upgrades, current wave) is NOT
    /// persisted; only settings (M13.2) and best run record (M13.3) will use
    /// this service later.
    ///
    /// Lives on the persistent GameManager GameObject (GameBootstrap),
    /// DontDestroyOnLoad, exactly one instance.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        /// <summary>Full path of the save file on this machine.</summary>
        public static string SavePath =>
            Path.Combine(Application.persistentDataPath, "VoidSurvivorSave.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // The parent GameObject (GameManager) is already DontDestroyOnLoad;
            // keep the singleton reference stable across scenes.
        }

        /// <summary>Serializes the data object to the save file.</summary>
        public void Save(SaveData data)
        {
            if (data == null) return;

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Deserializes the save file. Missing file -> default SaveData.
        /// Corrupt/unparsable JSON -> default SaveData (never crashes, never
        /// propagates bad data into gameplay).
        /// </summary>
        public SaveData Load()
        {
            if (!File.Exists(SavePath))
                return new SaveData();

            try
            {
                string json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Load failed (falling back to defaults): {ex.Message}");
                return new SaveData();
            }
        }

        /// <summary>Whether a save file exists on disk.</summary>
        public bool HasSave() => File.Exists(SavePath);

        /// <summary>Deletes the save file. No-op when it does not exist.</summary>
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                    File.Delete(SavePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SaveManager] DeleteSave failed: {ex.Message}");
            }
        }
    }
}
