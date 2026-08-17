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
    /// M5.2: acts as the enemy death/despawn layer — on its own
    /// <see cref="EnemyDied"/> it releases itself back to the pool (M7.2.2).
    /// M7.2.2: pooled — Spawn() from an <see cref="ObjectPool{T}"/>, OnSpawn
    /// resets health + notifies child AI components, OnDespawn stops physics and
    /// notifies child AI. EventBus subscription happens once in Awake (instance
    /// creation); pool reuse never re-subscribes.
    ///
    /// No AI behavior is implemented here — Chaser / Runner / Shooter / Tank
    /// compose this component in later M4 subtasks.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyController : MonoBehaviour, IPoolable
    {
        private EnemyStats _stats;
        private EnemyHealth _health;
        private Rigidbody2D _body;
        private PlayerHealth _target;
        private ObjectPool<EnemyController> _myPool;

        public EnemyStats Stats => _stats;

        /// <summary>
        /// M11.4: enemies act only while the game is Playing. Non-gameplay states
        /// (MainMenu / GameOver / Victory / Paused / LevelUp / Shop) freeze enemy
        /// AI (no movement, no shooting). Existing projectiles keep their pooled
        /// lifecycle.
        /// </summary>
        public static bool GameplayActive =>
            GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;
        public EnemyHealth Health => _health;
        public Rigidbody2D Body => _body;
        public PlayerHealth Target => _target;

        /// <summary>Gets an enemy from the pool, applies the wave multiplier and places it.</summary>
        public static EnemyController Spawn(ObjectPool<EnemyController> pool, Vector2 position, float waveMultiplier = 1f)
        {
            var enemy = pool.Get();
            enemy._myPool = pool;
            enemy.transform.position = position;
            // Order matters: multiplier FIRST, then ResetForSpawn so CurrentHP uses
            // the scaled MaxHP (M8.2). Pool reuse overwrites the previous wave's value.
            enemy._stats.WaveMultiplier = waveMultiplier;
            enemy._health.ResetForSpawn();
            return enemy;
        }

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

        private void Start()
        {
            // Resolve the player target once (not a hot path). PlayerHealth is a
            // stable singleton in the MVP scene. Runs on the first active frame
            // of each instance (pool warmup instances get it on their first Get).
            _target = Object.FindFirstObjectByType<PlayerHealth>();
        }

        private void OnEnemyDied(EnemyDied e)
        {
            if (e.Enemy == gameObject)
            {
                DespawnSelf();
            }
        }

        public void OnSpawn()
        {
            // Restore a "brand new enemy" state: full HP, alive.
            _health.ResetForSpawn();
            // Let per-type AI components (which implement IPoolable) reset too.
            var poolables = GetComponents<IPoolable>();
            for (int i = 0; i < poolables.Length; i++)
            {
                if (!ReferenceEquals(poolables[i], this)) poolables[i].OnSpawn();
            }
        }

        public void OnDespawn()
        {
            // Stop child AI state first, then stop physics.
            var poolables = GetComponents<IPoolable>();
            for (int i = 0; i < poolables.Length; i++)
            {
                if (!ReferenceEquals(poolables[i], this)) poolables[i].OnDespawn();
            }
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.angularVelocity = 0f;
            }
            // Reset the wave multiplier so a direct pool.Get (outside Spawn) can
            // never leak a previous wave's difficulty (M8.2).
            if (_stats != null) _stats.WaveMultiplier = 1f;
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
