using UnityEngine;
using System.Collections;
using TowerDefence.Data; // 如果需要的话

namespace TowerDefence.Operators
{
    // 技能类型
    public enum SkillType
    {
        Manual,      // 手动触发
        Auto,        // 自动触发
        Passive      // 被动技能
    }

    // 技能触发方式
    public enum SkillActivation
    {
        OnDeploy,    // 部署时
        OnAttack,    // 攻击时
        OnHit,       // 受击时
        Charged      // 充能后
    }

    // 技能数据配置
    [CreateAssetMenu(fileName = "New Skill", menuName = "Tower Defence/Skill Data")]
    public class SkillData : ScriptableObject
    {
        public string skillName;
        public string skillID;
        public SkillType skillType;
        public SkillActivation activation;

        [TextArea(2, 4)]
        public string description;

        public Sprite skillIcon;

        // 技能参数
        public float cooldown = 10f;          // 冷却时间
        public float duration = 5f;           // 持续时间
        public int spCost = 30;               // SP消耗/初始SP
        public int spChargeType = 1;          // 充能类型：1-自动回复 2-攻击回复 3-受击回复

        // 技能效果数值
        public float attackMultiplier = 1f;   // 攻击力倍率
        public float attackSpeedBonus = 0f;   // 攻速加成
        public int blockCountBonus = 0;       // 阻挡数加成
        public float rangeBonus = 0f;         // 范围加成
        public bool isStun = false;           // 是否眩晕
        public float stunDuration = 0f;       // 眩晕时长
    }

    // 技能实例
    public class Skill
    {
        public SkillData data;
        public Operator owner;

        private float currentSP = 0f;
        private float cooldownTimer = 0f;
        private bool isActive = false;
        private Coroutine activeCoroutine;

        public float CurrentSP => currentSP;
        public float MaxSP => data.spCost;
        public bool IsReady => currentSP >= data.spCost && cooldownTimer <= 0;
        public bool IsActive => isActive;

        public Skill(SkillData skillData, Operator operatorOwner)
        {
            data = skillData;
            owner = operatorOwner;

            // 被动技能直接激活
            if (data.skillType == SkillType.Passive)
            {
                ApplyPassiveEffect();
            }
        }

        // 更新技能
        public void Update()
        {
            // 处理冷却
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }

            // 自动回复SP（类型1）
            if (!isActive && currentSP < data.spCost && data.spChargeType == 1)
            {
                currentSP += Time.deltaTime;
                currentSP = Mathf.Min(currentSP, data.spCost);
            }

            // 自动触发技能
            if (data.skillType == SkillType.Auto && IsReady)
            {
                Activate();
            }
        }

        // 攻击时充能
        public void OnAttack()
        {
            if (data.spChargeType == 2 && !isActive && currentSP < data.spCost)
            {
                currentSP += 1f;
                currentSP = Mathf.Min(currentSP, data.spCost);
            }
        }

        // 受击时充能
        public void OnHit()
        {
            if (data.spChargeType == 3 && !isActive && currentSP < data.spCost)
            {
                currentSP += 1f;
                currentSP = Mathf.Min(currentSP, data.spCost);
            }
        }

        // 激活技能
        public bool Activate()
        {
            if (!IsReady || isActive)
                return false;

            currentSP = 0f;
            isActive = true;

            if (data.duration > 0)
            {
                activeCoroutine = owner.StartCoroutine(SkillDurationCoroutine());
            }
            else
            {
                // 瞬发技能
                ApplyInstantEffect();
                cooldownTimer = data.cooldown;
                isActive = false;
            }

            return true;
        }

        // 技能持续时间协程
        IEnumerator SkillDurationCoroutine()
        {
            ApplySkillEffect(true); // 应用属性加成

            // 瞬发效果（如眩晕周围）
            if (data.isStun)
            {
                // 使用 Physics2D 检测范围内的 Enemy
                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    owner.transform.position,
                    owner.CurrentStats.attackRange
                );
                foreach (var hit in hits)
                {
                    Enemy e = hit.GetComponent<Enemy>();
                    if (e != null)
                    {
                        // 假设Enemy类有ApplyStun方法
                        e.ApplyStun(data.stunDuration);
                    }
                }
            }

            yield return new WaitForSeconds(data.duration);

            ApplySkillEffect(false); // 移除属性加成
            isActive = false;
            cooldownTimer = data.cooldown;
        }

        // 应用技能效果（增强版，融合了两个版本）
        void ApplySkillEffect(bool apply)
        {
            float multiplier = apply ? 1f : -1f;

            // 攻击力加成（基于基础攻击力计算）
            if (data.attackMultiplier != 1f)
            {
                // 假设Operator有BaseStats和CurrentStats属性
                int bonusAttack = Mathf.RoundToInt(
                    owner.BaseStats.attack * (data.attackMultiplier - 1f)
                );
                owner.CurrentStats.attack += apply ? bonusAttack : -bonusAttack;
            }

            // 攻速加成
            if (data.attackSpeedBonus != 0f)
            {
                // 攻速计算：应用时减少攻击间隔，移除时恢复
                if (apply)
                {
                    owner.CurrentStats.attackInterval /= (1f + data.attackSpeedBonus);
                }
                else
                {
                    owner.CurrentStats.attackInterval *= (1f + data.attackSpeedBonus);
                }
            }

            // 阻挡数加成
            if (data.blockCountBonus != 0)
            {
                owner.CurrentStats.blockCount += apply ?
                    data.blockCountBonus :
                    -data.blockCountBonus;
            }

            // 范围加成
            if (data.rangeBonus != 0f)
            {
                owner.CurrentStats.attackRange += multiplier * data.rangeBonus;
            }
        }

        // 瞬发效果
        void ApplyInstantEffect()
        {
            // 例如：眩晕周围敌人
            if (data.isStun)
            {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(
                    owner.transform.position,
                    owner.CurrentStats.attackRange
                );

                foreach (var enemy in enemies)
                {
                    Enemy e = enemy.GetComponent<Enemy>();
                    if (e != null)
                    {
                        e.ApplyStun(data.stunDuration);
                    }
                }
            }
        }

        // 被动效果
        void ApplyPassiveEffect()
        {
            isActive = true;
            ApplySkillEffect(true);
        }

        // 强制停止技能
        public void Stop()
        {
            if (activeCoroutine != null)
            {
                owner.StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            if (isActive)
            {
                ApplySkillEffect(false);
                isActive = false;
            }
        }
    }

    // 技能管理器组件
    public class SkillManager : MonoBehaviour
    {
        private Operator owner;
        private Skill currentSkill;
        public Skill CurrentSkill => currentSkill;

        void Awake()
        {
            owner = GetComponent<Operator>();
        }

        void Start()
        {
            // 自动装备第一个技能（如果有）
            if (owner.Data != null && owner.Data.skills != null && owner.Data.skills.Count > 0)
            {
                EquipSkill(owner.Data.skills[0]);
            }
        }

        // 装备技能
        public void EquipSkill(SkillData skillData)
        {
            if (currentSkill != null)
            {
                currentSkill.Stop();
            }

            currentSkill = new Skill(skillData, owner);
        }

        // 更新技能
        public void UpdateSkill()
        {
            currentSkill?.Update();
        }

        // 手动激活技能
        public bool ActivateSkill()
        {
            if (currentSkill != null &&
                currentSkill.data.skillType == SkillType.Manual)
            {
                return currentSkill.Activate();
            }
            return false;
        }

        // 攻击事件
        public void OnAttackPerformed()
        {
            currentSkill?.OnAttack();
        }

        // 受击事件
        public void OnDamageTaken()
        {
            currentSkill?.OnHit();
        }
    }
}