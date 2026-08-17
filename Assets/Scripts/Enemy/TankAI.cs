using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Tank behavior (M4.5): a slow, high-HP pursuer. Uses the same pursuit
    /// pattern as ChaserAI — Rigidbody2D.MovePosition toward the player at the
    /// configured MoveSpeed (low, via TankData), reuses EnemyController refs
    /// (no per-frame GetComponent/Find), stops when dead. No special attack or
    /// extra mechanics — type identity comes from data (low speed, high HP).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class TankAI : MonoBehaviour
    {
        private EnemyController _controller;

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
        }

        private void FixedUpdate()
        {
            if (!EnemyController.GameplayActive) return; // M11.4
            if (_controller == null || _controller.Health == null || _controller.Health.IsDead) return;
            if (_controller.Body == null || _controller.Stats == null || _controller.Target == null) return;

            Vector2 toTarget = (Vector2)_controller.Target.transform.position - _controller.Body.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Vector2 next = _controller.Body.position + toTarget.normalized * (_controller.Stats.MoveSpeed * Time.fixedDeltaTime);
            _controller.Body.MovePosition(next);
        }
    }
}
