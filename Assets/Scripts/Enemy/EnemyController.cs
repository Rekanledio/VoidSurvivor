using UnityEngine;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.Enemy
{
    /// <summary>
    /// Common control base for every enemy (M4.1). Owns the runtime references
    /// shared by all enemy types (stats, health, physics body, player target)
    /// and provides the extension point for per-type AI (M4.2+).
    ///
    /// M5.2: also acts as the enemy death/despawn layer — on its own
    /// <see cref="EnemyDied"/> it destroys the enemy GameObject (plain Destroy;
    /// Object Pool arrives in M7).
    ///
    /// No AI behavior is implemented here — Chaser / Runner / Shooter / Tank
    /// derive from or compose this component in later M4 subtasks.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyController : MonoBehaviour
    {
        private EnemyStats _stats;
        private EnemyHealth _health;
        private Rigidbody2D _body;
        private PlayerHealth _target;

        public EnemyStats Stats => _stats;
        public EnemyHealth Health => _health;
        public Rigidbody2D Body => _body;
        public PlayerHealth Target => _target;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _health = GetComponent<EnemyHealth>();
            _body = GetComponent<Rigidbody2D>();

            if (_stats == null) Debug.LogError($"[EnemyController] Missing EnemyStats on '{gameObject.name}'.");
            if (_health == null) Debug.LogError($"[EnemyController] Missing EnemyHealth on '{gameObject.name}'.");
            if (_body == null) Debug.LogError($"[EnemyController] Missing Rigidbody2D on '{gameObject.name}'.");

            EventBus.Subscribe<EnemyDied>(OnEnemyDied);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDied>(OnEnemyDied);
        }

        private void OnEnemyDied(EnemyDied e)
        {
            if (e.Enemy == gameObject)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Resolve the player target once (not a hot path). PlayerHealth is a
            // stable singleton in the MVP scene; re-lookup per enemy type if the
            // player is ever pooled/respawned (M7/M12).
            _target = Object.FindFirstObjectByType<PlayerHealth>();
        }
    }
}
