using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.Pickup
{
    /// <summary>
    /// Collectible reward (M5.3, pooled since M7.2.3): a trigger that, on player
    /// contact, credits the player's <see cref="PlayerProgress"/> with its
    /// PickupData amount, publishes <see cref="PickupCollected"/> and releases
    /// itself back to its pool. No Update-loop player search; the collider event
    /// is the detection mechanism.
    ///
    /// Pooling (M7.2.3): spawned via <see cref="Spawn"/> from an
    /// <see cref="ObjectPool{T}"/> held by PickupSystem (one pool per prefab).
    /// Collection → DespawnSelf → Release. The pickup has NO runtime state
    /// (its PickupData is static per prefab), so OnSpawn/OnDespawn are no-ops —
    /// recycling is inherently clean; being inactive stops the trigger.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class Pickup : MonoBehaviour, IPoolable
    {
        [SerializeField] private PickupData data;

        private ObjectPool<Pickup> _myPool;

        /// <summary>Gets a pickup from the pool, places it and records the owning pool.</summary>
        public static Pickup Spawn(ObjectPool<Pickup> pool, Vector2 position)
        {
            var pickup = pool.Get();
            pickup._myPool = pool;
            pickup.transform.position = position;
            return pickup;
        }

        public void OnSpawn()
        {
            // No per-pickup runtime state to reset (PickupData is static per prefab).
        }

        public void OnDespawn()
        {
            // No velocity/physics state to clear (no Rigidbody2D); being inactive
            // stops the trigger and any further collection.
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (data == null) return;

            if (other.TryGetComponent(out PlayerProgress progress))
            {
                if (data.Type == PickupType.XP) progress.AddXP(data.Amount);
                else progress.AddGold(data.Amount);

                EventBus.Publish(new PickupCollected(data.Type, data.Amount, other.gameObject));
                DespawnSelf();
            }
        }

        private void DespawnSelf()
        {
            if (_myPool != null)
            {
                _myPool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
