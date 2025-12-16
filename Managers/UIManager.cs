using UnityEngine;
using UnityEngine.UI;
using TMPro; // 假设你使用 TextMeshPro

/// <summary>
/// UI 管理器 - 负责所有状态信息的显示和更新
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("顶部状态栏")]
    public TextMeshProUGUI lifeText;          // 关卡生命
    public TextMeshProUGUI waveEnemyText;     // 剩余敌人/波次信息

    [Header("部署栏")]
    public TextMeshProUGUI costText;          // 当前费用 (DP)
    public TextMeshProUGUI costTimerText;     // 费用倒计时

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 订阅事件
        if (LevelManager.Instance != null)
        {
            // 由于生命值事件需要修改 LevelManager，这里先订阅，并假设 LevelManager Start 时触发一次
            LevelManager.Instance.OnLifeChanged += UpdateLifeDisplay;
        }
        if (CostManager.Instance != null)
        {
            CostManager.Instance.OnCostChanged += UpdateCostDisplay;
            CostManager.Instance.OnTimerUpdated += UpdateCostTimer;
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyCountChanged += UpdateEnemyCount;
            WaveManager.Instance.OnWaveChanged += UpdateWaveCount;
        }

        // 确保初始显示
        UpdateWaveCount(1, WaveManager.Instance != null ? WaveManager.Instance.waves.Count : 1);
        UpdateEnemyCount(0, 0);
    }

    void UpdateLifeDisplay(int currentLife)
    {
        if (lifeText != null)
        {
            lifeText.text = $"生命: {currentLife}";
        }
    }

    void UpdateCostDisplay(float currentCost)
    {
        if (costText != null)
        {
            costText.text = Mathf.FloorToInt(currentCost).ToString();
        }
    }

    void UpdateCostTimer(float remainingTime)
    {
        if (costTimerText != null)
        {
            if (CostManager.Instance.GetCurrentCost() >= CostManager.Instance.maxCost)
            {
                costTimerText.text = "MAX";
            }
            else
            {
                costTimerText.text = $"{remainingTime:F1}s";
            }
        }
    }

    void UpdateEnemyCount(int remaining, int total)
    {
        // 更新波次敌人计数 (如: 12/20)
        if (waveEnemyText != null)
        {
            string[] parts = waveEnemyText.text.Split('\n');
            string waveInfo = parts[0];
            waveEnemyText.text = $"{waveInfo}\n敌人: {remaining}/{total}";
        }
    }

    void UpdateWaveCount(int currentWave, int totalWaves)
    {
        // 更新波次信息 (如: 波次 1/5)
        if (waveEnemyText != null)
        {
            string enemyInfo = "";
            if (waveEnemyText.text.Contains("敌人:"))
            {
                string[] parts = waveEnemyText.text.Split('\n');
                enemyInfo = parts.Length > 1 ? parts[1] : "敌人: ?";
            }
            waveEnemyText.text = $"波次 {currentWave}/{totalWaves}\n{enemyInfo}";
        }
    }

    void OnDestroy()
    {
        // 取消订阅
        if (LevelManager.Instance != null) LevelManager.Instance.OnLifeChanged -= UpdateLifeDisplay;
        if (CostManager.Instance != null)
        {
            CostManager.Instance.OnCostChanged -= UpdateCostDisplay;
            CostManager.Instance.OnTimerUpdated -= UpdateCostTimer;
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyCountChanged -= UpdateEnemyCount;
            WaveManager.Instance.OnWaveChanged -= UpdateWaveCount;
        }
    }
}