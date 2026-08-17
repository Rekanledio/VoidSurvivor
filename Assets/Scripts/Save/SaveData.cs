using System;

namespace VoidSurvivor.Save
{
    /// <summary>
    /// Serializable local-persistence model (M13.1).
    ///
    /// Fields are reserved for later milestones:
    ///   - masterVolume / sfxVolume : M13.2 Settings Persistence
    ///   - bestWave / bestLevel / bestGold : M13.3 Best Run Record
    ///
    /// M13.1 deliberately keeps the model minimal and stores NO run data
    /// (XP / gold / level / stat bonuses / weapon upgrades / wave are all
    /// run-scoped and intentionally NOT persisted).
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public int bestWave;
        public int bestLevel;
        public int bestGold;
    }
}
