using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Data;
using TowerDefence.Operators; // 技能

public class Operator : MonoBehaviour
{
    [Header("数据引用")]
    public OperatorData Data;

    [Header("运行时属性")]
    public OperatorStats CurrentStats;
    public SkillManager skillManager;

    private float currentHP;
    private List<Enemy> blockedEnemies = new List<Enemy>();
    private float attackTimer;

    // Grid 系统需要的引用
    public GridCell occupiedCell;

    void Awake()
    {
        skillManager = gameObject.AddComponent<SkillManager>();
    }

    public void Initialize(OperatorData data)
    {
        this.Data = data;
        this.CurrentStats = data.baseStats; // 复制结构体
        this.currentHP = CurrentStats.maxHP;

        // 初始化外观
        GetComponent<SpriteRenderer>().sprite = data.operatorIcon;

        // 初始化技能
        if (data.skills != null) skillManager.EquipSkill(data.skills[0]); // 默认带一技能
    }

    void Update()
    {
        skillManager.UpdateSkill();
        HandleCombat();
    }

    void HandleCombat()
    {
        // 1. 处理阻挡 (Block)
        CheckBlocking();

        // 2. 处理攻击 (Attack)
        attackTimer += Time.deltaTime;
        // 攻速计算：100 / (100 + bonus) 
        if (attackTimer >= CurrentStats.attackInterval)
        {
            Enemy target = FindTarget();
            if (target != null)
            {
                PerformAttack(target);
                attackTimer = 0;
            }
        }
    }

    void CheckBlocking()
    {
        // 只有地面单位且没达到阻挡上限时才阻挡
        if (Data.positionType != OperatorPosition.Ground) return;
        if (blockedEnemies.Count >= CurrentStats.blockCount) return;

        // 获取当前格子上的敌人
        if (occupiedCell == null) occupiedCell = GridManager.Instance.GetCellFromWorld(transform.position);
        if (occupiedCell == null) return;

        // 使用 Physics2D 检测非常近的敌人 (模拟进入格子)
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.5f, 0.5f), 0);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && !blockedEnemies.Contains(e) && blockedEnemies.Count < CurrentStats.blockCount)
            {
                if (e.TryBeBlocked(this))
                {
                    blockedEnemies.Add(e);
                }
            }
        }
    }

    public void UnblockEnemy(Enemy e)
    {
        if (blockedEnemies.Contains(e)) blockedEnemies.Remove(e);
    }

    Enemy FindTarget()
    {
        // 优先攻击被阻挡的
        if (blockedEnemies.Count > 0) return blockedEnemies[0];

        // 否则搜索范围内最近的敌人
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, CurrentStats.attackRange);
        float minDist = float.MaxValue;
        Enemy bestTarget = null;

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
            {
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    bestTarget = e;
                }
            }
        }
        return bestTarget;
    }

    void PerformAttack(Enemy target)
    {
        // 触发攻击动作
        // ...

        // 造成伤害
        target.TakeDamage(CurrentStats.attack);

        // 触发技能回调
        skillManager.OnAttackPerformed();
    }

    public void TakeDamage(float damage)
    {
        // 计算防御
        float finalDmg = Mathf.Max(damage - CurrentStats.defense, damage * 0.05f);
        currentHP -= finalDmg;

        skillManager.OnDamageTaken();

        if (currentHP <= 0) Retreat();
    }

    public void Retreat()
    {
        // 释放格子
        if (occupiedCell != null) occupiedCell.ClearOccupied();

        // 解除所有阻挡
        for (int i = blockedEnemies.Count - 1; i >= 0; i--)
        {
            blockedEnemies[i].Unblock();
        }

        Destroy(gameObject);
    }

    // 动态修改属性接口 (供 Skill 使用)
    public void BuffAttack(float multiplier, bool apply)
    {
        if (apply) CurrentStats.attack *= multiplier;
        else CurrentStats.attack /= multiplier; // 简化回滚，实际建议用 Base + Buff 方式计算
    }
}