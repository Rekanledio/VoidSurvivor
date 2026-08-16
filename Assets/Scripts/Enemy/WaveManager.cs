using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Wave lifecycle + spawn scheduling (M8.1). Drives waves 1..10: on Playing
    /// it advances wave time and spawns enemies through <see cref="EnemySpawner"/>
    /// (which owns the M7 pools — this class never touches ObjectPool directly).
    /// Wave completion is decided by the configured duration + spawn schedule
    /// (NOT by how many enemies are still alive). Publishes
    /// <see cref="WaveStarted"/> / <see cref="WaveCompleted"/>.
    ///
    /// GameState linkage: time only advances while Playing; Paused/LevelUp/Shop
    /// freeze it; GameOver/Victory stop it; re-entering Playing after GameOver/
    /// Victory starts a fresh run at wave 1. Difficulty grows via a per-wave
    /// runtime multiplier (M8.2). Wave 10 is the boss encounter (M8.3): one boss
    /// spawns instead of the normal schedule, and its defeat publishes
    /// BossDefeated and enters Victory.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemySpawner))]
    public class WaveManager : MonoBehaviour
    {
        public const int TotalWaves = 10;

        /// <summary>Per-wave schedule + difficulty multiplier (M8.1 tuning + M8.2 multiplier; not final difficulty design).</summary>
        public readonly struct WaveConfig
        {
            public readonly float Duration;
            public readonly int EnemyCount;
            public readonly float SpawnInterval;
            public readonly float Multiplier;

            public WaveConfig(float duration, int enemyCount, float spawnInterval, float multiplier)
            {
                Duration = duration;
                EnemyCount = enemyCount;
                SpawnInterval = spawnInterval;
                Multiplier = multiplier;
            }
        }

        // M8.1 placeholder scheduling: duration/count rise, interval falls.
        // M8.2 adds the wave difficulty multiplier (scales HP/Damage/MoveSpeed at
        // runtime only — EnemyData assets untouched). Simple base slope:
        // W1 1.00 → W10 1.45. Wave 10 currently spawns normal enemies only.
        private static readonly WaveConfig[] WaveTable =
        {
            new(8f, 5, 1.6f, 1.00f),  // W1
            new(9f, 6, 1.5f, 1.05f),  // W2
            new(10f, 7, 1.4f, 1.10f), // W3
            new(11f, 8, 1.3f, 1.15f), // W4
            new(12f, 9, 1.2f, 1.20f), // W5
            new(13f, 10, 1.1f, 1.25f),// W6
            new(14f, 11, 1.0f, 1.30f),// W7
            new(15f, 12, 0.9f, 1.35f),// W8
            new(16f, 13, 0.8f, 1.40f),// W9
            new(12f, 15, 0.7f, 1.45f),// W10 (boss entry reserved for M8.3)
        };

        private EnemySpawner _spawner;
        private bool _hasStartedRun;
        private bool _waveActive;
        private float _waveElapsed;
        private int _spawnedCount;
        private float _nextSpawnAt;
        private bool _bossSpawned;
        private EnemyController _activeBoss;

        public int CurrentWave { get; private set; } = 1;
        public bool IsWaveActive => _waveActive;
        public float WaveElapsed => _waveElapsed;
        public int SpawnedCount => _spawnedCount;
        public EnemyController ActiveBoss => _activeBoss;
        public static WaveConfig ConfigFor(int waveIndex) => WaveTable[waveIndex - 1];

        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Subscribe<EnemyKilled>(OnEnemyKilled);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
            EventBus.Unsubscribe<EnemyKilled>(OnEnemyKilled);
        }

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (e.To == GameState.Playing && !_hasStartedRun)
            {
                // First entry into Playing (or a fresh run after GameOver/Victory).
                _hasStartedRun = true;
                StartWave(1);
            }
            else if (e.To == GameState.GameOver || e.To == GameState.Victory)
            {
                _waveActive = false;
                _hasStartedRun = false; // next Playing starts a fresh run
                _activeBoss = null;
            }
        }

        /// <summary>
        /// The boss is killed through the normal combat chain: CombatSystem still
        /// publishes EnemyKilled; we match it against the active boss and then
        /// publish BossDefeated and enter Victory. (GameObject reference compare
        /// stays valid even if the boss was just released/inactive.)
        /// </summary>
        private void OnEnemyKilled(EnemyKilled e)
        {
            if (_activeBoss == null || e.Enemy != _activeBoss.gameObject) return;

            _waveActive = false;
            EventBus.Publish(new BossDefeated(e.Enemy, e.Killer));
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TryChangeState(GameState.Victory);
            }
        }

        /// <summary>Starts the given wave (public for later M8 subtasks and tests).</summary>
        public void StartWave(int index)
        {
            if (index < 1 || index > TotalWaves)
            {
                _waveActive = false;
                return;
            }

            CurrentWave = index;
            _waveElapsed = 0f;
            _spawnedCount = 0;
            _nextSpawnAt = 0f;
            _bossSpawned = false;
            _activeBoss = null;
            _waveActive = true;
            EventBus.Publish(new WaveStarted(index));
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return; // paused/stopped
            if (!_waveActive) return;

            // Wave 10 is the boss encounter: spawn exactly one boss and then run
            // no normal spawn schedule / no time-based completion — it ends via
            // BossDefeated (OnEnemyKilled).
            if (CurrentWave == TotalWaves)
            {
                if (!_bossSpawned)
                {
                    SpawnBossNow();
                }
                return;
            }

            _waveElapsed += Time.deltaTime;
            WaveConfig config = WaveTable[CurrentWave - 1];

            // Spawn until the schedule is exhausted for this wave.
            while (_spawnedCount < config.EnemyCount && _waveElapsed >= _nextSpawnAt)
            {
                SpawnOne();
                _spawnedCount++;
                _nextSpawnAt = _spawnedCount * config.SpawnInterval;
            }

            if (_waveElapsed >= config.Duration)
            {
                CompleteWave();
            }
        }

        private void SpawnBossNow()
        {
            if (_spawner == null) return;

            _bossSpawned = true;
            Vector2 origin = Vector2.zero;
            var player = GameObject.Find("Player");
            if (player != null) origin = player.transform.position;

            // Cardinal spawn point above the player (M4.6 rule), W10 multiplier.
            var boss = _spawner.SpawnBoss(_spawner.GetSpawnPosition(2, origin), WaveTable[TotalWaves - 1].Multiplier);
            _activeBoss = boss;
            if (boss != null)
            {
                EventBus.Publish(new BossSpawned(boss.gameObject));
            }
        }

        private void SpawnOne()
        {
            if (_spawner == null) return;

            var prefab = _spawner.GetEnemyPrefab(_spawnedCount); // deterministic type rotation
            if (prefab == null) return;

            Vector2 origin = Vector2.zero;
            var player = GameObject.Find("Player");
            if (player != null) origin = player.transform.position;

            WaveConfig config = WaveTable[CurrentWave - 1];
            _spawner.SpawnEnemy(prefab, _spawner.GetSpawnPosition(_spawnedCount, origin), config.Multiplier);
        }

        private void CompleteWave()
        {
            _waveActive = false;
            EventBus.Publish(new WaveCompleted(CurrentWave));

            int next = CurrentWave + 1;
            if (next <= TotalWaves)
            {
                StartWave(next);
            }
            // next > TotalWaves: waves exhausted — boss/victory handled by M8.3.
        }
    }
}
