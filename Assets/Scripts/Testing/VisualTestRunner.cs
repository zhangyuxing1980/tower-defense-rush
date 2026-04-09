// PROTOTYPE - NOT FOR PRODUCTION
// Visual Test Runner - Displays test progress on screen
// Date: 2026-04-08

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 可视化测试运行器 - 在Game视图中显示测试进度
    /// </summary>
    public class VisualTestRunner : MonoBehaviour
    {
        [Header("Visual Settings")]
        public bool showUI = true;
        public Vector2 uiPosition = new Vector2(10, 10);
        public float uiWidth = 400f;
        public float lineHeight = 20f;

        [Header("Colors")]
        public Color bgColor = new Color(0, 0, 0, 0.8f);
        public Color textColor = Color.white;
        public Color passColor = Color.green;
        public Color failColor = Color.red;
        public Color runningColor = Color.yellow;

        private TestRunner testRunner;
        private List<string> logMessages = new List<string>();
        private Vector2 scrollPosition;
        private bool testsStarted = false;

        void Start()
        {
            testRunner = FindObjectOfType<TestRunner>();
            if (testRunner == null)
            {
                Debug.LogError("[VisualTestRunner] TestRunner not found!");
                return;
            }

            // 订阅TestRunner的事件
            StartCoroutine(MonitorTests());
            testsStarted = true;

            Debug.Log("[VisualTestRunner] 可视化测试运行器启动");
        }

        IEnumerator MonitorTests()
        {
            yield return new WaitForSeconds(0.5f);

            while (testRunner.IsRunning || !testsStarted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[VisualTestRunner] 所有测试完成!");
        }

        void OnGUI()
        {
            if (!showUI) return;

            float uiHeight = Mathf.Min(600f, Screen.height - 20);
            Rect windowRect = new Rect(uiPosition.x, uiPosition.y, uiWidth, uiHeight);

            // 背景
            GUI.color = bgColor;
            GUI.DrawTexture(windowRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(windowRect);
            GUILayout.Space(10);

            // 标题
            GUI.color = textColor;
            GUILayout.Label("=== MCP 自动化测试 ===", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
            GUILayout.Space(10);

            if (testRunner == null)
            {
                GUILayout.Label("TestRunner not found!", new GUIStyle(GUI.skin.label) { normal = new GUIStyleState { textColor = failColor } });
                GUILayout.EndArea();
                return;
            }

            // 测试状态概览
            GUILayout.Label($"状态: {(testRunner.IsRunning ? "运行中..." : "已完成")}", new GUIStyle(GUI.skin.label) { fontSize = 14 });
            GUILayout.Label($"总计: {testRunner.AllTests.Count} | 通过: {ColorText(testRunner.PassedTests.Count.ToString(), passColor)} | 失败: {ColorText(testRunner.FailedTests.Count.ToString(), failColor)} | 跳过: {testRunner.SkippedTests.Count}", new GUIStyle(GUI.skin.label) { fontSize = 12 });
            GUILayout.Space(10);

            // 测试列表
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(uiHeight - 150));

            foreach (var test in testRunner.AllTests)
            {
                DrawTestItem(test);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(10);

            // 按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("重新运行测试", GUILayout.Height(30)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        void DrawTestItem(ITestCase test)
        {
            Color statusColor = textColor;
            string statusText = "未运行";

            switch (test.Result)
            {
                case TestResult.NotRun:
                    statusColor = Color.gray;
                    statusText = "等待中";
                    break;
                case TestResult.Running:
                    statusColor = runningColor;
                    statusText = "运行中...";
                    break;
                case TestResult.Passed:
                    statusColor = passColor;
                    statusText = $"✓ 通过 ({test.ExecutionTime:F2}s)";
                    break;
                case TestResult.Failed:
                    statusColor = failColor;
                    statusText = $"✗ 失败: {test.ErrorMessage}";
                    break;
                case TestResult.Skipped:
                    statusColor = Color.cyan;
                    statusText = "跳过";
                    break;
            }

            GUILayout.BeginHorizontal();

            // 状态指示器
            GUI.color = statusColor;
            GUILayout.Label("■", GUILayout.Width(20));
            GUI.color = textColor;

            // 测试名称和状态
            GUILayout.Label($"[{test.TestId}] {test.TestName}", GUILayout.Width(250));
            GUILayout.Label(statusText, new GUIStyle(GUI.skin.label) { normal = new GUIStyleState { textColor = statusColor } });

            GUILayout.EndHorizontal();

            // 如果有错误，显示详细信息
            if (test.Result == TestResult.Failed && !string.IsNullOrEmpty(test.ErrorMessage))
            {
                GUI.color = failColor;
                GUILayout.Label($"    错误: {test.ErrorMessage}", new GUIStyle(GUI.skin.label) { fontSize = 10 });
                GUI.color = textColor;
            }

            // 显示日志
            if (test.Logs.Count > 0)
            {
                var lastLog = test.Logs[test.Logs.Count - 1];
                GUILayout.Label($"    > {lastLog.Message}", new GUIStyle(GUI.skin.label) { fontSize = 10, normal = new GUIStyleState { textColor = Color.gray } });
            }

            GUILayout.Space(5);
        }

        string ColorText(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        // 在场景视图中绘制Gizmos
        void OnDrawGizmos()
        {
            // 绘制测试区域
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(20, 15, 1));

            // 绘制标签
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(0, 8, 0), "MCP Test Area");
            #endif
        }
    }
}
