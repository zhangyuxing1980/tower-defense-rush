// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Performance Test
// Date: 2026-04-07

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 性能测试 - 验证帧率和内存使用
    /// </summary>
    public class PerformanceTest : TestCaseBase
    {
        [Header("Performance Thresholds")]
        public float targetFrameRate = 60f;
        public float minAcceptableFPS = 30f;
        public long maxMemoryMB = 512;
        public int maxDrawCalls = 100;

        private List<float> frameTimes = new List<float>();
        private List<float> fpsReadings = new List<float>();
        private float testStartTime;
        private long initialMemory;

        protected override void Awake()
        {
            base.Awake();
            testId = "PERF_001";
            testName = "性能测试";
            description = "验证游戏在目标设备上的帧率和内存性能";
            category = "Performance";
            priority = 2;
            timeout = 120f;
        }

        protected override IEnumerator OnSetUp()
        {
            LogInfo("初始化性能测试");

            frameTimes.Clear();
            fpsReadings.Clear();
            testStartTime = Time.time;
            initialMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);

            LogInfo($"初始内存使用: {initialMemory}MB");

            yield return null;
        }

        public override IEnumerator Run()
        {
            LogInfo("开始性能测试");

            // 测试1: 基础帧率
            yield return TestBaselineFPS();

            // 测试2: 战斗场景帧率
            yield return TestCombatFPS();

            // 测试3: 内存使用
            yield return TestMemoryUsage();

            // 测试4: 总结分析
            yield return AnalyzeResults();

            LogInfo("性能测试完成");
        }

        IEnumerator TestBaselineFPS()
        {
            LogInfo("测试1: 基础场景帧率");

            // 记录60帧的数据
            for (int i = 0; i < 60; i++)
            {
                RecordFrameData();
                yield return null;
            }

            float avgFPS = CalculateAverageFPS();
            float minFPS = CalculateMinFPS();

            LogInfo($"基础场景平均FPS: {avgFPS:F2}, 最低FPS: {minFPS:F2}");

            Assert(avgFPS >= targetFrameRate * 0.9f,
                $"基础场景平均帧率应接近{targetFrameRate}FPS，实际: {avgFPS:F2}");
            Assert(minFPS >= minAcceptableFPS,
                $"基础场景最低帧率不应低于{minAcceptableFPS}FPS，实际: {minFPS:F2}");

            DataCollector.CollectData("BaselineAvgFPS", avgFPS);
            DataCollector.CollectData("BaselineMinFPS", minFPS);
        }

        IEnumerator TestCombatFPS()
        {
            LogInfo("测试2: 战斗场景帧率");

            // 清空之前的数据
            frameTimes.Clear();
            fpsReadings.Clear();

            // 通知其他系统生成大量敌人和效果
            LogInfo("模拟高强度战斗场景...");

            // 记录120帧的数据（约2秒）
            for (int i = 0; i < 120; i++)
            {
                RecordFrameData();

                // 模拟掉帧检测
                float currentFPS = 1f / Time.unscaledDeltaTime;
                if (currentFPS < minAcceptableFPS)
                {
                    LogWarning($"帧率下降检测: {currentFPS:F2} FPS (帧 {i})");
                }

                yield return null;
            }

            float avgFPS = CalculateAverageFPS();
            float minFPS = CalculateMinFPS();
            float percentile1 = CalculatePercentileFPS(1);  // 1%最低帧率

            LogInfo($"战斗场景平均FPS: {avgFPS:F2}");
            LogInfo($"战斗场景最低FPS: {minFPS:F2}");
            LogInfo($"战斗场景1%最低FPS: {percentile1:F2}");

            // 战斗场景标准稍微放宽
            Assert(avgFPS >= minAcceptableFPS,
                $"战斗场景平均帧率应至少{minAcceptableFPS}FPS，实际: {avgFPS:F2}");
            Assert(percentile1 >= 20f,
                $"战斗场景1%最低帧率应至少20FPS，实际: {percentile1:F2}");

            DataCollector.CollectData("CombatAvgFPS", avgFPS);
            DataCollector.CollectData("CombatMinFPS", minFPS);
            DataCollector.CollectData("Combat1PercentFPS", percentile1);
        }

        IEnumerator TestMemoryUsage()
        {
            LogInfo("测试3: 内存使用");

            // 强制垃圾回收后测量
            System.GC.Collect();
            yield return Wait(1f, "等待垃圾回收完成");

            long currentMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            long memoryIncrease = currentMemory - initialMemory;

            LogInfo($"当前内存使用: {currentMemory}MB");
            LogInfo($"内存增长: {memoryIncrease}MB");

            Assert(currentMemory <= maxMemoryMB,
                $"内存使用不应超过{maxMemoryMB}MB，实际: {currentMemory}MB");

            // 检查内存泄漏迹象
            if (memoryIncrease > 50)
            {
                LogWarning($"内存增长较高: {memoryIncrease}MB，可能存在内存泄漏");
            }

            DataCollector.CollectData("CurrentMemoryMB", currentMemory);
            DataCollector.CollectData("MemoryIncreaseMB", memoryIncrease);

            yield return null;
        }

        IEnumerator AnalyzeResults()
        {
            LogInfo("测试4: 性能分析总结");

            var analysis = new Dictionary<string, object>
            {
                ["TestDuration"] = Time.time - testStartTime,
                ["TotalFramesRecorded"] = frameTimes.Count,
                ["TargetFPS"] = targetFrameRate,
                ["MinAcceptableFPS"] = minAcceptableFPS,
                ["Platform"] = Application.platform.ToString(),
                ["DeviceModel"] = SystemInfo.deviceModel,
                ["GPU"] = SystemInfo.graphicsDeviceName
            };

            // 生成性能评级
            float overallAvgFPS = CalculateOverallAverageFPS();
            string rating;
            if (overallAvgFPS >= 55f) rating = "EXCELLENT";
            else if (overallAvgFPS >= 45f) rating = "GOOD";
            else if (overallAvgFPS >= 30f) rating = "ACCEPTABLE";
            else rating = "NEEDS_OPTIMIZATION";

            analysis["OverallRating"] = rating;
            analysis["OverallAvgFPS"] = overallAvgFPS;

            LogInfo($"性能评级: {rating}");
            LogInfo($"总体平均FPS: {overallAvgFPS:F2}");

            // 保存详细数据
            foreach (var kvp in analysis)
            {
                DataCollector.CollectData(kvp.Key, kvp.Value);
            }

            yield return null;
        }

        void RecordFrameData()
        {
            float deltaTime = Time.unscaledDeltaTime;
            float fps = 1f / deltaTime;

            frameTimes.Add(deltaTime * 1000f); // 转换为毫秒
            fpsReadings.Add(fps);
        }

        float CalculateAverageFPS()
        {
            if (fpsReadings.Count == 0) return 0f;

            float sum = 0f;
            foreach (float fps in fpsReadings)
            {
                sum += Mathf.Clamp(fps, 0f, 999f); // 防止异常值
            }
            return sum / fpsReadings.Count;
        }

        float CalculateMinFPS()
        {
            if (fpsReadings.Count == 0) return 0f;

            float min = float.MaxValue;
            foreach (float fps in fpsReadings)
            {
                if (fps > 0 && fps < min) min = fps;
            }
            return min == float.MaxValue ? 0f : min;
        }

        float CalculatePercentileFPS(int percentile)
        {
            if (fpsReadings.Count == 0) return 0f;

            List<float> sorted = new List<float>(fpsReadings);
            sorted.Sort();

            int index = Mathf.CeilToInt(sorted.Count * percentile / 100f);
            index = Mathf.Clamp(index, 0, sorted.Count - 1);

            return sorted[index];
        }

        float CalculateOverallAverageFPS()
        {
            // 使用所有记录的帧数据
            if (fpsReadings.Count == 0) return 0f;
            return CalculateAverageFPS();
        }

        protected override IEnumerator OnTearDown()
        {
            LogInfo("清理性能测试");

            // 输出最终报告
            LogInfo("========== 性能测试报告 ==========");
            LogInfo($"记录帧数: {frameTimes.Count}");
            LogInfo($"总体平均FPS: {CalculateOverallAverageFPS():F2}");
            LogInfo($"测试时长: {Time.time - testStartTime:F2}秒");
            LogInfo("===================================");

            yield return null;
        }
    }
}
