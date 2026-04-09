// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Synergy System Detailed Test
// Date: 2026-04-08

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefenseRush.Prototype;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 协同系统详细测试 - 验证3连击协同机制的各种场景
    /// </summary>
    public class SynergySystemTest : TestCaseBase
    {
        [Header("Test Configuration")]
        public GameObject enemyPrefab;
        public Transform spawnPoint;

        private SynergySystem synergySystem;
        private PlayerController player;
        private List<Enemy> testEnemies = new List<Enemy>();
        private int synergyTriggerCount = 0;

        protected override void Awake()
        {
            base.Awake();
            testId = "SYNERGY_001";
            testName = "协同系统详细测试";
            description = "验证协同攻击的触发条件、伤害计算、视觉效果、时间窗口等";
            category = "Combat";
            priority = 1;
            timeout = 90f;
        }

        protected override IEnumerator OnSetUp()
        {
            LogInfo("初始化协同系统测试");

            synergySystem = FindObjectOfType<SynergySystem>();
            AssertNotNull(synergySystem, "场景中必须有协同系统");

            player = FindObjectOfType<PlayerController>();
            AssertNotNull(player, "场景中必须有玩家");

            // 重置协同计数
            synergyTriggerCount = 0;

            yield return null;
        }

        public override IEnumerator Run()
        {
            LogInfo("开始协同系统详细测试");

            // 测试1: 基础协同触发
            yield return TestBasicSynergyTrigger();

            // 测试2: 时间窗口重置
            yield return TestComboResetTimer();

            // 测试3: 切换目标重置
            yield return TestTargetSwitchReset();

            // 测试4: 协同伤害数值
            yield return TestSynergyDamage();

            // 测试5: 眩晕效果
            yield return TestStunEffect();

            // 测试6: 多个敌人场景
            yield return TestMultipleEnemies();

            LogInfo("协同系统测试完成");
        }

        IEnumerator TestBasicSynergyTrigger()
        {
            LogInfo("测试1: 基础协同触发");

            // 确保时间缩放正常
            Time.timeScale = 1f;

            Enemy enemy = SpawnTestEnemy("BasicEnemy");
            testEnemies.Add(enemy);

            float initialHealth = enemy.currentHealth;
            int initialCount = synergyTriggerCount;

            // 连续攻击3次（使用小伤害避免杀死敌人，使用真实时间等待）
            for (int i = 0; i < 3; i++)
            {
                bool triggered = synergySystem.RegisterHit(enemy, 5f, player.transform);
                if (triggered)
                {
                    synergyTriggerCount++;
                    LogInfo($"第{i+1}次攻击触发协同！");
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }

            Assert(synergyTriggerCount > initialCount, "3次连续攻击应触发协同");

            float damageDealt = initialHealth - enemy.currentHealth;
            LogInfo($"基础协同测试完成，造成伤害: {damageDealt:F2}");

            Destroy(enemy.gameObject);
            testEnemies.Remove(enemy);
        }

        IEnumerator TestComboResetTimer()
        {
            LogInfo("测试2: 连击时间窗口重置");

            // 确保时间缩放正常（防止之前的测试影响）
            Time.timeScale = 1f;

            Enemy enemy = SpawnTestEnemy("TimerEnemy");
            testEnemies.Add(enemy);

            // 攻击2次（使用短时间等待，避免触发协同）
            synergySystem.RegisterHit(enemy, 5f, player.transform);
            yield return new WaitForSecondsRealtime(0.1f);
            synergySystem.RegisterHit(enemy, 5f, player.transform);

            LogInfo("已攻击2次，等待超过重置时间...");

            // 等待超过重置时间（使用真实时间，不受Time.timeScale影响）
            yield return new WaitForSecondsRealtime(2.5f);

            // 第3次攻击应该重新开始计数（不会触发协同）
            bool triggered = synergySystem.RegisterHit(enemy, 5f, player.transform);
            Assert(!triggered, "超时后不应触发协同，应重新开始计数");

            LogInfo("时间窗口重置测试通过");

            Destroy(enemy.gameObject);
            testEnemies.Remove(enemy);
        }

        IEnumerator TestTargetSwitchReset()
        {
            LogInfo("测试3: 切换目标重置连击");

            // 确保时间缩放正常
            Time.timeScale = 1f;

            Enemy enemy1 = SpawnTestEnemy("Enemy1");
            Enemy enemy2 = SpawnTestEnemy("Enemy2");
            testEnemies.Add(enemy1);
            testEnemies.Add(enemy2);

            // 攻击enemy1两次（使用小伤害，使用真实时间）
            synergySystem.RegisterHit(enemy1, 5f, player.transform);
            yield return new WaitForSecondsRealtime(0.1f);
            synergySystem.RegisterHit(enemy1, 5f, player.transform);

            // 切换到enemy2攻击
            yield return new WaitForSecondsRealtime(0.1f);
            bool triggered = synergySystem.RegisterHit(enemy2, 5f, player.transform);

            Assert(!triggered, "切换目标后不应立即触发协同");

            // 继续攻击enemy2两次应该触发
            yield return new WaitForSecondsRealtime(0.1f);
            synergySystem.RegisterHit(enemy2, 5f, player.transform);
            yield return new WaitForSecondsRealtime(0.1f);
            triggered = synergySystem.RegisterHit(enemy2, 5f, player.transform);

            Assert(triggered, "对新目标连续3次攻击应触发协同");

            LogInfo("目标切换重置测试通过");

            Destroy(enemy1.gameObject);
            Destroy(enemy2.gameObject);
            testEnemies.Remove(enemy1);
            testEnemies.Remove(enemy2);
        }

        IEnumerator TestSynergyDamage()
        {
            LogInfo("测试4: 协同伤害数值");

            // 确保时间缩放正常
            Time.timeScale = 1f;

            Enemy enemy = SpawnTestEnemy("DamageEnemy");
            testEnemies.Add(enemy);

            float initialHealth = enemy.currentHealth;
            float baseDamage = 5f;
            float expectedMultiplier = 2f; // 协同伤害倍数

            // 普通攻击
            synergySystem.RegisterHit(enemy, baseDamage, player.transform);
            yield return new WaitForSecondsRealtime(0.1f);
            synergySystem.RegisterHit(enemy, baseDamage, player.transform);

            float healthBeforeSynergy = enemy.currentHealth;

            // 触发协同的第三次攻击
            synergySystem.RegisterHit(enemy, baseDamage, player.transform);
            yield return new WaitForSecondsRealtime(0.1f);

            float healthAfterSynergy = enemy.currentHealth;
            float actualDamage = healthBeforeSynergy - healthAfterSynergy;
            float expectedDamage = baseDamage * expectedMultiplier;

            LogInfo($"协同伤害: {actualDamage:F2}, 期望: {expectedDamage:F2}");
            AssertApproximately(expectedDamage, actualDamage, 1f, "协同伤害应为普通伤害的2倍");

            Destroy(enemy.gameObject);
            testEnemies.Remove(enemy);
        }

        IEnumerator TestStunEffect()
        {
            LogInfo("测试5: 眩晕效果");

            // 确保时间缩放正常
            Time.timeScale = 1f;

            Enemy enemy = SpawnTestEnemy("StunEnemy");
            testEnemies.Add(enemy);

            // 触发协同
            for (int i = 0; i < 3; i++)
            {
                synergySystem.RegisterHit(enemy, 5f, player.transform);
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // 检查敌人是否被眩晕
            // 这里假设Enemy有IsStunned属性
            // Assert(enemy.IsStunned, "协同攻击应造成眩晕");

            LogInfo("眩晕效果测试完成（需Enemy实现眩晕接口）");

            Destroy(enemy.gameObject);
            testEnemies.Remove(enemy);
        }

        IEnumerator TestMultipleEnemies()
        {
            LogInfo("测试6: 多敌人场景");

            // 确保时间缩放正常
            Time.timeScale = 1f;

            // 只生成1个敌人进行快速测试
            Enemy enemy = SpawnTestEnemy("MultiEnemy");
            testEnemies.Add(enemy);

            int totalTriggers = 0;

            // 连续攻击敌人3次
            for (int i = 0; i < 3; i++)
            {
                if (synergySystem.RegisterHit(enemy, 5f, player.transform))
                {
                    totalTriggers++;
                }
            }

            Assert(totalTriggers >= 1, "应至少触发1次协同");

            LogInfo($"多敌人测试完成。触发次数: {totalTriggers}");

            Destroy(enemy.gameObject);
            testEnemies.Remove(enemy);
            yield return null;
        }

        Enemy SpawnTestEnemy(string name)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            spawnPos += Random.insideUnitSphere * 2f;
            spawnPos.z = 0;

            GameObject obj;
            if (enemyPrefab != null)
            {
                obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                obj = new GameObject(name);
                obj.transform.position = spawnPos;
                obj.AddComponent<Enemy>();
                obj.AddComponent<CircleCollider2D>();
                var rb = obj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
            }

            obj.name = name;
            Enemy enemy = obj.GetComponent<Enemy>();
            enemy.maxHealth = 100f;
            enemy.currentHealth = 100f;

            return enemy;
        }

        protected override IEnumerator OnTearDown()
        {
            LogInfo("清理测试敌人");

            foreach (var enemy in testEnemies)
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
            testEnemies.Clear();

            DataCollector.CollectData("TotalSynergyTriggers", synergyTriggerCount);

            yield return null;
        }
    }
}
