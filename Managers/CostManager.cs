using UnityEngine;
using System; // 引入 System 命名空间以使用 Action

/// <summary>
/// 费用管理器 (DP System)
/// </summary>
public class CostManager : MonoBehaviour
{
    public static CostManager Instance;

    [Header("费用设置")]
    public float maxCost = 99f;          // 最大费用
    public float initialCost = 10f;      // 初始费用
    public float generationRate = 1.0f;  // 费用生成速度 (秒/1点DP)

    // 事件：用于通知UI更新
    public event Action<float> OnCostChanged;
    public event Action<float> OnTimerUpdated;

    // 运行时变量
    private float _current_cost;
    private float _timer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _current_cost = initialCost;
        _timer = generationRate;
        OnCostChanged?.Invoke(_current_cost);
    }

    void Update()
    {
        if (_current_cost < maxCost)
        {
            _timer -= Time.deltaTime;
            OnTimerUpdated?.Invoke(_timer);

            if (_timer <= 0f)
            {
                GainCost(1);
                // 重置计时器，考虑溢出时间
                _timer += generationRate;
            }
        }
    }

    /// <summary>
    /// 消耗费用
    /// </summary>
    public bool DeductCost(float amount)
    {
        if (_current_cost >= amount)
        {
            _current_cost -= amount;
            OnCostChanged?.Invoke(_current_cost);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获得费用
    /// </summary>
    public void GainCost(float amount)
    {
        _current_cost = Mathf.Min(_current_cost + amount, maxCost);
        OnCostChanged?.Invoke(_current_cost);
    }

    /// <summary>
    /// 检查费用是否足够
    /// </summary>
    public bool CanAfford(float amount)
    {
        return _current_cost >= amount;
    }

    public float GetCurrentCost() => _current_cost;
}