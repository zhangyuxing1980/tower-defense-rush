// PROTOTYPE - NOT FOR PRODUCTION
// Game Manager - Central game state management
// Date: 2026-04-07

using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    public enum GameState
    {
        Playing,
        Paused,
        Victory,
        GameOver
    }

    /// <summary>
    /// 游戏管理器 - 管理游戏状态和全局逻辑
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Game State")]
        public GameState currentState = GameState.Playing;
        public float townHealth = 100f;
        public float maxTownHealth = 100f;

        [Header("Events")]
        public System.Action OnGameOver;
        public System.Action OnVictory;
        public System.Action<float> OnTownDamaged;

        public bool IsPlaying => currentState == GameState.Playing;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // 订阅波次完成事件
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnAllWavesComplete += HandleVictory;
            }
        }

        void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnAllWavesComplete -= HandleVictory;
            }
        }

        public void DamageTown(float damage)
        {
            if (currentState != GameState.Playing) return;

            townHealth -= damage;
            OnTownDamaged?.Invoke(townHealth);

            if (townHealth <= 0)
            {
                HandleGameOver();
            }
        }

        void HandleVictory()
        {
            currentState = GameState.Victory;
            OnVictory?.Invoke();
            Debug.Log("🎉 胜利！所有波次完成！");
        }

        void HandleGameOver()
        {
            currentState = GameState.GameOver;
            OnGameOver?.Invoke();
            Debug.Log("💀 游戏结束！城镇被摧毁！");
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        void Update()
        {
            // 快捷键 - 注释掉以适配Input System
            // if (Input.GetKeyDown(KeyCode.Escape))
            // {
            //     TogglePause();
            // }
            //
            // if (Input.GetKeyDown(KeyCode.R) && currentState != GameState.Playing)
            // {
            //     RestartGame();
            // }
        }

        void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0f;
            }
            else if (currentState == GameState.Paused)
            {
                currentState = GameState.Playing;
                Time.timeScale = 1f;
            }
        }
    }
}
// Force compile 1775708111
// Force compile 1775709118
