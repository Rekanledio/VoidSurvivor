using System.Collections.Generic;
using UnityEngine;
using VoidSurvivor.Enemy;

namespace VoidSurvivor.Weapons
{
    /// <summary>
    /// Arc Blade (M6.5): close-range area attack. Auto-attack loop (Time.time
    /// cooldown). Each strike performs ONE Physics2D.OverlapCircleAll query
    /// centered on the player, hits every live EnemyHealth inside Range exactly
    /// once (deduped, dead targets skipped, self ignored), and routes each hit
    /// through the player attack entry → CombatSystem. No projectile, no
    /// single-target selection, no upgrade logic.
    /// </summary>
    [DisallowMultipleComponent]
    public class ArcBlade : WeaponController
    {
        private float _nextAttackTime;

        private void Update()
        {
            if (!GameplayActive || Data == null) return; // M11.4: weapons act only while Playing
            if (Time.time < _nextAttackTime) return;

            Strike();
            _nextAttackTime = Time.time + EffectiveAttackCooldown;
        }

        /// <summary>One area strike: query once, hit every in-range live enemy once.</summary>
        private void Strike()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, EffectiveRange);
            if (hits.Length == 0) return;

            var targets = new List<GameObject>();
            for (int i = 0; i < hits.Length; i++)
            {
                GameObject go = hits[i].gameObject;
                if (go == gameObject) continue;
                if (!hits[i].TryGetComponent(out EnemyHealth enemy)) continue;
                if (enemy.IsDead) continue;
                if (targets.Contains(go)) continue; // one hit per enemy per strike
                targets.Add(go);
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Attack(targets[i], EffectiveDamage);
            }
        }
    }
}
