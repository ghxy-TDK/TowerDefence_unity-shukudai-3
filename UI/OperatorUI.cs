using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 引入新的数据命名空间
using TowerDefence.Data;

// 假设 CostManager 和 DeploymentManager 在全局命名空间或 Unity 项目的约定命名空间中
// 如果它们在 TowerDefence.Managers 中，请添加 using TowerDefence.Managers;

namespace TowerDefence.UI
{
    // =====================================================================
    // 干员卡片UI (OperatorCard)
    // 负责单个卡片的显示、点击、费用检查和冷却计时
    // =====================================================================
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

        private float currentCooldown = 0f;
        private bool isOnCooldown = false;

        // **核心改动 1：Initialize 不再接收 OperatorPlacementSystem**
        public void Initialize(OperatorData data)
        {
            operatorData = data;

            // 设置UI
            if (operatorIcon != null)
                operatorIcon.sprite = data.operatorIcon;

            if (operatorNameText != null)
                operatorNameText.text = data.operatorName;

            // **核心改动 2：从新数据结构读取费用**
            if (costText != null)
                costText.text = data.baseStats.deploymentCost.ToString();

            // 绑定点击事件
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(OnCardClicked);
            }

            // **核心改动 3：订阅 CostManager 事件以实时检查费用**
            if (CostManager.Instance != null)
            {
                CostManager.Instance.OnCostChanged += UpdateCardState;
                UpdateCardState(CostManager.Instance.GetCurrentCost()); // 初始检查
            }

            UpdateCardVisuals();
        }

        void OnDestroy()
        {
            // 取消订阅，防止内存泄漏
            if (CostManager.Instance != null)
            {
                CostManager.Instance.OnCostChanged -= UpdateCardState;
            }
        }

        void Update()
        {
            // 处理冷却时间 (Set B 特性)
            if (isOnCooldown)
            {
                currentCooldown -= Time.deltaTime;

                if (currentCooldown <= 0f)
                {
                    currentCooldown = 0f;
                    isOnCooldown = false;
                    // 冷却结束后需要重新检查费用状态
                    UpdateCardState(CostManager.Instance.GetCurrentCost());
                }
                UpdateCardVisuals();
            }
        }

        // 基于费用和冷却更新卡片状态
        public void UpdateCardState(float currentCost)
        {
            if (operatorData == null) return;

            bool canAfford = CostManager.Instance != null && currentCost >= operatorData.baseStats.deploymentCost;

            // 按钮可交互 = 有费用 && 不在冷却中
            bool interactable = canAfford && !isOnCooldown;
            cardButton.interactable = interactable;

            UpdateCardVisuals();
        }

        // 更新视觉效果
        void UpdateCardVisuals()
        {
            // 冷却视觉效果
            if (cooldownOverlay != null)
            {
                // 使用 redeployTime 作为最大冷却时间
                cooldownOverlay.fillAmount = isOnCooldown ? currentCooldown / operatorData.baseStats.redeployTime : 0f;
                cooldownOverlay.gameObject.SetActive(isOnCooldown);
            }

            if (cooldownText != null)
            {
                cooldownText.text = isOnCooldown ? Mathf.Ceil(currentCooldown).ToString() : "";
                cooldownText.gameObject.SetActive(isOnCooldown);
            }

            // 费用不足时的灰度效果
            if (operatorIcon != null)
            {
                bool isInteractable = cardButton.interactable;
                Color color = isInteractable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1.0f);
                operatorIcon.color = color;
            }
        }

        // **核心改动 4：点击处理，调用 DeploymentManager**
        void OnCardClicked()
        {
            if (DeploymentManager.Instance != null && !isOnCooldown)
            {
                // 启动部署流程，将干员数据传给管理器
                DeploymentManager.Instance.StartPlacement(operatorData);

                // 启动冷却计时 (Set B 特性：优先实现新功能)
                StartCooldown();
            }
        }

        // 启动冷却
        public void StartCooldown()
        {
            currentCooldown = operatorData.baseStats.redeployTime;
            isOnCooldown = true;
            // 立即更新状态，禁用卡片
            UpdateCardState(CostManager.Instance.GetCurrentCost());
        }
    }

    // =====================================================================
    // 干员UI列表管理器 (OperatorUI)
    // 负责实例化卡片列表
    // =====================================================================
    public class OperatorUI : MonoBehaviour
    {
        [Header("配置")]
        // **核心改动 5：使用新的 OperatorData 类型列表**
        public List<OperatorData> availableOperators;
        public Transform cardContainer;
        public GameObject operatorCardPrefab;

        // **核心改动 6：移除 OperatorPlacementSystem 的引用**
        // private OperatorPlacementSystem placementSystem; 

        private List<OperatorCard> operatorCards = new List<OperatorCard>();

        void Start()
        {
            // 移除查找 placementSystem 的代码
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
                    // **核心改动 7：调用更新后的 Initialize 方法**
                    card.Initialize(operatorData);
                    operatorCards.Add(card);
                }
            }
        }

        // 添加新干员到队伍 (Set B 特性：保留)
        public void AddOperatorToTeam(OperatorData data)
        {
            if (!availableOperators.Contains(data))
            {
                availableOperators.Add(data);

                GameObject cardObj = Instantiate(operatorCardPrefab, cardContainer);
                OperatorCard card = cardObj.GetComponent<OperatorCard>();

                if (card != null)
                {
                    // **核心改动 8：调用更新后的 Initialize 方法**
                    card.Initialize(data);
                    operatorCards.Add(card);
                }
            }
        }
    }
}