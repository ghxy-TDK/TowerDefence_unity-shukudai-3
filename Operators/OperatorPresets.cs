using UnityEngine;

namespace TowerDefence.Operators
{
    // 这个类提供各职业干员的预设配置示例
    // 可以通过 Unity 编辑器创建 ScriptableObject 资源
    public static class OperatorPresets
    {
        // 先锋职业 - 低成本，快速回复费用
        public static OperatorStats VanguardStats = new OperatorStats
        {
            maxHP = 1200,
            currentHP = 1200,
            attack = 350,
            defense = 120,
            magicResistance = 0,
            blockCount = 2,
            attackInterval = 1.05f,
            attackRange = 1.5f,
            deploymentCost = 10,
            redeployTime = 70f
        };

        // 近卫职业 - 平衡型，攻防兼备
        public static OperatorStats GuardStats = new OperatorStats
        {
            maxHP = 1500,
            currentHP = 1500,
            attack = 450,
            defense = 150,
            magicResistance = 0,
            blockCount = 1,
            attackInterval = 1.2f,
            attackRange = 1.5f,
            deploymentCost = 14,
            redeployTime = 70f
        };

        // 重装职业 - 高血防，高阻挡
        public static OperatorStats DefenderStats = new OperatorStats
        {
            maxHP = 2400,
            currentHP = 2400,
            attack = 300,
            defense = 400,
            magicResistance = 0,
            blockCount = 3,
            attackInterval = 1.5f,
            attackRange = 1.5f,
            deploymentCost = 18,
            redeployTime = 70f
        };

        // 狙击职业 - 远程物理输出
        public static OperatorStats SniperStats = new OperatorStats
        {
            maxHP = 900,
            currentHP = 900,
            attack = 550,
            defense = 80,
            magicResistance = 0,
            blockCount = 0,
            attackInterval = 1.0f,
            attackRange = 6f,
            deploymentCost = 15,
            redeployTime = 70f
        };

        // 术师职业 - 远程法术输出
        public static OperatorStats CasterStats = new OperatorStats
        {
            maxHP = 850,
            currentHP = 850,
            attack = 600,
            defense = 60,
            magicResistance = 10,
            blockCount = 0,
            attackInterval = 1.6f,
            attackRange = 4f,
            deploymentCost = 19,
            redeployTime = 70f
        };

        // 医疗职业 - 治疗友军
        public static OperatorStats MedicStats = new OperatorStats
        {
            maxHP = 1000,
            currentHP = 1000,
            attack = 400, // 治疗量
            defense = 80,
            magicResistance = 0,
            blockCount = 0,
            attackInterval = 2.85f,
            attackRange = 4f,
            deploymentCost = 15,
            redeployTime = 70f
        };

        // 辅助职业 - 提供增益/减益
        public static OperatorStats SupporterStats = new OperatorStats
        {
            maxHP = 1100,
            currentHP = 1100,
            attack = 280,
            defense = 90,
            magicResistance = 10,
            blockCount = 0,
            attackInterval = 1.9f,
            attackRange = 4f,
            deploymentCost = 16,
            redeployTime = 70f
        };

        // 特种职业 - 特殊机制
        public static OperatorStats SpecialistStats = new OperatorStats
        {
            maxHP = 1300,
            currentHP = 1300,
            attack = 380,
            defense = 110,
            magicResistance = 0,
            blockCount = 1,
            attackInterval = 1.3f,
            attackRange = 1.5f,
            deploymentCost = 13,
            redeployTime = 18f // 特种通常再部署时间短
        };
    }

    // 职业特性系统
    public abstract class OperatorTrait
    {
        public abstract void ApplyTrait(Operator op);
        public abstract void RemoveTrait(Operator op);
    }

    // 先锋特性：击杀敌人回复费用
    public class VanguardTrait : OperatorTrait
    {
        private int costRecoveryPerKill = 1;

        public override void ApplyTrait(Operator op)
        {
            // 监听击杀事件
        }

        public override void RemoveTrait(Operator op)
        {
        }
    }

    // 重装特性：受到物理伤害-20%
    public class DefenderTrait : OperatorTrait
    {
        public override void ApplyTrait(Operator op)
        {
            // 在受伤时减免伤害
        }

        public override void RemoveTrait(Operator op)
        {
        }
    }

    // 狙击特性：优先攻击空中单位
    public class SniperTrait : OperatorTrait
    {
        public override void ApplyTrait(Operator op)
        {
            // 修改目标选择逻辑
        }

        public override void RemoveTrait(Operator op)
        {
        }
    }

    // 术师特性：攻击造成法术伤害
    public class CasterTrait : OperatorTrait
    {
        public override void ApplyTrait(Operator op)
        {
            // 已在攻击类型中处理
        }

        public override void RemoveTrait(Operator op)
        {
        }
    }

    // 医疗特性：不攻击，只治疗
    public class MedicTrait : OperatorTrait
    {
        public override void ApplyTrait(Operator op)
        {
            // 已在攻击类型中处理
        }

        public override void RemoveTrait(Operator op)
        {
        }
    }

    // 辅助特性：减速敌人
    public class SupporterTrait : OperatorTrait
    {
        private float slowAmount = 0.3f;

        public override void ApplyTrait(Operator op)
        {
            // 攻击时给敌人施加减速效果
        }

        public override void RemoveTrait(Operator op)
        {
        }
    }

    // 特种特性：快速再部署
    public class SpecialistTrait : OperatorTrait
    {
        public override void ApplyTrait(Operator op)
        {
            op.currentStats.redeployTime *= 0.3f;
        }

        public override void RemoveTrait(Operator op)
        {
            op.currentStats.redeployTime /= 0.3f;
        }
    }
}

// ============= 示例干员数据创建代码 =============
// 在 Unity 编辑器中，可以通过以下方式创建干员数据：

/*
1. 右键 -> Create -> Tower Defence -> Operator Data
2. 填写干员信息：
   - Operator Name: "夜刀"
   - Operator ID: "char_001"
   - Operator Class: Guard (近卫)
   - Rarity: ThreeStar
   - Attack Type: Physical
   
3. 配置基础属性（使用 GuardStats 预设）
4. 添加技能数据
5. 设置图标和预制体引用

类似的，创建技能数据：
1. 右键 -> Create -> Tower Defence -> Skill Data
2. 填写技能信息：
   - Skill Name: "剑术·破甲"
   - Skill Type: Manual
   - Activation: Charged
   - Cooldown: 15
   - Duration: 10
   - SP Cost: 30
   - Attack Multiplier: 1.5
*/