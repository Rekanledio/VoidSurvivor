using UnityEngine;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Runner behavior (M4.3): a faster pursuer. Continuously chases the player's
    /// current world position using Rigidbody2D.MovePosition at the configured
    /// MoveSpeed (higher than Chaser via RunnerData). Reuses the references
    /// resolved by <see cref="EnemyController"/> — no per-frame GetComponent/Find.
    /// Stops when dead. No attack/damage logic.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class RunnerAI : MonoBehaviour
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
