// PROTOTYPE - NOT FOR PRODUCTION
// Game Over UI - Victory and Defeat screens
// Date: 2026-04-09

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefenseRush.Prototype
{
    /// <summary>
    /// 游戏结束界面 - 显示胜利/失败，提供重新开始
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject victoryPanel;
        public GameObject defeatPanel;
        public GameObject hudPanel;

        [Header("Victory UI")]
        public TextMeshProUGUI victoryTitle;
        public TextMeshProUGUI victoryStats;
        public Button victoryRestartButton;

        [Header("Defeat UI")]
        public TextMeshProUGUI defeatTitle;
        public TextMeshProUGUI defeatStats;
        public Button defeatRestartButton;

        [Header("HUD UI")]
        public Slider playerHealthSlider;
        public Slider townHealthSlider;
        public TextMeshProUGUI waveText;

        private void Start()
        {
            // 订阅游戏事件
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnVictory += ShowVictory;
                GameManager.Instance.OnGameOver += ShowDefeat;
                GameManager.Instance.OnTownDamaged += UpdateTownHealth;
            }

            // 查找玩家
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.OnDeath += () => ShowDefeat();
            }

            // 设置按钮
            if (victoryRestartButton != null)
                victoryRestartButton.onClick.AddListener(RestartGame);
            if (defeatRestartButton != null)
                defeatRestartButton.onClick.AddListener(RestartGame);

            // 初始化面板
            HideAllPanels();
            if (hudPanel != null)
                hudPanel.SetActive(true);

            // 初始化血量
            UpdateHealthDisplays();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnVictory -= ShowVictory;
                GameManager.Instance.OnGameOver -= ShowDefeat;
                GameManager.Instance.OnTownDamaged -= UpdateTownHealth;
            }
        }

        private void Update()
        {
            // 更新HUD
            UpdateHealthDisplays();
            UpdateWaveDisplay();
        }

        void HideAllPanels()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        public void ShowVictory()
        {
            HideAllPanels();
            if (hudPanel != null) hudPanel.SetActive(false);
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);

                // 统计信息
                string stats = GetStatsText();
                if (victoryStats != null)
                    victoryStats.text = stats;
            }

            Debug.Log("🎉 显示胜利界面");
        }

        public void ShowDefeat()
        {
            HideAllPanels();
            if (hudPanel != null) hudPanel.SetActive(false);
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);

                // 统计信息
                string stats = GetStatsText();
                if (defeatStats != null)
                    defeatStats.text = stats;
            }

            Debug.Log("💀 显示失败界面");
        }

        void UpdateHealthDisplays()
        {
            // 玩家血量
            var player = FindObjectOfType<PlayerController>();
            if (player != null && playerHealthSlider != null)
            {
                playerHealthSlider.maxValue = player.maxHealth;
                playerHealthSlider.value = player.currentHealth;
            }

            // 城镇血量
            if (GameManager.Instance != null && townHealthSlider != null)
            {
                townHealthSlider.maxValue = GameManager.Instance.maxTownHealth;
                townHealthSlider.value = GameManager.Instance.townHealth;
            }
        }

        void UpdateTownHealth(float health)
        {
            if (townHealthSlider != null)
                townHealthSlider.value = health;
        }

        void UpdateWaveDisplay()
        {
            if (waveText != null && WaveManager.Instance != null)
            {
                int current = WaveManager.Instance.currentWaveIndex + 1;
                int total = WaveManager.Instance.waves.Length;
                waveText.text = $"波次: {current}/{total}";
            }
        }

        string GetStatsText()
        {
            string stats = "";

            if (WaveManager.Instance != null)
            {
                stats += $"完成波次: {WaveManager.Instance.currentWaveIndex + 1}\n";
            }

            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                stats += $"剩余血量: {player.currentHealth:F0}/{player.maxHealth:F0}\n";
            }

            if (GameManager.Instance != null)
            {
                stats += $"城镇血量: {GameManager.Instance.townHealth:F0}/{GameManager.Instance.maxTownHealth:F0}\n";
            }

            return stats;
        }

        void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
    }
}
