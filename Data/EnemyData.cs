using UnityEngine;
using System.Collections;

namespace TowerDefence
{
    // 敌人类型
    public enum EnemyType
    {
        Normal,      // 普通
        Elite,       // 精英
        Boss,        // Boss
        Flying       // 飞行
    }

    // 敌人基础类
    public class Enemy : MonoBehaviour
    {
        [Header("敌人属性")]
        public string enemyName = "敌人";
        public EnemyType enemyType = EnemyType.Normal;
        public int maxHP = 1000;
        public int currentHP;
        public int attack = 100;
        public int defense = 50;
        public float magicResistance = 0f; // 百分比
        public float moveSpeed = 2f;
        public bool isFlying = false;

        [Header("状态")]
        public bool isAlive = true;
        public bool isBlocked = false;
        public bool isStunned = false;
        private float stunTimer = 0f;

        [Header("路径")]
        public Transform[] waypoints;
        private int currentWaypointIndex = 0;

        [Header("阻挡")]
        private Operators.Operator blockingOperator;

        [Header("组件")]
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;

        public int Defense => defense;
        public float MagicResistance => magicResistance;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            currentHP = maxHP;
        }

        void Update()
        {
            if (!isAlive) return;

            // 处理眩晕
            if (isStunned)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    isStunned = false;
                }
                return;
            }

            // 如果被阻挡，攻击阻挡干员
            if (isBlocked && blockingOperator != null)
            {
                AttackOperator();
            }
            else
            {
                Move();
            }
        }

        // 移动
        void Move()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (currentWaypointIndex >= waypoints.Length)
            {
                ReachEnd();
                return;
            }

            Vector3 targetPos = waypoints[currentWaypointIndex].position;
            Vector3 direction = (targetPos - transform.position).normalized;

            transform.position += direction * moveSpeed * Time.deltaTime;

            // 到达路点
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                currentWaypointIndex++;
            }
        }

        // 受到伤害
        public void TakeDamage(int damage)
        {
            if (!isAlive) return;

            currentHP -= damage;

            // 显示伤害数字（可选）
            ShowDamageNumber(damage);

            if (currentHP <= 0)
            {
                Die();
            }
        }

        // 眩晕
        public void Stun(float duration)
        {
            isStunned = true;
            stunTimer = duration;
        }

        // 减速
        public void Slow(float slowAmount, float duration)
        {
            StartCoroutine(SlowCoroutine(slowAmount, duration));
        }

        IEnumerator SlowCoroutine(float slowAmount, float duration)
        {
            float originalSpeed = moveSpeed;
            moveSpeed *= (1f - slowAmount);

            yield return new WaitForSeconds(duration);

            moveSpeed = originalSpeed;
        }

        // 被阻挡
        public void OnBlocked(Operators.Operator op)
        {
            isBlocked = true;
            blockingOperator = op;
        }

        // 解除阻挡
        public void OnUnblocked()
        {
            isBlocked = false;
            blockingOperator = null;
        }

        // 攻击干员
        void AttackOperator()
        {
            if (blockingOperator != null)
            {
                // 简单的攻击间隔
                blockingOperator.TakeDamage(attack);
            }
        }

        // 死亡
        void Die()
        {
            isAlive = false;

            // 如果被阻挡，解除阻挡
            if (isBlocked && blockingOperator != null)
            {
                blockingOperator.UnblockEnemy(this);
            }

            // 播放死亡动画
            // 掉落奖励
            DropRewards();

            // 销毁对象
            Destroy(gameObject, 0.5f);
        }

        // 到达终点
        void ReachEnd()
        {
            // 扣除生命值等
            isAlive = false;
            Destroy(gameObject);
        }

        // 掉落奖励
        void DropRewards()
        {
            // 增加部署点等
            var placementSystem = FindObjectOfType<Operators.OperatorPlacementSystem>();
            if (placementSystem != null)
            {
                placementSystem.AddDeploymentPoints(2);
            }
        }

        // 显示伤害数字（可选实现）
        void ShowDamageNumber(int damage)
        {
            // 实例化伤害数字预制体
        }

        // 碰撞检测（用于阻挡系统）
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (isFlying) return; // 飞行单位不会被阻挡

            if (collision.CompareTag("Operator"))
            {
                var op = collision.GetComponent<Operators.Operator>();
                if (op != null && op.isActive && !isBlocked)
                {
                    if (op.TryBlockEnemy(this))
                    {
                        OnBlocked(op);
                    }
                }
            }
        }
    }
}