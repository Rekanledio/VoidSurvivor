using UnityEngine;
using VoidSurvivor.Core;

namespace VoidSurvivor.Pickup
{
    /// <summary>
    /// Spawns pickups when enemies die (M5.3, pooled since M7.2.3). Subscribes to
    /// <see cref="EnemyKilled"/> and takes an XP and a Gold pickup from their
    /// respective pools at the enemy's death position immediately (before the
    /// enemy is released at frame end). Pure event subscriber — no coupling to
    /// EnemyHealth/EnemyController. Deterministic MVP rule: every killed normal
    /// enemy drops 1 XP + 1 Gold. One pool per prefab, lazily created.
    /// </summary>
    [DisallowMultipleComponent]
    public class PickupSystem : MonoBehaviour
    {
        [SerializeField] private GameObject xpPickupPrefab;
        [SerializeField] private GameObject goldPickupPrefab;

        private ObjectPool<Pickup> _xpPool;
        private ObjectPool<Pickup> _goldPool;

        private void Awake()
        {
            EventBus.Subscribe<EnemyKilled>(OnEnemyKilled);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyKilled>(OnEnemyKilled);
        }

        private ObjectPool<Pickup> EnsurePool(GameObject prefab)
        {
            var pool = prefab == xpPickupPrefab ? _xpPool : _goldPool;
            if (pool == null)
            {
                var pickupPrefab = prefab.GetComponent<Pickup>();
                pool = new ObjectPool<Pickup>(pickupPrefab, 16, transform);
                if (prefab == xpPickupPrefab) _xpPool = pool;
                else _goldPool = pool;
            }
            return pool;
        }

        private void OnEnemyKilled(EnemyKilled e)
        {
            if (e.Enemy == null) return;

            // Read the position NOW — the enemy is released at frame end
            // (EnemyController death cleanup), so no delayed lookup.
            Vector2 position = e.Enemy.transform.position;

            if (xpPickupPrefab != null) Pickup.Spawn(EnsurePool(xpPickupPrefab), position);
            if (goldPickupPrefab != null) Pickup.Spawn(EnsurePool(goldPickupPrefab), position);
        }
    }
}
