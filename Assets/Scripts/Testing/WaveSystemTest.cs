// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Example: Wave System Test
// Date: 2026-04-07

using System.Collections;
using UnityEngine;
using TowerDefenseRush.Prototype;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 波次系统测试 - 验证波次生成和节奏
    /// </summary>
    public class WaveSystemTest : TestCaseBase
    {
        private WaveManager waveManager;

        protected override void Awake()
        {
            base.Awake();
            testId = "WAVE_001";
            testName = "波次系统测试";
            description = "验证波次生成、敌人生成节奏、波次完成判定";
            category = "Wave";
            priority = 2;
            timeout = 120f;
        }

        protected override IEnumerator OnSetUp()
        {
            waveManager = FindObjectOfType<WaveManager>();
            AssertNotNull(waveManager, "场景中必须有波次管理器");

            // 订阅事件
            if (waveManager != null)
            {
                waveManager.OnWaveStart += OnWaveStart;
                waveManager.OnWaveComplete += OnWaveComplete;
            }

            yield return null;
        }

        public override IEnumerator Run()
        {
            LogInfo("开始波次系统测试");

            // 测试1: 验证波次配置
            yield return TestWaveConfiguration();

            // 测试2: 验证敌人生成
            yield return TestEnemySpawning();

            // 测试3: 验证波次完成流程
            yield return TestWaveCompletion();

            LogInfo("波次系统测试完成");
        }

        IEnumerator TestWaveConfiguration()
        {
            LogInfo("测试1: 波次配置验证");

            AssertNotNull(waveManager.waves, "波次配置不应为空");
            Assert(waveManager.waves.Length > 0, "至少需要一个波次配置");

            for (int i = 0; i < waveManager.waves.Length; i++)
            {
                var wave = waveManager.waves[i];
                Assert(wave.enemyCount > 0, $"波次{i+1}敌人数量应大于0");
                Assert(wave.spawnInterval > 0, $"波次{i+1}生成间隔应大于0");

                LogInfo($"波次{i+1}配置: {wave.enemyCount}敌人, 间隔{wave.spawnInterval}s");
            }

            DataCollector.CollectData("WaveCount", waveManager.waves.Length);
            yield return null;
        }

        IEnumerator TestEnemySpawning()
        {
            LogInfo("测试2: 敌人生成验证");

            int initialWave = waveManager.currentWaveIndex;
            int enemiesBefore = waveManager.enemiesAlive;

            // 等待第一波开始
            yield return WaitUntil(() => waveManager.isWaveActive, 5f, "等待波次激活");

            // 等待敌人生成
            yield return Wait(2f, "等待敌人生成");

            int enemiesAfter = waveManager.enemiesAlive;
            Assert(enemiesAfter > enemiesBefore, "应生成敌人");

            LogInfo($"敌人生成正常: {enemiesAfter}个活跃敌人");
            DataCollector.CollectData("EnemiesSpawned", enemiesAfter);
        }

        IEnumerator TestWaveCompletion()
        {
            LogInfo("测试3: 波次完成验证");

            int targetWave = waveManager.currentWaveIndex;
            bool waveCompleted = false;

            // 等待波次完成
            float startTime = Time.time;
            while (!waveCompleted && Time.time - startTime < 30f)
            {
                if (!waveManager.isWaveActive && waveManager.enemiesAlive == 0)
                {
                    waveCompleted = true;
                }
                yield return null;
            }

            Assert(waveCompleted, "波次应在合理时间内完成");
            LogInfo("波次完成流程正常");

            DataCollector.CollectData("WaveCompletionTime", Time.time - startTime);
        }

        void OnWaveStart(int waveNumber)
        {
            LogInfo($"波次{waveNumber}开始");
            DataCollector.CollectSnapshot($"Wave{waveNumber}Start");
        }

        void OnWaveComplete()
        {
            LogInfo("波次完成");
            DataCollector.CollectSnapshot($"Wave{waveManager.currentWaveIndex}Complete");
        }

        protected override IEnumerator OnTearDown()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveStart -= OnWaveStart;
                waveManager.OnWaveComplete -= OnWaveComplete;
            }
            yield return null;
        }
    }
}
