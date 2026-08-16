using System.Collections.Generic;
using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Minimal spawn entry (M4.6, pooled since M7.2.2, wave-driven since M8.1).
    /// Owns the per-prefab <see cref="ObjectPool{T}"/>s and exposes the public
    /// spawn entry used by <see cref="WaveManager"/> (M8.1). No wave logic, no
    /// timers, no loops here — the initial Start-time automatic spawn was
    /// removed in M8.1; WaveManager now drives generation.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Configuration (M4.6)")]
        [SerializeField, Tooltip("Enemy prefabs available to waves, in list order.")]
        private List<GameObject> enemyPrefabs = new();

        [SerializeField, Tooltip("Distance from the player at which enemies spawn.")]
        private float spawnDistance = 10f;

        [SerializeField, Tooltip("Wave 10 boss prefab (M8.3); pooled via the same enemy pool dictionary.")]
        private GameObject bossPrefab;

        private static readonly Vector2[] CardinalOffsets =
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down,
        };

        // One pool per prefab, created lazily on first use (M7.2.2).
        private readonly Dictionary<GameObject, ObjectPool<EnemyController>> _pools = new();

        public int EnemyPrefabCount => enemyPrefabs.Count;

        /// <summary>Deterministic prefab selection (index cycles the list).</summary>
        public GameObject GetEnemyPrefab(int index)
        {
            if (enemyPrefabs.Count == 0) return null;
            return enemyPrefabs[((index % enemyPrefabs.Count) + enemyPrefabs.Count) % enemyPrefabs.Count];
        }

        /// <summary>Cardinal spawn point around the player (M4.6 rule), cycling offsets.</summary>
        public Vector2 GetSpawnPosition(int offsetIndex, Vector2 playerPosition)
        {
            return playerPosition + CardinalOffsets[offsetIndex % CardinalOffsets.Length] * spawnDistance;
        }

        /// <summary>Public pooled spawn entry used by WaveManager (M8.1, multiplier since M8.2).</summary>
        public EnemyController SpawnEnemy(GameObject prefab, Vector2 position, float multiplier = 1f)
        {
            if (prefab == null) return null;
            return EnemyController.Spawn(GetPool(prefab), position, multiplier);
        }

        /// <summary>Public pooled boss spawn entry (M8.3); same pool dictionary, no second pool system.</summary>
        public EnemyController SpawnBoss(Vector2 position, float multiplier)
        {
            if (bossPrefab == null) return null;
            return EnemyController.Spawn(GetPool(bossPrefab), position, multiplier);
        }

        private ObjectPool<EnemyController> GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                var controllerPrefab = prefab.GetComponent<EnemyController>();
                pool = new ObjectPool<EnemyController>(controllerPrefab, 1, transform);
                _pools.Add(prefab, pool);
            }
            return pool;
        }
    }
}
