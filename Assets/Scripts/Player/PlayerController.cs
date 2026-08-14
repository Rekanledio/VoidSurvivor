using UnityEngine;
using UnityEngine.InputSystem;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Drives player movement from the Input System. Owns no HP/stats logic —
    /// those live in PlayerStats / PlayerHealth. Movement math is exposed as
    /// public static helpers so it can be verified without physical input.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("InputSystem_Actions asset. The Move action is resolved by name at runtime.")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Movement Bounds (placeholder arena, M3)")]
        [Tooltip("If true, clamps the player to arenaBoundsHalfExtents around the origin.")]
        [SerializeField] private bool clampToBounds = true;
        [SerializeField] private Vector2 arenaBoundsHalfExtents = new(20f, 20f);

        private PlayerStats _stats;
        private PlayerHealth _health;
        private Rigidbody2D _body;
        private InputAction _moveAction;

        public Vector2 LastMoveInput { get; private set; }

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _health = GetComponent<PlayerHealth>();
            _body = GetComponent<Rigidbody2D>();

            if (_stats == null) Debug.LogError($"[PlayerController] Missing PlayerStats on '{gameObject.name}'.");
            if (_body == null) Debug.LogError($"[PlayerController] Missing Rigidbody2D on '{gameObject.name}'.");

            // Resolve the Move action from the asset. Using the asset reference
            // (instead of an InputActionReference) keeps serialization reliable.
            if (inputActions != null)
            {
                _moveAction = inputActions.FindAction("Move");
                if (_moveAction == null) Debug.LogError("[PlayerController] Move action not found in input asset.");
            }
            else
            {
                Debug.LogError("[PlayerController] No inputActions asset assigned.");
            }
        }

        private void OnEnable()
        {
            // Idempotent: enabling an already-enabled action is a no-op.
            _moveAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
        }

        private void FixedUpdate()
        {
            if (_stats == null || _body == null || (_health != null && _health.IsDead)) return;

            Vector2 raw = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
            LastMoveInput = NormalizeMoveInput(raw);

            Vector2 velocity = LastMoveInput * _stats.MoveSpeed;

            Vector2 next = _body.position + velocity * Time.fixedDeltaTime;
            if (clampToBounds)
            {
                next = ClampToBounds(next, arenaBoundsHalfExtents);
            }

            _body.MovePosition(next);
        }

        /// <summary>
        /// Normalizes input so diagonal movement has the same speed as cardinal
        /// (vector magnitude capped at 1; analog sticks keep their partial range).
        /// </summary>
        public static Vector2 NormalizeMoveInput(Vector2 input)
        {
            if (input.sqrMagnitude > 1f)
            {
                return input.normalized;
            }
            return input;
        }

        /// <summary>Clamps a position to the symmetric arena bounds around the origin.</summary>
        public static Vector2 ClampToBounds(Vector2 position, Vector2 halfExtents)
        {
            position.x = Mathf.Clamp(position.x, -halfExtents.x, halfExtents.x);
            position.y = Mathf.Clamp(position.y, -halfExtents.y, halfExtents.y);
            return position;
        }
    }
}
