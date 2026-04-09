// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does squad-based combat with synergy mechanics feel satisfying?
// Date: 2026-04-07

using UnityEngine;
using System.Collections.Generic;

namespace TowerDefenseRush.Prototype
{
    /// <summary>
    /// 协同攻击系统 - 核心玩法机制
    /// 连续攻击同一目标3次触发协同爆发
    /// </summary>
    public class SynergySystem : MonoBehaviour
    {
        [Header("Settings")]
        public int comboThreshold = 3;
        public float comboResetTime = 2f;
        public float synergyDamageMultiplier = 2f;
        public float synergyStunDuration = 0.5f;

        [Header("Visual")]
        public GameObject comboIndicatorPrefab;
        public GameObject synergyBurstEffect;
        public AudioClip synergySFX;

        // 追踪每个目标的受击计数
        private Dictionary<Enemy, int> targetComboCount = new Dictionary<Enemy, int>();
        private Dictionary<Enemy, float> targetLastHitTime = new Dictionary<Enemy, float>();

        public static SynergySystem Instance;

        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 记录一次攻击，返回是否触发协同
        /// </summary>
        public bool RegisterHit(Enemy target, float baseDamage, Transform attacker)
        {
            if (target == null || !target.IsAlive) return false;

            float currentTime = Time.time;

            // 检查是否重置计数
            if (targetLastHitTime.ContainsKey(target))
            {
                if (currentTime - targetLastHitTime[target] > comboResetTime)
                {
                    targetComboCount[target] = 0;
                }
            }

            // 增加计数
            if (!targetComboCount.ContainsKey(target))
            {
                targetComboCount[target] = 0;
            }
            targetComboCount[target]++;
            targetLastHitTime[target] = currentTime;

            int currentCombo = targetComboCount[target];

            // 显示连击指示
            ShowComboIndicator(target, currentCombo);

            // 检查是否触发协同攻击
            if (currentCombo >= comboThreshold)
            {
                TriggerSynergyAttack(target, baseDamage, attacker);
                targetComboCount[target] = 0;
                return true;
            }

            return false;
        }

        void ShowComboIndicator(Enemy target, int combo)
        {
            // 在敌人头顶显示 1... 2... SYNERGY!
            Vector3 pos = target.transform.position + Vector3.up * 1.5f;

            string text = combo switch
            {
                1 => "1",
                2 => "2",
                _ => combo.ToString()
            };

            // 使用Debug.DrawLine暂时替代UI
            Debug.DrawLine(pos, pos + Vector3.up * 0.5f, combo == 2 ? Color.yellow : Color.white, 0.5f);

            if (combo == 2)
            {
                Debug.Log($"即将触发协同! 目标: {target.name}");
            }
        }

        void TriggerSynergyAttack(Enemy target, float baseDamage, Transform attacker)
        {
            // 协同伤害
            float synergyDamage = baseDamage * synergyDamageMultiplier;
            target.TakeDamage(synergyDamage);

            // 眩晕效果
            target.Stun(synergyStunDuration);

            // 视觉特效
            if (synergyBurstEffect != null)
            {
                Instantiate(synergyBurstEffect, target.transform.position, Quaternion.identity);
            }

            // 时间缩放效果（短暂慢动作）
            StartCoroutine(TimeSlowEffect());

            // 屏幕震动
            CameraShake.Instance?.Shake(0.3f, 0.2f);

            Debug.Log($"<color=cyan>协同攻击触发!</color> 对 {target.name} 造成 {synergyDamage} 伤害");
        }

        System.Collections.IEnumerator TimeSlowEffect()
        {
            Time.timeScale = 0.3f;
            yield return new WaitForSecondsRealtime(0.15f);
            Time.timeScale = 1f;
        }

        void Update()
        {
            // 清理死亡敌人的记录
            List<Enemy> toRemove = new List<Enemy>();
            foreach (var kvp in targetComboCount)
            {
                if (kvp.Key == null || !kvp.Key.IsAlive)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var enemy in toRemove)
            {
                targetComboCount.Remove(enemy);
                targetLastHitTime.Remove(enemy);
            }
        }
    }
}
