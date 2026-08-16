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
    /// Victory starts a fresh run at wave 1. No difficulty scaling (M8.2), no
    /// boss (M8.3), no Victory/Boss logic.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemySpawner))]
    public class WaveManager : MonoBehaviour
    {
        public const int TotalWaves = 10;

        /// <summary>Per-wave schedule (M8.1 placeholder tuning, not final difficulty design).</summary>
        public readonly struct WaveConfig
        {
            public readonly float Duration;
            public readonly int EnemyCount;
            public readonly float SpawnInterval;

            public WaveConfig(float duration, int enemyCount, float spawnInterval)
            {
                Duration = duration;
                EnemyCount = enemyCount;
                SpawnInterval = spawnInterval;
            }
        }

        // MVP placeholder scheduling: duration/count rise, interval falls —
        // a simple ramp toward harder waves WITHOUT touching enemy stats.
        // Wave 10 currently spawns normal enemies only; the boss is M8.3.
        private static readonly WaveConfig[] WaveTable =
        {
            new(8f, 5, 1.6f),   // W1
            new(9f, 6, 1.5f),   // W2
            new(10f, 7, 1.4f),  // W3
            new(11f, 8, 1.3f),  // W4
            new(12f, 9, 1.2f),  // W5
            new(13f, 10, 1.1f), // W6
            new(14f, 11, 1.0f), // W7
            new(15f, 12, 0.9f), // W8
            new(16f, 13, 0.8f), // W9
            new(12f, 15, 0.7f), // W10 (boss entry reserved for M8.3)
        };

        private EnemySpawner _spawner;
        private bool _hasStartedRun;
        private bool _waveActive;
        private float _waveElapsed;
        private int _spawnedCount;
        private float _nextSpawnAt;

        public int CurrentWave { get; private set; } = 1;
        public bool IsWaveActive => _waveActive;
        public float WaveElapsed => _waveElapsed;
        public int SpawnedCount => _spawnedCount;
        public static WaveConfig ConfigFor(int waveIndex) => WaveTable[waveIndex - 1];

        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
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
            _waveActive = true;
            EventBus.Publish(new WaveStarted(index));
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return; // paused/stopped
            if (!_waveActive) return;

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

        private void SpawnOne()
        {
            if (_spawner == null) return;

            var prefab = _spawner.GetEnemyPrefab(_spawnedCount); // deterministic type rotation
            if (prefab == null) return;

            Vector2 origin = Vector2.zero;
            var player = GameObject.Find("Player");
            if (player != null) origin = player.transform.position;

            _spawner.SpawnEnemy(prefab, _spawner.GetSpawnPosition(_spawnedCount, origin));
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
