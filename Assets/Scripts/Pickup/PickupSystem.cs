using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Pickup
{
    /// <summary>
    /// Spawns pickups when enemies die (M5.3). Subscribes to
    /// <see cref="EnemyKilled"/> and instantiates an XP and a Gold pickup at the
    /// enemy's death position immediately (before the enemy is destroyed at
    /// frame end). Pure event subscriber — no coupling to EnemyHealth/EnemyController.
    /// Deterministic MVP rule: every killed normal enemy drops 1 XP + 1 Gold.
    /// </summary>
    [DisallowMultipleComponent]
    public class PickupSystem : MonoBehaviour
    {
        [SerializeField] private GameObject xpPickupPrefab;
        [SerializeField] private GameObject goldPickupPrefab;

        private void Awake()
        {
            EventBus.Subscribe<EnemyKilled>(OnEnemyKilled);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyKilled>(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyKilled e)
        {
            if (e.Enemy == null) return;

            // Read the position NOW — the enemy is destroyed at frame end
            // (EnemyController death cleanup), so no delayed lookup.
            Vector2 position = e.Enemy.transform.position;

            if (xpPickupPrefab != null) Instantiate(xpPickupPrefab, position, Quaternion.identity);
            if (goldPickupPrefab != null) Instantiate(goldPickupPrefab, position, Quaternion.identity);
        }
    }
}
