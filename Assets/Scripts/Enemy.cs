// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does joystick-controlled lord + auto-following Jimmy create satisfying combat?
// Date: 2026-04-07

using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    public enum EnemyType
    {
        WildBoar,      // 野猪 - 普通
        BoarWarrior,   // 野猪战士 - 精英
        BoarBoss       // 野猪王 - Boss
    }

    /// <summary>
    /// 敌人AI - 向城镇移动，攻击沿途目标
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Type")]
        public EnemyType enemyType;

        [Header("Stats")]
        public float maxHealth = 50f;
        public float currentHealth;
        public float moveSpeed = 2f;
        public float attackDamage = 10f;
        public float attackCooldown = 1f;
        public float attackRange = 1f;

        [Header("Targeting")]
        public Transform townCenter;
        public LayerMask playerLayer;

        [Header("Visual")]
        public Transform visualTransform;
        public SpriteRenderer healthBar;

        private float lastAttackTime;
        private Rigidbody2D rb;
        private Transform currentTarget;
        private bool isStunned = false;
        private float stunEndTime = 0f;

        public bool IsAlive => currentHealth > 0;
        public bool IsStunned => isStunned && Time.time < stunEndTime;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            currentHealth = maxHealth;
            currentTarget = townCenter;
        }

        void Update()
        {
            if (!IsAlive) return;

            // 处理眩晕
            if (isStunned)
            {
                if (Time.time >= stunEndTime)
                {
                    isStunned = false;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    return;
                }
            }

            FindTarget();
            MoveToTarget();
            HandleAttack();
            UpdateHealthBar();
        }

        void FindTarget()
        {
            // 寻找范围内的玩家或吉米
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 3f, playerLayer);

            float nearestDist = float.MaxValue;
            Transform nearest = townCenter;

            foreach (var col in players)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = col.transform;
                }
            }

            currentTarget = nearest;
        }

        void MoveToTarget()
        {
            if (currentTarget == null)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            float distToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distToTarget > attackRange)
            {
                Vector2 dir = (currentTarget.position - transform.position).normalized;
                rb.velocity = dir * moveSpeed;

                // 朝向
                if (dir.x != 0)
                {
                    float scaleX = dir.x > 0 ? 1f : -1f;
                    visualTransform.localScale = new Vector3(scaleX, 1f, 1f);
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

        void HandleAttack()
        {
            if (currentTarget == null) return;
            if (Time.time < lastAttackTime + attackCooldown) return;

            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= attackRange)
            {
                Attack(currentTarget);
                lastAttackTime = Time.time;
            }
        }

        void Attack(Transform target)
        {
            // 攻击玩家
            var player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                // 原型简化：直接调用玩家的受伤逻辑
                return;
            }

            // 攻击吉米
            var jimmy = target.GetComponent<Jimmy>();
            if (jimmy != null)
            {
                return;
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            // 受伤闪烁
            if (healthBar != null)
            {
                healthBar.color = Color.red;
                Invoke(nameof(ResetColor), 0.1f);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Stun(float duration)
        {
            isStunned = true;
            stunEndTime = Time.time + duration;
        }

        void ResetColor()
        {
            if (healthBar != null)
                healthBar.color = Color.green;
        }

        void Die()
        {
            // 通知波次管理器
            WaveManager.Instance?.OnEnemyDeath(this);

            // 简单消失
            Destroy(gameObject);
        }

        void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                float healthPercent = currentHealth / maxHealth;
                healthBar.transform.localScale = new Vector3(healthPercent, 1f, 1f);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
