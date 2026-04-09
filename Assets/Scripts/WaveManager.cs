// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does squad-based combat with synergy mechanics feel satisfying?
// Date: 2026-04-07

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    [System.Serializable]
    public class WaveConfig
    {
        public int enemyCount = 5;
        public float spawnInterval = 1.5f;
        public float timeBetweenWaves = 5f;
        public GameObject enemyPrefab;
    }

    /// <summary>
    /// 波次管理器 - 控制敌人波次生成
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        public WaveConfig[] waves;
        public Transform[] spawnPoints;
        public Transform townCenter;

        [Header("Events")]
        public Action<int> OnWaveStart;
        public Action OnWaveComplete;
        public Action OnAllWavesComplete;

        public static WaveManager Instance;

        public int currentWaveIndex { get; private set; } = 0;
        public bool isWaveActive { get; private set; } = false;
        public int enemiesAlive { get; private set; } = 0;
        public int totalEnemiesSpawned { get; private set; } = 0;

        private List<Enemy> activeEnemies = new List<Enemy>();

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            StartCoroutine(RunWaves());
        }

        IEnumerator RunWaves()
        {
            while (currentWaveIndex < waves.Length)
            {
                yield return StartCoroutine(SpawnWave(currentWaveIndex));
                currentWaveIndex++;

                // 等待波次完成
                yield return new WaitUntil(() => enemiesAlive == 0);

                OnWaveComplete?.Invoke();

                // 等待下一波
                if (currentWaveIndex < waves.Length)
                {
                    yield return new WaitForSeconds(waves[currentWaveIndex].timeBetweenWaves);
                }
            }

            OnAllWavesComplete?.Invoke();
            Debug.Log("所有波次完成！");
        }

        IEnumerator SpawnWave(int waveIndex)
        {
            isWaveActive = true;
            OnWaveStart?.Invoke(waveIndex + 1);

            var wave = waves[waveIndex];
            Debug.Log($"开始波次 {waveIndex + 1}，敌人数量: {wave.enemyCount}");

            for (int i = 0; i < wave.enemyCount; i++)
            {
                SpawnEnemy(wave);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            isWaveActive = false;
        }

        void SpawnEnemy(WaveConfig wave)
        {
            if (wave.enemyPrefab == null || spawnPoints.Length == 0) return;

            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            GameObject enemyObj = Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.townCenter = townCenter;
                activeEnemies.Add(enemy);
                enemiesAlive++;
                totalEnemiesSpawned++;
            }
        }

        public void OnEnemyDeath(Enemy enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                enemiesAlive--;
            }
        }
    }
}
