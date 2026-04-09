// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does joystick-controlled lord + auto-following Jimmy create satisfying combat?
// Date: 2026-04-07

using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    /// <summary>
    /// 领主控制器 - 摇杆移动 + 自动攻击
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public Joystick joystick; // 简单摇杆引用

        [Header("Health")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public bool IsAlive => currentHealth > 0;

        [Header("Combat")]
        public float attackRange = 3f;
        public float attackDamage = 15f;
        public float attackCooldown = 0.5f;
        public LayerMask enemyLayer;

        [Header("Events")]
        public System.Action OnDeath;

        [Header("Visual")]
        public Transform visualTransform;
        public Transform attackPoint;
        public SpriteRenderer healthBar;

        // 公开属性供测试使用
        public float baseAttackSpeed { get; set; } = 1f;
        public float baseMoveSpeed { get; set; } = 5f;
        public float attackSpeed { get; set; } = 1f;

        private float lastAttackTime;
        private Rigidbody2D rb;
        private Enemy currentTarget;

        // 协同攻击计数
        public int comboCount = 0;
        public float comboResetTime = 2f;
        private float lastComboTime;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // 自动设置visualTransform
            if (visualTransform == null)
            {
                visualTransform = transform;
            }
        }

        void Update()
        {
            HandleMovement();
            HandleCombat();
            HandleComboReset();
        }

        void HandleMovement()
        {
            // 摇杆输入（适配Input System，不使用旧版Input.GetAxis）
            float horizontal = 0f;
            float vertical = 0f;

            // 使用joystick组件
            if (joystick != null)
            {
                horizontal = joystick.Horizontal;
                vertical = joystick.Vertical;
            }

            Vector2 moveInput = new Vector2(horizontal, vertical).normalized;

            // 移动
            rb.velocity = moveInput * moveSpeed;

            // 朝向
            if (moveInput.x != 0)
            {
                float scaleX = moveInput.x > 0 ? 1f : -1f;
                visualTransform.localScale = new Vector3(scaleX, 1f, 1f);
            }
        }

        void HandleCombat()
        {
            // 寻找范围内最近的敌人
            currentTarget = FindNearestEnemy();

            if (currentTarget != null && Time.time > lastAttackTime + attackCooldown)
            {
                Attack(currentTarget);
                lastAttackTime = Time.time;
            }
        }

        Enemy FindNearestEnemy()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

            Enemy nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemyCollider in enemies)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    float dist = Vector2.Distance(transform.position, enemy.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = enemy;
                    }
                }
            }

            return nearest;
        }

        void Attack(Enemy target)
        {
            // 简单近战攻击
            target.TakeDamage(attackDamage);

            // 协同攻击计数
            if (Time.time - lastComboTime < comboResetTime)
            {
                comboCount++;
                if (comboCount >= 3)
                {
                    TriggerSynergyAttack(target);
                    comboCount = 0;
                }
            }
            else
            {
                comboCount = 1;
            }
            lastComboTime = Time.time;

            // 视觉反馈（原型用简单缩放）- 添加null检查
            if (visualTransform != null)
            {
                visualTransform.localScale *= 1.2f;
                Invoke(nameof(ResetVisual), 0.1f);
            }
        }

        void TriggerSynergyAttack(Enemy target)
        {
            // 触发协同攻击 - 额外伤害
            target.TakeDamage(attackDamage * 0.5f);
            Debug.Log("协同攻击触发！");
        }

        void HandleComboReset()
        {
            if (Time.time - lastComboTime > comboResetTime)
            {
                comboCount = 0;
            }
        }

        void ResetVisual()
        {
            if (visualTransform != null)
            {
                visualTransform.localScale = new Vector3(
                    Mathf.Sign(visualTransform.localScale.x),
                    1f,
                    1f
                );
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            currentHealth -= damage;
            UpdateHealthBar();

            // 受伤闪烁
            if (visualTransform != null)
            {
                SpriteRenderer sr = visualTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.red;
                    Invoke(nameof(ResetColor), 0.1f);
                }
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                float healthPercent = currentHealth / maxHealth;
                healthBar.transform.localScale = new Vector3(healthPercent, 1f, 1f);
            }
        }

        void ResetColor()
        {
            if (visualTransform != null)
            {
                SpriteRenderer sr = visualTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = Color.white;
            }
        }

        void Die()
        {
            currentHealth = 0;
            OnDeath?.Invoke();
            Debug.Log("💀 玩家死亡！");

            // 通知游戏管理器
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamageTown(9999f); // 玩家死亡 = 游戏结束
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
