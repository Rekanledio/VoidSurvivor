using UnityEngine;
using VoidSurvivor.Combat;
using VoidSurvivor.Core;
using VoidSurvivor.Player;

namespace VoidSurvivor.VFX
{
    /// <summary>
    /// Minimal event-driven 2D VFX (M12.3): listens to gameplay events and
    /// spawns short-lived ParticleSystem prefabs at the event position.
    /// Owns no gameplay/audio/state; no second EventBus. DamageApplied is
    /// rate-limited by a 0.05s cooldown. Prefabs auto-stop and self-destroy.
    /// </summary>
    [DisallowMultipleComponent]
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private const float HitCooldown = 0.05f;

        [Header("VFX Prefabs (Assets/Prefabs/VFX)")]
        [SerializeField] private GameObject vfxHit;
        [SerializeField] private GameObject vfxEnemyDeath;
        [SerializeField] private GameObject vfxPickup;
        [SerializeField] private GameObject vfxLevelUp;
        [SerializeField] private GameObject vfxBossSpawn;
        [SerializeField] private GameObject vfxBossDefeat;
        [SerializeField] private GameObject vfxPlayerDeath;
        [SerializeField] private GameObject vfxVictory;

        private Transform _player;
        private float _lastHitTime = -1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load prefabs via Resources (runtime-safe, works in Editor and
            // standalone builds). Prefabs live under Assets/Resources/VFX.
            LoadViaResources();

            // Cache the player ONCE (PlayerLevelUp / PlayerDied / Victory events
            // carry no object reference).
            var p = GameObject.Find("Player");
            _player = p != null ? p.transform : null;

            EventBus.Subscribe<DamageApplied>(OnDamageApplied);
            EventBus.Subscribe<EnemyDied>(OnEnemyDied);
            EventBus.Subscribe<PickupCollected>(OnPickupCollected);
            EventBus.Subscribe<PlayerLevelUp>(OnPlayerLevelUp);
            EventBus.Subscribe<BossSpawned>(OnBossSpawned);
            EventBus.Subscribe<BossDefeated>(OnBossDefeated);
            EventBus.Subscribe<PlayerDied>(OnPlayerDied);
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DamageApplied>(OnDamageApplied);
            EventBus.Unsubscribe<EnemyDied>(OnEnemyDied);
            EventBus.Unsubscribe<PickupCollected>(OnPickupCollected);
            EventBus.Unsubscribe<PlayerLevelUp>(OnPlayerLevelUp);
            EventBus.Unsubscribe<BossSpawned>(OnBossSpawned);
            EventBus.Unsubscribe<BossDefeated>(OnBossDefeated);
            EventBus.Unsubscribe<PlayerDied>(OnPlayerDied);
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void LoadViaResources()
        {
            vfxHit = Load("VFX/VFX_Hit");
            vfxEnemyDeath = Load("VFX/VFX_EnemyDeath");
            vfxPickup = Load("VFX/VFX_Pickup");
            vfxLevelUp = Load("VFX/VFX_LevelUp");
            vfxBossSpawn = Load("VFX/VFX_BossSpawn");
            vfxBossDefeat = Load("VFX/VFX_BossDefeat");
            vfxPlayerDeath = Load("VFX/VFX_PlayerDeath");
            vfxVictory = Load("VFX/VFX_Victory");
        }

        private static GameObject Load(string resName)
        {
            var go = Resources.Load<GameObject>(resName);
            if (go == null)
                Debug.LogWarning($"[VFXManager] Missing VFX resource: {resName}");
            return go;
        }

        private void Play(GameObject prefab, Vector3 position, float scale = 1f)
        {
            if (prefab == null) return;

            var go = Instantiate(prefab, position, Quaternion.identity);
            if (scale != 1f) go.transform.localScale = Vector3.one * scale;
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(go, ps.main.duration + 0.2f);
            }
            else
            {
                Destroy(go);
            }
        }

        private Vector3 PlayerPos()
        {
            return _player != null ? _player.position : Vector3.zero;
        }

        private void OnDamageApplied(DamageApplied e)
        {
            // Rate-limit high-frequency hits (same-frame bursts -> 1 effect).
            if (Time.time - _lastHitTime < HitCooldown) return;
            _lastHitTime = Time.time;

            if (e.Target != null) Play(vfxHit, e.Target.transform.position);
        }

        private void OnEnemyDied(EnemyDied e)
        {
            if (e.Enemy != null) Play(vfxEnemyDeath, e.Enemy.transform.position);
        }

        private void OnPickupCollected(PickupCollected e)
        {
            if (e.Collector != null) Play(vfxPickup, e.Collector.transform.position);
        }

        private void OnPlayerLevelUp(PlayerLevelUp e) => Play(vfxLevelUp, PlayerPos());

        private void OnBossSpawned(BossSpawned e)
        {
            if (e.Boss != null) Play(vfxBossSpawn, e.Boss.transform.position, 2f);
        }

        private void OnBossDefeated(BossDefeated e)
        {
            if (e.Boss != null) Play(vfxBossDefeat, e.Boss.transform.position, 2f);
        }

        private void OnPlayerDied(PlayerDied e) => Play(vfxPlayerDeath, PlayerPos());

        private void OnGameStateChanged(GameStateChanged e)
        {
            if (e.To == GameState.Victory) Play(vfxVictory, PlayerPos(), 2f);
        }
    }
}
