using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TowerDefence.UI
{
    using Operators;

    // 干员卡片UI
    public class OperatorCard : MonoBehaviour
    {
        [Header("UI组件")]
        public Image operatorIcon;
        public TextMeshProUGUI operatorNameText;
        public TextMeshProUGUI costText;
        public Image costIcon;
        public Button cardButton;
        public Image cooldownOverlay;
        public TextMeshProUGUI cooldownText;

        private OperatorData operatorData;
        private DeploymentManager deploymentManager;  // 改为 DeploymentManager
        private float currentCooldown = 0f;
        private bool isOnCooldown = false;

        public void Initialize(OperatorData data)
        {
            operatorData = data;
            deploymentManager = DeploymentManager.Instance; // 获取单例

            // 设置UI
            if (operatorIcon != null)
                operatorIcon.sprite = data.operatorIcon;

            if (operatorNameText != null)
                operatorNameText.text = data.operatorName;

            if (costText != null)
                costText.text = data.baseStats.deploymentCost.ToString();

            // 绑定点击事件
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnCardClicked);
            }

            UpdateCardState();
        }

        void Update()
        {
            if (isOnCooldown)
            {
                currentCooldown -= Time.deltaTime;

                if (currentCooldown <= 0)
                {
                    isOnCooldown = false;
                    currentCooldown = 0;
                }

                UpdateCooldownDisplay();
            }

            UpdateCardState();
        }

        void OnCardClicked()
        {
            if (CanDeploy())
            {
                // 调用新的部署管理器
                deploymentManager.StartPlacement(operatorData);
                // StartCooldown(); // 部署成功后才开始冷却，这里可以先不做，或者监听部署成功事件
            }
        }

        bool CanDeploy()
        {
            // 检查费用
            return !isOnCooldown &&
                   CostManager.Instance.GetCurrentCost() >= operatorData.baseStats.deploymentCost;
        }

        void UpdateCardState()
        {
            bool canDeploy = CanDeploy();

            if (cardButton != null)
            {
                cardButton.interactable = canDeploy;
            }

            // 更新颜色或透明度
            Color targetColor = canDeploy ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            if (operatorIcon != null)
            {
                operatorIcon.color = targetColor;
            }
        }

        void StartCooldown()
        {
            isOnCooldown = true;
            currentCooldown = operatorData.baseStats.redeployTime;
        }

        void UpdateCooldownDisplay()
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(isOnCooldown);
                cooldownOverlay.fillAmount = currentCooldown / operatorData.baseStats.redeployTime;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(isOnCooldown);
                cooldownText.text = Mathf.CeilToInt(currentCooldown).ToString();
            }
        }
    }
    
    // 干员详情面板
    public class OperatorDetailPanel : MonoBehaviour
    {
        [Header("UI组件")]
        public TextMeshProUGUI operatorNameText;
        public TextMeshProUGUI classText;
        public Image operatorPortrait;
        public TextMeshProUGUI descriptionText;

        [Header("属性显示")]
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI attackText;
        public TextMeshProUGUI defenseText;
        public TextMeshProUGUI blockText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI redeployText;

        [Header("技能显示")]
        public Transform skillContainer;
        public GameObject skillItemPrefab;

        public void ShowOperatorDetails(OperatorData data)
        {
            gameObject.SetActive(true);

            // 基本信息
            if (operatorNameText != null)
                operatorNameText.text = data.operatorName;

            if (classText != null)
                classText.text = GetClassNameChinese(data.operatorClass);

            if (operatorPortrait != null)
                operatorPortrait.sprite = data.operatorIcon;

            if (descriptionText != null)
                descriptionText.text = data.description;

            // 属性信息
            var stats = data.baseStats;
            if (hpText != null)
                hpText.text = stats.maxHP.ToString();
            if (attackText != null)
                attackText.text = stats.attack.ToString();
            if (defenseText != null)
                defenseText.text = stats.defense.ToString();
            if (blockText != null)
                blockText.text = stats.blockCount.ToString();
            if (costText != null)
                costText.text = stats.deploymentCost.ToString();
            if (redeployText != null)
                redeployText.text = stats.redeployTime.ToString("F0") + "秒";

            // 显示技能
            DisplaySkills(data.skills);
        }

        void DisplaySkills(List<SkillData> skills)
        {
            // 清除旧的技能UI
            foreach (Transform child in skillContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建技能项
            foreach (var skill in skills)
            {
                GameObject skillItem = Instantiate(skillItemPrefab, skillContainer);
                var skillUI = skillItem.GetComponent<SkillItemUI>();
                if (skillUI != null)
                {
                    skillUI.SetSkill(skill);
                }
            }
        }

        string GetClassNameChinese(OperatorClass operatorClass)
        {
            switch (operatorClass)
            {
                case OperatorClass.Vanguard: return "先锋";
                case OperatorClass.Guard: return "近卫";
                case OperatorClass.Defender: return "重装";
                case OperatorClass.Sniper: return "狙击";
                case OperatorClass.Caster: return "术师";
                case OperatorClass.Medic: return "医疗";
                case OperatorClass.Supporter: return "辅助";
                case OperatorClass.Specialist: return "特种";
                default: return "未知";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    // 技能项UI
    public class SkillItemUI : MonoBehaviour
    {
        public Image skillIcon;
        public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI skillDescText;
        public TextMeshProUGUI spCostText;
        public TextMeshProUGUI cooldownText;

        public void SetSkill(SkillData skill)
        {
            if (skillIcon != null)
                skillIcon.sprite = skill.skillIcon;

            if (skillNameText != null)
                skillNameText.text = skill.skillName;

            if (skillDescText != null)
                skillDescText.text = skill.description;

            if (spCostText != null)
                spCostText.text = "SP: " + skill.spCost;

            if (cooldownText != null && skill.cooldown > 0)
                cooldownText.text = "冷却: " + skill.cooldown + "秒";
        }
    }

    // 部署点显示
    public class DeploymentPointsUI : MonoBehaviour
    {
        public TextMeshProUGUI pointsText;
        public Image fillBar;
        private OperatorPlacementSystem placementSystem;

        void Start()
        {
            placementSystem = FindObjectOfType<OperatorPlacementSystem>();
        }

        void Update()
        {
            if (placementSystem != null)
            {
                int current = placementSystem.currentDeploymentPoints;
                int max = placementSystem.maxDeploymentPoints;

                if (pointsText != null)
                    pointsText.text = $"{current}/{max}";

                if (fillBar != null)
                    fillBar.fillAmount = (float)current / max;
            }
        }
    }

    // 已部署干员信息显示
    public class DeployedOperatorUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Image operatorIcon;
        public Image hpBar;
        public Image spBar;
        public Button skillButton;
        public Button retreatButton;
        public TextMeshProUGUI hpText;

        private Operator targetOperator;
        private SkillManager skillManager;

        public void SetTarget(Operator op)
        {
            targetOperator = op;
            skillManager = op.GetComponent<SkillManager>();

            if (operatorIcon != null)
                operatorIcon.sprite = op.operatorData.operatorIcon;

            // 绑定按钮
            if (skillButton != null)
            {
                skillButton.onClick.RemoveAllListeners();
                skillButton.onClick.AddListener(OnSkillButtonClicked);
            }

            if (retreatButton != null)
            {
                retreatButton.onClick.RemoveAllListeners();
                retreatButton.onClick.AddListener(OnRetreatButtonClicked);
            }
        }

        void Update()
        {
            if (targetOperator == null || !targetOperator.isActive)
            {
                gameObject.SetActive(false);
                return;
            }

            UpdateHP();
            UpdateSP();
        }

        void UpdateHP()
        {
            if (targetOperator == null) return;

            float hpRatio = (float)targetOperator.currentStats.currentHP /
                           targetOperator.currentStats.maxHP;

            if (hpBar != null)
                hpBar.fillAmount = hpRatio;

            if (hpText != null)
                hpText.text = $"{targetOperator.currentStats.currentHP}/{targetOperator.currentStats.maxHP}";
        }

        void UpdateSP()
        {
            if (skillManager == null || skillManager.CurrentSkill == null) return;

            var skill = skillManager.CurrentSkill;
            float spRatio = skill.CurrentSP / skill.MaxSP;

            if (spBar != null)
            {
                spBar.fillAmount = spRatio;
                spBar.gameObject.SetActive(true);
            }

            if (skillButton != null)
            {
                skillButton.interactable = skill.IsReady &&
                    skill.data.skillType == SkillType.Manual;

                // 技能激活中改变颜色
                var colors = skillButton.colors;
                colors.normalColor = skill.IsActive ? Color.yellow : Color.white;
                skillButton.colors = colors;
            }
        }

        void OnSkillButtonClicked()
        {
            if (skillManager != null)
            {
                skillManager.ActivateSkill();
            }
        }

        void OnRetreatButtonClicked()
        {
            if (targetOperator != null)
            {
                targetOperator.Retreat();
            }
        }
    }

    // 干员选择面板管理器
    public class OperatorSelectionPanel : MonoBehaviour
    {
        [Header("配置")]
        public List<OperatorData> availableOperators;
        public Transform cardContainer;
        public GameObject operatorCardPrefab;

        private OperatorPlacementSystem placementSystem;
        private List<OperatorCard> operatorCards = new List<OperatorCard>();

        void Start()
        {
            placementSystem = FindObjectOfType<OperatorPlacementSystem>();
            InitializeCards();
        }

        void InitializeCards()
        {
            foreach (var operatorData in availableOperators)
            {
                GameObject cardObj = Instantiate(operatorCardPrefab, cardContainer);
                OperatorCard card = cardObj.GetComponent<OperatorCard>();

                if (card != null)
                {
                    card.Initialize(operatorData, placementSystem);
                    operatorCards.Add(card);
                }
            }
        }

        // 添加新干员到队伍
        public void AddOperatorToTeam(OperatorData data)
        {
            if (!availableOperators.Contains(data))
            {
                availableOperators.Add(data);

                GameObject cardObj = Instantiate(operatorCardPrefab, cardContainer);
                OperatorCard card = cardObj.GetComponent<OperatorCard>();

                if (card != null)
                {
                    card.Initialize(data, placementSystem);
                    operatorCards.Add(card);
                }
            }
        }
    }
}