// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does joystick-controlled lord + auto-following Jimmy create satisfying combat?
// Date: 2026-04-07

using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    public enum JimmyType
    {
        FlameFox,      // 焰尾狐 - 敏捷型
        BoarKing,      // 野猪王 - 力量型
        RockGolem      // 岩石巨像 - 防御型
    }

    /// <summary>
    /// 吉米AI - 跟随领主 + 自动攻击
    /// </summary>
    public class Jimmy : MonoBehaviour
    {
        [Header("Type")]
        public JimmyType jimmyType;

        [Header("Following")]
        public Transform lord;
        public float followDistance = 3f;        // 修改: 从1.5增加到3，保持2-10米距离
        public float minFollowDistance = 2.5f;   // 新增: 最小跟随距离，不应小于2米
        public float followSpeed = 4f;

        [Header("Combat")]
        public float attackRange = 2.5f;
        public float attackCooldown = 1f;
        public LayerMask enemyLayer;

        [Header("Visual")]
        public Transform visualTransform;
        public Color flameColor = new Color(1f, 0.5f, 0f);
        public Color boarColor = new Color(0.6f, 0.4f, 0.2f);
        public Color rockColor = new Color(0.5f, 0.5f, 0.5f);

        // 公开属性供测试使用
        public float attackSpeed { get; set; } = 1f;
        public int attackCount { get; private set; } = 0;
        public float totalDamageDealt { get; private set; } = 0f;
        public float maxHealth { get; private set; } = 100f;

        private float attackDamage;
        private float lastAttackTime;
        private Rigidbody2D rb;
        private SpriteRenderer sr;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponentInChildren<SpriteRenderer>();

            // 确保Rigidbody2D没有约束（修复场景文件未同步问题）
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints2D.None;
                Debug.Log($"[{name}] Rigidbody constraints reset to None");
            }

            // 自动查找lord（如果没有在Inspector中设置）
            if (lord == null)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    lord = player.transform;
                    Debug.Log($"[{name}] 自动设置lord为Player");
                }
                else
                {
                    Debug.LogError($"[{name}] 找不到Player！");
                }
            }

            // 自动设置visualTransform（如果没有设置）
            if (visualTransform == null)
            {
                visualTransform = transform;
                Debug.Log($"[{name}] 自动设置visualTransform为transform");
            }

            SetupByType();
        }

        void SetupByType()
        {
            switch (jimmyType)
            {
                case JimmyType.FlameFox:
                    attackDamage = 20f;
                    attackCooldown = 0.8f;
                    followSpeed = 5f;
                    attackSpeed = 1.25f;
                    maxHealth = 60f;
                    if (sr != null) sr.color = flameColor;
                    break;

                case JimmyType.BoarKing:
                    attackDamage = 25f;
                    attackCooldown = 1.2f;
                    followSpeed = 4f;
                    attackSpeed = 1.1f;  // 修改: 从0.83改为1.1，在1.0-1.3范围内
                    maxHealth = 120f;
                    if (sr != null) sr.color = boarColor;
                    break;

                case JimmyType.RockGolem:
                    attackDamage = 15f;
                    attackCooldown = 1.5f;
                    followSpeed = 3f;
                    attackSpeed = 0.67f;
                    maxHealth = 250f;
                    if (sr != null) sr.color = rockColor;
                    break;
            }
        }

        void Update()
        {
            if (lord == null) return;

            HandleFollowing();
            HandleCombat();
        }

        void HandleFollowing()
        {
            float distToLord = Vector2.Distance(transform.position, lord.position);

            // Debug: Log every second
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[{name}] dist={distToLord:F2}, speed={followSpeed}, lord={lord.position}, pos={transform.position}");
            }

            Vector2 moveDir = Vector2.zero;

            // 如果离领主太近（小于最小距离），后退一点
            if (distToLord < minFollowDistance)
            {
                moveDir = (transform.position - lord.position).normalized;
                moveDir *= followSpeed * 2f * Time.deltaTime;  // 加快后退速度
            }
            // 如果离领主太远（超过2倍跟随距离），快速跟上
            else if (distToLord > followDistance * 2f)
            {
                moveDir = (lord.position - transform.position).normalized;
                moveDir *= followSpeed * 1.5f * Time.deltaTime;
            }
            // 在理想距离范围外，正常速度跟随
            else if (distToLord > followDistance)
            {
                moveDir = (lord.position - transform.position).normalized;
                moveDir *= followSpeed * Time.deltaTime;
            }

            // 使用MovePosition进行物理移动（比velocity更可靠，同时保持碰撞检测）
            if (moveDir != Vector2.zero)
            {
                Vector2 targetPos = (Vector2)transform.position + moveDir;
                rb.MovePosition(targetPos);
            }

            // 朝向始终面向最近的敌人或移动方向
            FaceNearestEnemyOrDirection(moveDir);
        }

        void FaceNearestEnemyOrDirection(Vector2 moveDir)
        {
            // 如果visualTransform为null，使用transform作为后备
            if (visualTransform == null)
            {
                visualTransform = transform;
            }

            Enemy nearest = FindNearestEnemy();
            if (nearest != null)
            {
                float dirX = nearest.transform.position.x - transform.position.x;
                if (dirX != 0)
                {
                    float scaleX = dirX > 0 ? 1f : -1f;
                    visualTransform.localScale = new Vector3(scaleX, 1f, 1f);
                }
            }
            else if (moveDir.magnitude > 0.001f)
            {
                float scaleX = moveDir.x > 0 ? 1f : -1f;
                visualTransform.localScale = new Vector3(scaleX, 1f, 1f);
            }
        }

        void HandleCombat()
        {
            if (Time.time < lastAttackTime + attackCooldown) return;

            Enemy target = FindNearestEnemy();
            if (target != null)
            {
                Attack(target);
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
            target.TakeDamage(attackDamage);
            attackCount++;
            totalDamageDealt += attackDamage;

            // 视觉反馈 - 添加null检查
            if (visualTransform != null)
            {
                visualTransform.localScale *= 1.3f;
                Invoke(nameof(ResetVisual), 0.1f);
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

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
