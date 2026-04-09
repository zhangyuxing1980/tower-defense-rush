// PROTOTYPE - NOT FOR PRODUCTION
// Runtime Test Loader - Dynamically loads test cases
// Date: 2026-04-08

using UnityEngine;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 在运行时动态添加测试用例组件
    /// </summary>
    public class RuntimeTestLoader : MonoBehaviour
    {
        [Header("Test Cases to Add")]
        public bool addSynergyTest = true;
        public bool addJimmyTest = true;
        public bool addCombatTest = true;
        public bool addPerformanceTest = true;
        public bool addWaveTest = true;

        void Awake()
        {
            Debug.Log("[RuntimeTestLoader] 正在动态添加测试用例...");

            // 动态添加测试用例组件
            if (addSynergyTest && GetComponent<SynergySystemTest>() == null)
            {
                gameObject.AddComponent<SynergySystemTest>();
                Debug.Log("[RuntimeTestLoader] 已添加 SynergySystemTest");
            }

            if (addJimmyTest && GetComponent<JimmyAITest>() == null)
            {
                gameObject.AddComponent<JimmyAITest>();
                Debug.Log("[RuntimeTestLoader] 已添加 JimmyAITest");
            }

            if (addCombatTest && GetComponent<CombatSystemTest>() == null)
            {
                gameObject.AddComponent<CombatSystemTest>();
                Debug.Log("[RuntimeTestLoader] 已添加 CombatSystemTest");
            }

            if (addPerformanceTest && GetComponent<PerformanceTest>() == null)
            {
                gameObject.AddComponent<PerformanceTest>();
                Debug.Log("[RuntimeTestLoader] 已添加 PerformanceTest");
            }

            if (addWaveTest && GetComponent<WaveSystemTest>() == null)
            {
                gameObject.AddComponent<WaveSystemTest>();
                Debug.Log("[RuntimeTestLoader] 已添加 WaveSystemTest");
            }

            Debug.Log("[RuntimeTestLoader] 测试用例添加完成");

            // 销毁自己，防止重复添加
            Destroy(this);
        }
    }
}
