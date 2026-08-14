using UnityEngine;

namespace VoidSurvivor.Pickup
{
    /// <summary>
    /// Static pickup configuration (M5.3): which reward kind and how much.
    /// Assets are never mutated at runtime — Pickup reads Type/Amount only.
    /// </summary>
    [CreateAssetMenu(fileName = "PickupData", menuName = "VoidSurvivor/Pickup Data")]
    public class PickupData : ScriptableObject
    {
        [SerializeField] private PickupType type = PickupType.XP;
        [SerializeField] private int amount = 10;

        public PickupType Type => type;
        public int Amount => amount;
    }
}
