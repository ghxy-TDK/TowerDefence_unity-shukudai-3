using UnityEngine;
using System;

/// <summary>
/// 关卡管理器 - 管理关卡生命值和游戏状态
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public event Action<int> OnLifeChanged;
    [Header("关卡设置")]
    public int maxLifePoints = 20; // 关卡最大生命 (例如: 20点)

    // 运行时变量
    private int _current_life_points;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _current_life_points = maxLifePoints;
        OnLifeChanged?.Invoke(_current_life_points); // 初始通知
        Debug.Log($"🎮 关卡开始，当前生命值: {_current_life_points}");
    }

    /// <summary>
    /// 扣除关卡生命值 (漏怪)
    /// </summary>
    public void DeductLife(int amount = 1)
    {
        _current_life_points -= amount;
        OnLifeChanged?.Invoke(_current_life_points); // 通知UI
        Debug.Log($"⚠️ 敌人到达终点！关卡生命 -{amount}，剩余: {_current_life_points}");

        if (_current_life_points <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("❌ 任务失败 (Mission Failed)");
        // TODO: 触发游戏结束UI
    }
}