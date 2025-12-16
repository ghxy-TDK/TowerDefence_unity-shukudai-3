using UnityEngine;
using System.Collections.Generic;
using TowerDefence.Operators; // 引用技能命名空间

namespace TowerDefence.Data
{
    [System.Serializable]
    public struct OperatorStats
    {
        public float maxHP;
        public float attack;
        public float defense;
        public float magicResistance;
        public int blockCount;
        public int deploymentCost;
        public float redeployTime;
        public float attackInterval;
        public float attackRange; // 0=自身/阻挡, >0=射程
    }

    public enum OperatorClass
    {
        Vanguard, Guard, Defender, Sniper, Caster, Medic, Supporter, Specialist
    }

    public enum OperatorPosition
    {
        HighGround, // 高台
        Ground      // 地面
    }

    [CreateAssetMenu(fileName = "New Operator", menuName = "Arknights/Operator Data")]
    public class OperatorData : ScriptableObject
    {
        [Header("基本信息")]
        public string operatorName;
        public Sprite operatorIcon;
        public Sprite operatorPortrait; // 立绘
        [TextArea] public string description;

        [Header("职业与部署")]
        public OperatorClass operatorClass;
        public OperatorPosition positionType;
        public GameObject operatorPrefab; // 实际的模型/预制体

        [Header("属性")]
        public OperatorStats baseStats;

        [Header("技能")]
        public List<SkillData> skills;
    }
}