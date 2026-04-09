// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Test Case Base Class
// Date: 2026-04-07

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseRush.Testing
{
    public abstract class TestCaseBase : MonoBehaviour, ITestCase
    {
        [Header("Test Info")]
        [SerializeField] protected string testId;
        [SerializeField] protected string testName;
        [TextArea(2, 4)]
        [SerializeField] protected string description;
        [SerializeField] protected string category = "General";
        [SerializeField] protected int priority = 3;
        [SerializeField] protected float timeout = 30f;

        public string TestId => testId;
        public string TestName => testName;
        public string Description => description;
        public string Category => category;
        public int Priority => priority;
        public float Timeout => timeout;

        public TestResult Result { get; protected set; } = TestResult.NotRun;
        public string ErrorMessage { get; protected set; } = "";
        public List<TestLogEntry> Logs { get; protected set; } = new List<TestLogEntry>();
        public float ExecutionTime { get; protected set; }

        protected float startTime;
        protected bool isRunning = false;
        protected MCPDataCollector DataCollector { get; private set; }

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(testId))
            {
                testId = GetType().Name + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            }
            DataCollector = new MCPDataCollector(this);
        }

        public virtual IEnumerator SetUp()
        {
            LogInfo($"[{TestId}] 设置测试环境...");
            Result = TestResult.Running;
            ErrorMessage = "";
            Logs.Clear();
            startTime = Time.time;
            isRunning = true;
            DataCollector.StartCollection();
            yield return OnSetUp();
        }

        protected virtual IEnumerator OnSetUp()
        {
            yield return null;
        }

        public abstract IEnumerator Run();

        public virtual IEnumerator TearDown()
        {
            LogInfo($"[{TestId}] 清理测试环境...");
            yield return OnTearDown();
            ExecutionTime = Time.time - startTime;
            isRunning = false;
            DataCollector.StopCollection();
            if (Result == TestResult.Running)
            {
                Validate();
            }
        }

        protected virtual IEnumerator OnTearDown()
        {
            yield return null;
        }

        public virtual bool Validate()
        {
            if (Result == TestResult.Running)
            {
                Result = TestResult.Passed;
                LogInfo($"[{TestId}] 测试通过 (执行时间: {ExecutionTime:F2}s)");
            }
            return Result == TestResult.Passed;
        }

        protected void LogDebug(string message, string context = "")
        {
            AddLog(TestLogLevel.Debug, message, context);
        }

        protected void LogInfo(string message, string context = "")
        {
            AddLog(TestLogLevel.Info, message, context);
        }

        protected void LogWarning(string message, string context = "")
        {
            AddLog(TestLogLevel.Warning, message, context);
        }

        protected void LogError(string message, string context = "")
        {
            AddLog(TestLogLevel.Error, message, context);
        }

        protected void LogCritical(string message, string context = "")
        {
            AddLog(TestLogLevel.Critical, message, context);
        }

        private void AddLog(TestLogLevel level, string message, string context)
        {
            var entry = new TestLogEntry(level, message, context);
            Logs.Add(entry);
            string logMessage = $"[Test] [{TestId}] [{level}] {message}";
            if (!string.IsNullOrEmpty(context))
            {
                logMessage += $" | Context: {context}";
            }

            switch (level)
            {
                case TestLogLevel.Debug:
                case TestLogLevel.Info:
                    Debug.Log(logMessage);
                    break;
                case TestLogLevel.Warning:
                    Debug.LogWarning(logMessage);
                    break;
                case TestLogLevel.Error:
                case TestLogLevel.Critical:
                    Debug.LogError(logMessage);
                    break;
            }
        }

        protected void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Result = TestResult.Failed;
                ErrorMessage = message;
                LogError($"断言失败: {message}");
                throw new TestAssertionException(message);
            }
            else
            {
                LogDebug($"断言通过: {message}");
            }
        }

        protected void AssertEqual<T>(T expected, T actual, string message = "")
        {
            bool equal = EqualityComparer<T>.Default.Equals(expected, actual);
            string fullMessage = string.IsNullOrEmpty(message)
                ? $"期望值: {expected}, 实际值: {actual}"
                : $"{message} | 期望值: {expected}, 实际值: {actual}";
            Assert(equal, fullMessage);
        }

        protected void AssertNotNull(object obj, string message = "")
        {
            string fullMessage = string.IsNullOrEmpty(message) ? "对象不应为空" : message;
            Assert(obj != null, fullMessage);
        }

        protected void AssertApproximately(float expected, float actual, float tolerance = 0.01f, string message = "")
        {
            bool close = Mathf.Abs(expected - actual) <= tolerance;
            string fullMessage = string.IsNullOrEmpty(message)
                ? $"期望值: {expected}, 实际值: {actual}, 容差: {tolerance}"
                : $"{message} | 期望值: {expected}, 实际值: {actual}, 容差: {tolerance}";
            Assert(close, fullMessage);
        }

        public void Fail(string message)
        {
            Result = TestResult.Failed;
            ErrorMessage = message;
            LogError($"测试失败: {message}");
        }

        protected void Skip(string reason)
        {
            Result = TestResult.Skipped;
            ErrorMessage = reason;
            LogWarning($"测试跳过: {reason}");
        }

        protected IEnumerator Wait(float seconds, string reason = "")
        {
            if (!string.IsNullOrEmpty(reason))
            {
                LogDebug($"等待 {seconds:F2}s: {reason}");
            }
            yield return new WaitForSeconds(seconds);
        }

        protected IEnumerator WaitUntil(System.Func<bool> condition, float maxWait = 10f, string description = "")
        {
            float startWait = Time.time;
            if (!string.IsNullOrEmpty(description))
            {
                LogDebug($"等待条件: {description}");
            }

            while (!condition())
            {
                if (Time.time - startWait > maxWait)
                {
                    Fail($"等待超时 ({maxWait}s): {description}");
                    yield break;
                }
                yield return null;
            }
            LogDebug($"条件满足: {description}");
        }
    }

    public class MCPDataCollector
    {
        private TestCaseBase testCase;
        private Dictionary<string, object> collectedData = new Dictionary<string, object>();
        private bool isCollecting = false;

        public MCPDataCollector(TestCaseBase test)
        {
            testCase = test;
        }

        public void StartCollection()
        {
            isCollecting = true;
            collectedData.Clear();
            CollectSnapshot("Start");
        }

        public void StopCollection()
        {
            if (!isCollecting) return;
            CollectSnapshot("End");
            isCollecting = false;
        }

        public void CollectSnapshot(string label)
        {
            if (!isCollecting) return;
            var snapshot = new Dictionary<string, object>
            {
                ["Time"] = Time.time,
                ["FrameCount"] = Time.frameCount,
                ["Memory"] = GC.GetTotalMemory(false) / 1024 / 1024,
            };
            collectedData[$"Snapshot_{label}"] = snapshot;
        }

        public void CollectData(string key, object value)
        {
            if (!isCollecting) return;
            collectedData[key] = value;
        }

        public Dictionary<string, object> GetCollectedData()
        {
            return new Dictionary<string, object>(collectedData);
        }
    }
}
