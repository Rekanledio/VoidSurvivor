using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.Pickup
{
    /// <summary>
    /// Collectible reward (M5.3): a trigger that, on player contact, credits the
    /// player's <see cref="PlayerProgress"/> with its PickupData amount, publishes
    /// <see cref="PickupCollected"/> and destroys itself. No Update-loop player
    /// search; the collider event is the detection mechanism. Object Pool arrives M7.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupData data;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (data == null) return;

            if (other.TryGetComponent(out PlayerProgress progress))
            {
                if (data.Type == PickupType.XP) progress.AddXP(data.Amount);
                else progress.AddGold(data.Amount);

                EventBus.Publish(new PickupCollected(data.Type, data.Amount, other.gameObject));
                Destroy(gameObject);
            }
        }
    }
}
