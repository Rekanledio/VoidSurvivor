using System.Collections.Generic;
using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Upgrade chooser logic (M9.2): listens for <see cref="PlayerLevelUp"/>,
    /// maintains a pending level-up queue (one AddXP can cross multiple levels),
    /// generates 3 unique random options from the configured pool, applies the
    /// chosen one to <see cref="PlayerStats"/>, publishes <see cref="UpgradeSelected"/>
    /// and returns to Playing once the queue drains. Pure logic — no UI; the
    /// future LevelUp panel drives GenerateOptions()/Select() through this API.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStats))]
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Pool of all available upgrades (10 in M9.2).")]
        private List<UpgradeData> upgradePool = new();

        private PlayerStats _stats;
        private PlayerProgress _progress;
        private readonly List<UpgradeData> _options = new();
        private int _pendingLevelUps;
        private bool _waitingForSelection;

        public int PendingLevelUps => _pendingLevelUps;
        public bool IsWaitingForSelection => _waitingForSelection;
        public IReadOnlyList<UpgradeData> Options => _options;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _progress = GetComponent<PlayerProgress>();
            EventBus.Subscribe<PlayerLevelUp>(OnPlayerLevelUp);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerLevelUp>(OnPlayerLevelUp);
        }

        private void OnPlayerLevelUp(PlayerLevelUp e)
        {
            _pendingLevelUps++;

            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.Playing) return;

            if (gm.TryChangeState(GameState.LevelUp))
            {
                GenerateOptions();
            }
        }

        /// <summary>
        /// Draws up to 3 UNIQUE options from the pool (no weights/rarity).
        /// Different level-ups may draw the same upgrade again. Publishes
        /// <see cref="UpgradeOptionsGenerated"/> AFTER Options is fully updated
        /// (M9.3) so UI listeners always read the complete candidate set.
        /// </summary>
        public void GenerateOptions()
        {
            _options.Clear();
            if (upgradePool == null || upgradePool.Count == 0)
            {
                _waitingForSelection = false;
                PublishOptionsGenerated();
                return;
            }

            var candidates = new List<UpgradeData>(upgradePool);
            while (_options.Count < 3 && candidates.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, candidates.Count);
                _options.Add(candidates[idx]);
                candidates.RemoveAt(idx);
            }

            _waitingForSelection = _options.Count > 0;
            PublishOptionsGenerated();
        }

        /// <summary>Test hook: forces the next set of options (ignores the pool).</summary>
        public void SetForcedOptions(params UpgradeData[] forced)
        {
            _options.Clear();
            if (forced != null) _options.AddRange(forced);
            _waitingForSelection = _options.Count > 0;
            PublishOptionsGenerated();
        }

        private void PublishOptionsGenerated()
        {
            EventBus.Publish(new UpgradeOptionsGenerated(
                _options.Count > 0 ? _options[0] : null,
                _options.Count > 1 ? _options[1] : null,
                _options.Count > 2 ? _options[2] : null));
        }

        /// <summary>
        /// Applies Options[index] exactly once, publishes <see cref="UpgradeSelected"/>,
        /// then either keeps LevelUp for the next pending level or returns to Playing.
        /// Guards: must be waiting, in LevelUp state, index in range, option valid.
        /// </summary>
        public void Select(int index)
        {
            if (!_waitingForSelection) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.LevelUp) return;

            if (index < 0 || index >= _options.Count || _options[index] == null) return;

            var chosen = _options[index];
            _options.Clear();
            _waitingForSelection = false;

            _stats.ApplyUpgrade(chosen);
            _pendingLevelUps--;
            EventBus.Publish(new UpgradeSelected(chosen, _progress != null ? _progress.Level : 0));

            if (_pendingLevelUps > 0)
            {
                GenerateOptions(); // stay in LevelUp; next set ready
            }
            else
            {
                gm.TryChangeState(GameState.Playing);
            }
        }
    }
}
