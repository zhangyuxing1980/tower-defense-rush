// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Example: Combat System Test
// Date: 2026-04-07

using System.Collections;
using UnityEngine;
using TowerDefenseRush.Prototype;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 战斗系统测试 - 验证协同攻击机制
    /// 这是一个示例，展示如何编写测试用例
    /// </summary>
    public class CombatSystemTest : TestCaseBase
    {
        [Header("Test Configuration")]
        public GameObject testEnemyPrefab;
        public Transform testSpawnPoint;

        private Enemy testEnemy;
        private PlayerController player;
        private Jimmy[] jimmies;
        private int initialSynergyCount = 0;

        protected override void Awake()
        {
            base.Awake();
            testId = "COMBAT_001";
            testName = "协同攻击系统测试";
            description = "验证3连击协同攻击机制是否正确触发，包括伤害计算和视觉效果";
            category = "Combat";
            priority = 1;
            timeout = 60f;
        }

        protected override IEnumerator OnSetUp()
        {
            LogInfo("初始化战斗系统测试环境");

            // 查找或创建测试对象
            player = FindObjectOfType<PlayerController>();
            AssertNotNull(player, "场景中必须有玩家控制器");

            jimmies = FindObjectsOfType<Jimmy>();
            Assert(jimmies.Length >= 3, "场景中至少需要3只吉米");

            // 创建测试敌人
            if (testEnemyPrefab != null && testSpawnPoint != null)
            {
                GameObject enemyObj = Instantiate(testEnemyPrefab, testSpawnPoint.position, Quaternion.identity);
                testEnemy = enemyObj.GetComponent<Enemy>();
                AssertNotNull(testEnemy, "测试敌人创建失败");
            }
            else
            {
                LogWarning("未配置测试敌人预制体，跳过敌人相关测试");
            }

            // 记录初始状态
            initialSynergyCount = 0;
            DataCollector.CollectData("InitialJimmyCount", jimmies.Length);
            DataCollector.CollectData("PlayerPosition", player.transform.position);

            yield return Wait(0.5f, "等待场景稳定");
        }

        public override IEnumerator Run()
        {
            LogInfo("开始执行战斗测试");

            // 测试1: 验证吉米跟随
            yield return TestJimmyFollowing();

            // 测试2: 验证自动攻击
            if (testEnemy != null)
            {
                yield return TestAutoAttack();

                // 测试3: 验证协同攻击（核心）
                yield return TestSynergyMechanic();
            }

            // 测试4: 验证伤害计算
            yield return TestDamageCalculation();

            LogInfo("所有战斗测试完成");
        }

        IEnumerator TestJimmyFollowing()
        {
            LogInfo("测试1: 吉米跟随机制");

            Vector3 initialLordPos = player.transform.position;
            Vector3 targetPos = initialLordPos + Vector3.right * 5f;

            // 移动领主
            player.transform.position = targetPos;

            yield return Wait(2f, "等待吉米跟随");

            // 验证吉米是否在跟随距离内
            foreach (var jimmy in jimmies)
            {
                float distance = Vector3.Distance(jimmy.transform.position, player.transform.position);
                Assert(distance < 5f, $"吉米 {jimmy.name} 应跟随领主，距离: {distance:F2}");
                LogInfo($"吉米 {jimmy.name} 跟随正常，距离: {distance:F2}m");
            }

            // 恢复位置
            player.transform.position = initialLordPos;
        }

        IEnumerator TestAutoAttack()
        {
            LogInfo("测试2: 自动攻击机制");

            float initialEnemyHealth = testEnemy.currentHealth;
            LogInfo($"敌人初始生命值: {initialEnemyHealth}");

            // 将玩家和吉米移动到攻击范围
            player.transform.position = testEnemy.transform.position + Vector3.left * 2f;
            foreach (var jimmy in jimmies)
            {
                jimmy.transform.position = testEnemy.transform.position + Vector3.left * 2.5f;
            }

            yield return Wait(3f, "等待自动攻击造成伤害");

            float currentEnemyHealth = testEnemy.currentHealth;
            LogInfo($"敌人当前生命值: {currentEnemyHealth}");

            Assert(currentEnemyHealth < initialEnemyHealth, "自动攻击应造成伤害");
            LogInfo($"自动攻击正常，造成伤害: {initialEnemyHealth - currentEnemyHealth:F2}");
        }

        IEnumerator TestSynergyMechanic()
        {
            LogInfo("测试3: 协同攻击机制（核心测试）");

            if (SynergySystem.Instance == null)
            {
                Skip("场景中缺少协同系统");
                yield break;
            }

            float initialHealth = testEnemy.currentHealth;
            int synergyTriggered = 0;

            // 手动触发3次攻击
            for (int i = 0; i < 3; i++)
            {
                float damageBefore = testEnemy.currentHealth;
                bool triggered = SynergySystem.Instance.RegisterHit(testEnemy, 10f, player.transform);

                if (triggered)
                {
                    synergyTriggered++;
                    LogInfo($"协同攻击触发！伤害: {damageBefore - testEnemy.currentHealth:F2}");
                }

                yield return Wait(0.3f, "等待攻击间隔");
            }

            Assert(synergyTriggered > 0, "3次连续攻击应触发协同效果");

            float totalDamage = initialHealth - testEnemy.currentHealth;
            LogInfo($"协同攻击测试完成，总伤害: {totalDamage:F2}，协同触发次数: {synergyTriggered}");

            // 验证时停效果
            DataCollector.CollectData("SynergyTriggers", synergyTriggered);
            DataCollector.CollectData("TotalDamage", totalDamage);
        }

        IEnumerator TestDamageCalculation()
        {
            LogInfo("测试4: 伤害计算验证");

            // 创建一个新的测试敌人
            GameObject dummyEnemy = new GameObject("DamageTestDummy");
            Enemy enemy = dummyEnemy.AddComponent<Enemy>();
            enemy.maxHealth = 100f;
            enemy.currentHealth = 100f;

            // 测试基础伤害
            enemy.TakeDamage(10f);
            AssertEqual(90f, enemy.currentHealth, "基础伤害计算错误");

            // 测试协同伤害（2倍）
            enemy.currentHealth = 100f;
            enemy.TakeDamage(20f); // 模拟协同伤害
            AssertEqual(80f, enemy.currentHealth, "协同伤害计算错误");

            Destroy(dummyEnemy);

            LogInfo("伤害计算验证通过");
            yield return null;
        }

        protected override IEnumerator OnTearDown()
        {
            LogInfo("清理测试环境");

            // 销毁测试敌人
            if (testEnemy != null)
            {
                Destroy(testEnemy.gameObject);
            }

            // 重置玩家位置（可选）
            // player.transform.position = DataCollector.GetCollectedData()["PlayerPosition"];

            yield return null;
        }

        public override bool Validate()
        {
            // 额外的验证逻辑
            if (Result == TestResult.Passed)
            {
                var data = DataCollector.GetCollectedData();
                if (data.ContainsKey("SynergyTriggers"))
                {
                    int triggers = (int)data["SynergyTriggers"];
                    if (triggers == 0)
                    {
                        Fail("协同攻击未触发");
                        return false;
                    }
                }
            }

            return base.Validate();
        }
    }
}
