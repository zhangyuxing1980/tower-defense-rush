// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Choice Gate System Test
// Date: 2026-04-07

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefenseRush.Prototype;

namespace TowerDefenseRush.Testing
{
    /// <summary>
    /// 选择门系统测试 - 验证波次之间的Buff选择机制
    /// </summary>
    public class ChoiceGateTest : TestCaseBase
    {
        [Header("Test Configuration")]
        public GameObject choiceGateUIPrefab;

        private ChoiceGateManager choiceGate;
        private WaveManager waveManager;
        private PlayerController player;
        private Jimmy[] jimmies;

        // 测试数据收集
        private List<BuffData> selectedBuffs = new List<BuffData>();
        private int choiceGateOpenedCount = 0;
        private int choiceGateClosedCount = 0;

        protected override void Awake()
        {
            base.Awake();
            testId = "CHOICE_001";
            testName = "选择门系统测试";
            description = "验证波次间Buff选择机制，包括选项生成、选择应用、UI交互";
            category = "Progression";
            priority = 1;
            timeout = 60f;
        }

        protected override IEnumerator OnSetUp()
        {
            LogInfo("初始化选择门系统测试");

            choiceGate = FindObjectOfType<ChoiceGateManager>();
            AssertNotNull(choiceGate, "场景中必须有选择门管理器");

            waveManager = FindObjectOfType<WaveManager>();
            AssertNotNull(waveManager, "场景中必须有波次管理器");

            player = FindObjectOfType<PlayerController>();
            AssertNotNull(player, "场景中必须有玩家");

            jimmies = FindObjectsOfType<Jimmy>();
            Assert(jimmies.Length >= 3, "场景中至少需要3只吉米");

            // 订阅事件
            if (choiceGate != null)
            {
                choiceGate.OnChoiceGateOpened += OnChoiceGateOpened;
                choiceGate.OnChoiceGateClosed += OnChoiceGateClosed;
                choiceGate.OnBuffSelected += OnBuffSelected;
            }

            selectedBuffs.Clear();
            choiceGateOpenedCount = 0;
            choiceGateClosedCount = 0;

            yield return null;
        }

        public override IEnumerator Run()
        {
            LogInfo("开始选择门系统测试");

            // 测试1: Buff配置验证
            yield return TestBuffConfiguration();

            // 测试2: 选项生成
            yield return TestOptionGeneration();

            // 测试3: Buff应用
            yield return TestBuffApplication();

            // 测试4: 数值计算
            yield return TestBuffValueCalculation();

            // 测试5: 多次选择累加
            yield return TestMultipleBuffStacking();

            LogInfo("选择门系统测试完成");
        }

        IEnumerator TestBuffConfiguration()
        {
            LogInfo("测试1: Buff配置验证");

            AssertNotNull(choiceGate.availableBuffs, "可用Buff列表不应为空");
            Assert(choiceGate.availableBuffs.Length >= 5, "至少需要有5种Buff类型");

            // 验证每种Buff的配置
            foreach (var buff in choiceGate.availableBuffs)
            {
                Assert(!string.IsNullOrEmpty(buff.buffName), "Buff必须有名称");
                Assert(buff.value > 0, $"Buff {buff.buffName} 数值必须大于0");
                Assert(buff.icon != null, $"Buff {buff.buffName} 需要有图标");

                LogInfo($"Buff配置: {buff.buffName} ({buff.type}, +{buff.value})");
            }

            // 验证必须有所有5种类型
            bool hasAttackSpeed = false, hasMoveSpeed = false, hasDamage = false,
                 hasRange = false, hasHeal = false;

            foreach (var buff in choiceGate.availableBuffs)
            {
                switch (buff.type)
                {
                    case BuffType.AttackSpeed: hasAttackSpeed = true; break;
                    case BuffType.MoveSpeed: hasMoveSpeed = true; break;
                    case BuffType.Damage: hasDamage = true; break;
                    case BuffType.Range: hasRange = true; break;
                    case BuffType.Heal: hasHeal = true; break;
                }
            }

            Assert(hasAttackSpeed, "必须有攻击速度Buff");
            Assert(hasMoveSpeed, "必须有移动速度Buff");
            Assert(hasDamage, "必须有伤害Buff");
            Assert(hasRange, "必须有攻击范围Buff");
            Assert(hasHeal, "必须有治疗Buff");

            LogInfo("Buff配置验证通过");
            yield return null;
        }

        IEnumerator TestOptionGeneration()
        {
            LogInfo("测试2: 选项生成测试");

            // 强制打开选择门
            choiceGate.ForceOpenChoiceGate();

            yield return WaitUntil(() => choiceGate.IsOpen, 2f, "等待选择门打开");

            Assert(choiceGateOpenedCount > 0, "选择门应该触发打开事件");
            Assert(choiceGate.CurrentOptions != null, "应该生成选项");
            AssertEqual(3, choiceGate.CurrentOptions.Length, "应该生成3个选项");

            // 验证选项不重复
            HashSet<BuffType> types = new HashSet<BuffType>();
            foreach (var option in choiceGate.CurrentOptions)
            {
                Assert(!types.Contains(option.type), $"选项不应重复: {option.type}");
                types.Add(option.type);
                LogInfo($"生成选项: {option.buffName} ({option.type})");
            }

            // 关闭选择门
            choiceGate.ForceCloseChoiceGate();
            yield return Wait(0.5f);

            LogInfo("选项生成测试通过");
        }

        IEnumerator TestBuffApplication()
        {
            LogInfo("测试3: Buff应用测试");

            // 记录初始状态
            float initialAttackSpeed = player.attackSpeed;
            float initialMoveSpeed = player.moveSpeed;

            // 模拟选择一个Buff
            var attackSpeedBuff = System.Array.Find(
                choiceGate.availableBuffs,
                b => b.type == BuffType.AttackSpeed
            );

            AssertNotNull(attackSpeedBuff, "应找到攻击速度Buff");

            choiceGate.SimulateBuffSelection(attackSpeedBuff);

            yield return Wait(0.5f);

            // 验证Buff已应用
            Assert(player.attackSpeed > initialAttackSpeed,
                $"攻击速度应该增加: {initialAttackSpeed} -> {player.attackSpeed}");

            LogInfo($"Buff应用成功: 攻击速度 {initialAttackSpeed:F2} -> {player.attackSpeed:F2}");

            // 验证吉米也受到影响
            foreach (var jimmy in jimmies)
            {
                Assert(jimmy.attackSpeed > 0, "吉米攻击速度应该被更新");
            }

            DataCollector.CollectData("AttackSpeedIncrease",
                player.attackSpeed - initialAttackSpeed);
        }

        IEnumerator TestBuffValueCalculation()
        {
            LogInfo("测试4: Buff数值计算测试");

            // 测试各种Buff的数值计算是否正确
            var damageBuff = System.Array.Find(
                choiceGate.availableBuffs,
                b => b.type == BuffType.Damage
            );
            var rangeBuff = System.Array.Find(
                choiceGate.availableBuffs,
                b => b.type == BuffType.Range
            );

            if (damageBuff != null)
            {
                float baseDamage = player.attackDamage;
                float expectedDamage = baseDamage * (1 + damageBuff.value);

                choiceGate.SimulateBuffSelection(damageBuff);
                yield return Wait(0.3f);

                AssertApproximately(expectedDamage, player.attackDamage, 0.1f,
                    "伤害Buff计算应正确");

                LogInfo($"伤害Buff计算: {baseDamage} * (1 + {damageBuff.value}) = {player.attackDamage}");
            }

            if (rangeBuff != null)
            {
                float baseRange = player.attackRange;
                float expectedRange = baseRange * (1 + rangeBuff.value);

                choiceGate.SimulateBuffSelection(rangeBuff);
                yield return Wait(0.3f);

                AssertApproximately(expectedRange, player.attackRange, 0.1f,
                    "范围Buff计算应正确");

                LogInfo($"范围Buff计算: {baseRange} * (1 + {rangeBuff.value}) = {player.attackRange}");
            }

            yield return null;
        }

        IEnumerator TestMultipleBuffStacking()
        {
            LogInfo("测试5: 多次Buff叠加测试");

            // 重置玩家状态
            player.attackSpeed = player.baseAttackSpeed;

            float initialSpeed = player.attackSpeed;
            float expectedMultiplier = 1f;

            // 模拟选择3次攻击速度Buff
            var speedBuff = System.Array.Find(
                choiceGate.availableBuffs,
                b => b.type == BuffType.AttackSpeed
            );

            if (speedBuff != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    choiceGate.SimulateBuffSelection(speedBuff);
                    expectedMultiplier *= (1 + speedBuff.value);
                    yield return Wait(0.2f);
                }

                float expectedSpeed = initialSpeed * expectedMultiplier;
                float actualSpeed = player.attackSpeed;

                LogInfo($"多次Buff叠加: {initialSpeed:F2} * {expectedMultiplier:F2} = {actualSpeed:F2}");

                AssertApproximately(expectedSpeed, actualSpeed, 0.5f,
                    "多次Buff应正确叠加");

                DataCollector.CollectData("FinalAttackSpeedMultiplier", expectedMultiplier);
            }

            yield return null;
        }

        void OnChoiceGateOpened()
        {
            choiceGateOpenedCount++;
            LogInfo("选择门已打开");
        }

        void OnChoiceGateClosed()
        {
            choiceGateClosedCount++;
            LogInfo("选择门已关闭");
        }

        void OnBuffSelected(BuffData buff)
        {
            selectedBuffs.Add(buff);
            LogInfo($"选择了Buff: {buff.buffName}");
        }

        protected override IEnumerator OnTearDown()
        {
            LogInfo("清理选择门测试");

            // 取消事件订阅
            if (choiceGate != null)
            {
                choiceGate.OnChoiceGateOpened -= OnChoiceGateOpened;
                choiceGate.OnChoiceGateClosed -= OnChoiceGateClosed;
                choiceGate.OnBuffSelected -= OnBuffSelected;
            }

            // 收集测试数据
            DataCollector.CollectData("ChoiceGateOpenedCount", choiceGateOpenedCount);
            DataCollector.CollectData("SelectedBuffCount", selectedBuffs.Count);

            yield return null;
        }
    }
}
