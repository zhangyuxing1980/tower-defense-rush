// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does choice gate buff system add strategic depth?
// Date: 2026-04-07

using System;
using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    public enum BuffType
    {
        AttackSpeed,
        MoveSpeed,
        Damage,
        Range,
        Heal
    }

    [System.Serializable]
    public class BuffData
    {
        public string buffName;
        public BuffType type;
        public float value; // 百分比提升 (0.2 = 20%)
        public Sprite icon;
        public string description;
    }

    /// <summary>
    /// 选择门管理器 - 波次间的Buff选择
    /// </summary>
    public class ChoiceGateManager : MonoBehaviour
    {
        [Header("Available Buffs")]
        public BuffData[] availableBuffs;

        [Header("Events")]
        public Action OnChoiceGateOpened;
        public Action OnChoiceGateClosed;
        public Action<BuffData> OnBuffSelected;

        public BuffData[] CurrentOptions { get; private set; }
        public bool IsOpen { get; private set; } = false;

        private PlayerController player;
        private Jimmy[] jimmies;

        void Start()
        {
            player = FindObjectOfType<PlayerController>();
            jimmies = FindObjectsOfType<Jimmy>();
        }

        /// <summary>
        /// 强制打开选择门（用于测试）
        /// </summary>
        public void ForceOpenChoiceGate()
        {
            CurrentOptions = GenerateOptions();
            IsOpen = true;
            OnChoiceGateOpened?.Invoke();
            Debug.Log("选择门已打开");
        }

        /// <summary>
        /// 强制关闭选择门（用于测试）
        /// </summary>
        public void ForceCloseChoiceGate()
        {
            IsOpen = false;
            OnChoiceGateClosed?.Invoke();
            Debug.Log("选择门已关闭");
        }

        BuffData[] GenerateOptions()
        {
            // 随机选择3个不同的Buff
            BuffData[] options = new BuffData[3];
            int[] indices = new int[availableBuffs.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;

            // Fisher-Yates shuffle
            for (int i = indices.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }

            for (int i = 0; i < 3; i++)
            {
                options[i] = availableBuffs[indices[i]];
            }

            return options;
        }

        /// <summary>
        /// 模拟选择Buff（用于测试）
        /// </summary>
        public void SimulateBuffSelection(BuffData buff)
        {
            ApplyBuff(buff);
            OnBuffSelected?.Invoke(buff);
            ForceCloseChoiceGate();
        }

        void ApplyBuff(BuffData buff)
        {
            if (player == null) return;

            switch (buff.type)
            {
                case BuffType.AttackSpeed:
                    player.attackSpeed *= (1 + buff.value);
                    foreach (var jimmy in jimmies)
                    {
                        if (jimmy != null)
                            jimmy.attackSpeed *= (1 + buff.value);
                    }
                    Debug.Log($"攻击速度提升 {buff.value * 100}%");
                    break;

                case BuffType.MoveSpeed:
                    player.moveSpeed *= (1 + buff.value);
                    Debug.Log($"移动速度提升 {buff.value * 100}%");
                    break;

                case BuffType.Damage:
                    player.attackDamage *= (1 + buff.value);
                    Debug.Log($"伤害提升 {buff.value * 100}%");
                    break;

                case BuffType.Range:
                    player.attackRange *= (1 + buff.value);
                    Debug.Log($"攻击范围提升 {buff.value * 100}%");
                    break;

                case BuffType.Heal:
                    player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + buff.value);
                    Debug.Log($"恢复 {buff.value} 生命值");
                    break;
            }
        }
    }
}
