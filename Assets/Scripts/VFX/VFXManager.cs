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

            // Load prefabs from Resources-free path (Assets/Prefabs is editor-only
            // at runtime in builds, so load via Resources for runtime availability).
            // NOTE: prefabs live under Assets/Prefabs/VFX; loaded via AssetDatabase
            // in-editor. For runtime builds they must be assigned in the scene or
            // loaded from Resources; here we load from Resources/Audio? No —
            // keep them in Assets/Prefabs and assign via code below (editor run).
            LoadViaAssetDatabase();

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

        private void LoadViaAssetDatabase()
        {
            var dir = "Assets/Prefabs/VFX/";
            vfxHit = Load(dir + "VFX_Hit.prefab");
            vfxEnemyDeath = Load(dir + "VFX_EnemyDeath.prefab");
            vfxPickup = Load(dir + "VFX_Pickup.prefab");
            vfxLevelUp = Load(dir + "VFX_LevelUp.prefab");
            vfxBossSpawn = Load(dir + "VFX_BossSpawn.prefab");
            vfxBossDefeat = Load(dir + "VFX_BossDefeat.prefab");
            vfxPlayerDeath = Load(dir + "VFX_PlayerDeath.prefab");
            vfxVictory = Load(dir + "VFX_Victory.prefab");
        }

        private static GameObject Load(string path)
        {
            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
                Debug.LogWarning($"[VFXManager] Missing VFX prefab: {path}");
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
