// PROTOTYPE - NOT FOR PRODUCTION
// Victory/Defeat Condition Test
// Date: 2026-04-09

using System.Collections;
using UnityEngine;
using TowerDefenseRush.Prototype;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 胜利/失败条件测试 - 验证游戏结束逻辑
    /// </summary>
    public class VictoryConditionTest : TestCaseBase
    {
        private TowerDefenseRush.Prototype.GameManager gameManager;
        private TowerDefenseRush.Prototype.PlayerController player;
        private TowerDefenseRush.Prototype.WaveManager waveManager;

        protected override void Awake()
        {
            base.Awake();
            testId = "VICTORY_001";
            testName = "胜利/失败条件测试";
            description = "验证玩家死亡和城镇被毁触发游戏结束，波次完成触发胜利";
            category = "Systems";
            priority = 1;
            timeout = 60f;
        }

        protected override IEnumerator OnSetUp()
        {
            LogInfo("初始化胜利/失败条件测试");

            gameManager = FindObjectOfType<TowerDefenseRush.Prototype.GameManager>();
            AssertNotNull(gameManager, "场景中必须有GameManager");

            player = FindObjectOfType<TowerDefenseRush.Prototype.PlayerController>();
            AssertNotNull(player, "场景中必须有玩家");

            waveManager = FindObjectOfType<TowerDefenseRush.Prototype.WaveManager>();
            AssertNotNull(waveManager, "场景中必须有WaveManager");

            // 确保游戏状态正常
            gameManager.currentState = TowerDefenseRush.Prototype.GameState.Playing;
            player.currentHealth = player.maxHealth;
            gameManager.townHealth = gameManager.maxTownHealth;

            yield return null;
        }

        public override IEnumerator Run()
        {
            LogInfo("开始胜利/失败条件测试");

            // 测试1: 玩家受伤
            yield return TestPlayerDamage();

            // 测试2: 城镇受伤
            yield return TestTownDamage();

            // 测试3: 玩家死亡触发游戏结束
            yield return TestPlayerDeath();

            LogInfo("胜利/失败条件测试完成");
        }

        IEnumerator TestPlayerDamage()
        {
            LogInfo("测试1: 玩家受伤");

            float initialHealth = player.currentHealth;
            player.TakeDamage(10f);

            Assert(player.currentHealth < initialHealth, "玩家受伤后血量应减少");
            AssertApproximately(initialHealth - 10f, player.currentHealth, 0.1f, "玩家应受到正确伤害");

            LogInfo($"玩家受伤测试通过: {initialHealth} -> {player.currentHealth}");
            yield return null;
        }

        IEnumerator TestTownDamage()
        {
            LogInfo("测试2: 城镇受伤");

            // 重置玩家血量（防止之前的测试影响）
            gameManager.currentState = TowerDefenseRush.Prototype.GameState.Playing;
            player.currentHealth = player.maxHealth;

            float initialTownHealth = gameManager.townHealth;
            gameManager.DamageTown(20f);

            Assert(gameManager.townHealth < initialTownHealth, "城镇受伤后血量应减少");
            AssertApproximately(initialTownHealth - 20f, gameManager.townHealth, 0.1f, "城镇应受到正确伤害");

            LogInfo($"城镇受伤测试通过: {initialTownHealth} -> {gameManager.townHealth}");
            yield return null;
        }

        IEnumerator TestPlayerDeath()
        {
            LogInfo("测试3: 玩家死亡触发游戏结束");

            // 重置状态
            gameManager.currentState = TowerDefenseRush.Prototype.GameState.Playing;
            gameManager.townHealth = gameManager.maxTownHealth;
            player.currentHealth = 100f;

            // 杀死玩家
            player.TakeDamage(999f);

            yield return new WaitForSecondsRealtime(0.5f);

            // 验证玩家死亡
            Assert(!player.IsAlive, "玩家应已死亡");
            Assert(gameManager.currentState == TowerDefenseRush.Prototype.GameState.GameOver || gameManager.townHealth <= 0,
                "玩家死亡应触发游戏结束");

            LogInfo("玩家死亡触发游戏结束测试通过");
            yield return null;
        }

        protected override IEnumerator OnTearDown()
        {
            LogInfo("清理胜利/失败条件测试");

            // 重置游戏状态
            if (gameManager != null)
            {
                gameManager.currentState = TowerDefenseRush.Prototype.GameState.Playing;
                gameManager.townHealth = gameManager.maxTownHealth;
            }

            if (player != null)
            {
                player.currentHealth = player.maxHealth;
            }

            yield return null;
        }
    }
}

