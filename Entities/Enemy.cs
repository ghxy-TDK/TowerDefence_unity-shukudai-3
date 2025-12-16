using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Operators; // 为了引用 Operator

public class Enemy : MonoBehaviour
{
    [Header("配置")]
    public EnemyData baseData; // 基础数值配置

    [Header("运行时状态")]
    public bool isFlying = false;
    private float currentHP;
    private float currentMoveSpeed;

    // 状态效果
    private bool isStunned = false;
    private float stunTimer = 0f;
    private bool isSlowed = false;

    // 寻路与移动 (来自 Set A)
    private List<Vector2Int> pathPoints;
    private int pathIndex = 0;

    // 战斗与阻挡 (来自 Set B + Set A)
    private Operator blockingOperator; // 阻挡我的干员
    private bool isBlocked => blockingOperator != null;
    private float attackTimer = 0f;

    void Start()
    {
        InitStats();
        InitPath(); // 依赖 GridManager
    }

    void InitStats()
    {
        if (baseData != null)
        {
            currentHP = baseData.maxHp;
            currentMoveSpeed = baseData.moveSpeed;
        }
    }

    void InitPath()
    {
        if (GridManager.Instance != null)
        {
            pathPoints = GridManager.Instance.pathPoints;
            pathIndex = 1; // 0 是起点

            // 对齐位置
            var startCell = GridManager.Instance.GetStartCell();
            if (startCell != null) transform.position = startCell.worldPosition;
        }
    }

    void Update()
    {
        // 1. 处理状态效果 (Set B)
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0) isStunned = false;
            return; // 眩晕时不移动不攻击
        }

        // 2. 攻击逻辑
        if (isBlocked)
        {
            AttackTarget(blockingOperator);
        }
        else
        {
            // 未被阻挡，尝试移动
            Move();
        }
    }

    void Move()
    {
        if (pathPoints == null || pathIndex >= pathPoints.Count) return;

        Vector2Int targetGrid = pathPoints[pathIndex];
        GridCell targetCell = GridManager.Instance.GetCell(targetGrid.x, targetGrid.y);

        if (targetCell == null) return;

        // 计算实际速度 (考虑减速)
        float speed = currentMoveSpeed;

        transform.position = Vector3.MoveTowards(transform.position, targetCell.worldPosition, speed * Time.deltaTime);

        // 检测是否到达路点
        if (Vector3.Distance(transform.position, targetCell.worldPosition) < 0.05f)
        {
            pathIndex++;
            if (pathIndex >= pathPoints.Count) ReachEnd();
        }

        // 旋转
        Vector3 dir = (targetCell.worldPosition - transform.position).normalized;
        if (dir != Vector3.zero) transform.right = dir; // 2D常用朝向处理
    }

    // 被干员阻挡 (Set B 逻辑)
    public bool TryBeBlocked(Operator op)
    {
        if (isFlying || isBlocked) return false;

        blockingOperator = op;
        return true;
    }

    // 解除阻挡
    public void Unblock()
    {
        blockingOperator = null;
    }

    void AttackTarget(Operator target)
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= baseData.attackInterval)
        {
            attackTimer = 0;
            target.TakeDamage(baseData.attackPower);
        }
    }

    public void TakeDamage(float amount, float armorPenetration = 0)
    {
        float finalDef = Mathf.Max(0, baseData.defense - armorPenetration);
        float damage = Mathf.Max(amount - finalDef, amount * 0.05f); // 抛光伤害

        currentHP -= damage;
        // 可以添加 UI 飘字

        if (currentHP <= 0) Die();
    }

    // 状态接口 (Set B)
    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = Mathf.Max(stunTimer, duration);
    }

    public void ApplySlow(float percentage, float duration)
    {
        // 简化实现：直接减速逻辑可放入 Coroutine
        StartCoroutine(SlowRoutine(percentage, duration));
    }

    System.Collections.IEnumerator SlowRoutine(float pct, float time)
    {
        float original = baseData.moveSpeed;
        currentMoveSpeed = original * (1 - pct);
        yield return new WaitForSeconds(time);
        currentMoveSpeed = original;
    }

    void Die()
    {
        if (blockingOperator != null) blockingOperator.UnblockEnemy(this);

        if (WaveManager.Instance != null) WaveManager.Instance.EnemyDefeated();
        if (CostManager.Instance != null) CostManager.Instance.GainCost(1); // 杀敌回费

        Destroy(gameObject);
    }

    void ReachEnd()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.DeductLife();
        if (WaveManager.Instance != null) WaveManager.Instance.EnemyDefeated();
        Destroy(gameObject);
    }
}