using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 波次管理器 - 控制敌人的生成节奏
/// </summary>
public class WaveManager : MonoBehaviour
{
    /// <summary>
    /// 事件：剩余敌人数量 / 当前波次总敌人数量
    /// </summary>
    public event Action<int, int> OnEnemyCountChanged;

    /// <summary>
    /// 事件：当前波次索引 / 总波次数
    /// </summary>
    public event Action<int, int> OnWaveChanged;

    public static WaveManager Instance;

    [Header("关卡波次配置")]
    [Tooltip("按顺序放入波次数据")]
    public List<WaveData> waves = new List<WaveData>();

    [Header("状态")]
    public bool autoStart = false;

    // ----------------- 私有状态变量 -----------------
    private int _current_wave_index = 0;
    private bool _is_wave_running = false;

    private int _total_enemy_count = 0;       // 当前波次总敌人数量
    private int _remaining_enemy_count = 0;   // 剩余敌人数量（用于 UI）
    private int _enemies_on_field = 0;         // 场上现存敌人数量
    // ------------------------------------------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (autoStart)
        {
            StartLevel();
        }
    }

    /// <summary>
    /// 开始关卡
    /// </summary>
    public void StartLevel()
    {
        if (waves.Count == 0)
        {
            Debug.LogWarning("⚠️ [WaveManager] 没有配置波次数据！");
            return;
        }

        _current_wave_index = 0;
        StartCoroutine(ProcessWaveRoutine());
    }

    /// <summary>
    /// 处理波次的协程
    /// </summary>
    IEnumerator ProcessWaveRoutine()
    {
        // -------- 等待 GridManager 路径初始化 --------
        Debug.Log("等待 GridManager 完成路径初始化...");

        while (GridManager.Instance == null ||
               GridManager.Instance.pathPoints == null ||
               GridManager.Instance.pathPoints.Count == 0)
        {
            yield return null;
        }

        Debug.Log("✅ 路径数据准备就绪，开始生成敌人。");
        // --------------------------------------------

        while (_current_wave_index < waves.Count)
        {
            // 波次开始事件通知
            OnWaveChanged?.Invoke(_current_wave_index + 1, waves.Count);

            WaveData _current_wave = waves[_current_wave_index];
            _is_wave_running = true;

            Debug.Log($"🌊 --- 波次开始: {_current_wave.waveName} ---");

            // -------- 计算当前波次总敌人数量 --------
            _total_enemy_count = 0;
            foreach (var group in _current_wave.enemyGroups)
            {
                _total_enemy_count += group.count;
            }

            _remaining_enemy_count = _total_enemy_count;
            _enemies_on_field = 0;

            // 初始敌人数量通知（UI 初始化）
            OnEnemyCountChanged?.Invoke(_remaining_enemy_count, _total_enemy_count);
            // ----------------------------------------

            // 遍历敌人组
            foreach (var group in _current_wave.enemyGroups)
            {
                if (group.preDelay > 0)
                {
                    yield return new WaitForSeconds(group.preDelay);
                }

                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group);

                    if (i < group.count - 1)
                    {
                        yield return new WaitForSeconds(group.spawnInterval);
                    }
                }
            }

            Debug.Log($"✅ --- 波次结束: {_current_wave.waveName} ---");

            _current_wave_index++;
            yield return new WaitForSeconds(3.0f);
        }

        Debug.Log("🎉 所有波次已完成！");
    }

    /// <summary>
    /// 生成单个敌人
    /// </summary>
    void SpawnEnemy(WaveData.EnemyGroup group)
    {
        if (GridManager.Instance == null) return;

        List<Vector2Int> _path = GridManager.Instance.pathPoints;
        if (_path == null || _path.Count == 0)
        {
            Debug.LogError("❌ 无法生成敌人：路径点列表为空。");
            return;
        }

        Vector2Int _start_grid_pos = _path[0];
        GridCell _start_cell = GridManager.Instance.GetCell(_start_grid_pos.x, _start_grid_pos.y);

        if (_start_cell == null)
        {
            Debug.LogError("❌ 无法生成敌人：起点格子无效。");
            return;
        }

        GameObject _enemy_obj = Instantiate(
            group.enemyPrefab,
            _start_cell.worldPosition,
            Quaternion.identity
        );

        _enemies_on_field++;

        Enemy _enemy_script = _enemy_obj.GetComponent<Enemy>();
        if (_enemy_script != null && group.overrideData != null)
        {
            _enemy_script.data = group.overrideData;
        }
    }

    /// <summary>
    /// 敌人被击败或成功到达终点时调用
    /// </summary>
    public void EnemyDefeated()
    {
        _remaining_enemy_count = Mathf.Max(0, _remaining_enemy_count - 1);
        _enemies_on_field = Mathf.Max(0, _enemies_on_field - 1);

        // 通知 UI 更新剩余敌人数量
        OnEnemyCountChanged?.Invoke(_remaining_enemy_count, _total_enemy_count);

        // TODO：这里可以扩展波次胜利 / 关卡胜利判定逻辑
    }
}
