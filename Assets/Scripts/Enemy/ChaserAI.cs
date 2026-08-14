using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Chaser behavior (M4.2): continuously pursues the player's current world
    /// position using Rigidbody2D.MovePosition at the configured MoveSpeed.
    /// Reuses the references resolved by <see cref="EnemyController"/> — no
    /// per-frame GetComponent/Find. Stops when dead. No attack/damage logic.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class ChaserAI : MonoBehaviour
    {
        private EnemyController _controller;

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
        }

        private void FixedUpdate()
        {
            if (_controller == null || _controller.Health == null || _controller.Health.IsDead) return;
            if (_controller.Body == null || _controller.Stats == null || _controller.Target == null) return;

            Vector2 toTarget = (Vector2)_controller.Target.transform.position - _controller.Body.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Vector2 next = _controller.Body.position + toTarget.normalized * (_controller.Stats.MoveSpeed * Time.fixedDeltaTime);
            _controller.Body.MovePosition(next);
        }
    }
}
