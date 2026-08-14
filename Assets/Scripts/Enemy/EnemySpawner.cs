using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Minimal spawn entry (M4.6): instantiates the configured enemy prefabs
    /// once around the player when the game starts. Spawns exactly one of each
    /// prefab in the list at fixed offsets around the player (no wave logic,
    /// no timers, no loops). Enemies run their own AI via EnemyController.
    /// Full wave management belongs to M8.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Configuration (M4.6)")]
        [SerializeField, Tooltip("Enemy prefabs to spawn, one each, in list order.")]
        private List<GameObject> enemyPrefabs = new();

        [SerializeField, Tooltip("Distance from the player at which enemies spawn.")]
        private float spawnDistance = 10f;

        private void Start()
        {
            SpawnOnce();
        }

        /// <summary>Spawns one instance of each configured prefab around the player.</summary>
        private void SpawnOnce()
        {
            Vector2 origin = Vector2.zero;
            var player = GameObject.Find("Player");
            if (player != null)
            {
                origin = player.transform.position;
            }

            // Fixed cardinal offsets so enemies never spawn on the player.
            Vector2[] offsets =
            {
                Vector2.left,
                Vector2.right,
                Vector2.up,
                Vector2.down,
            };

            for (int i = 0; i < enemyPrefabs.Count && i < offsets.Length; i++)
            {
                GameObject prefab = enemyPrefabs[i];
                if (prefab == null)
                {
                    Debug.LogWarning($"[EnemySpawner] Enemy prefab at index {i} is not assigned; skipped.");
                    continue;
                }

                Instantiate(prefab, origin + offsets[i] * spawnDistance, Quaternion.identity);
            }
        }
    }
}
