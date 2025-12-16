using UnityEngine;

/// <summary>
/// 格子输入处理器 - 处理鼠标交互
/// </summary>
public class GridInputHandler : MonoBehaviour
{
    public static GridInputHandler Instance;

    [Header("引用")]
    public Camera mainCamera;

    [Header("设置")]
    public bool enableInput = true;
    public bool showDebugInfo = false;

    private GridCell currentHoveredCell;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // 如果场景中已经有实例存在，销毁这个新的物体
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (!enableInput) return;

        HandleMouseInput();
    }

    /// <summary>
    /// 处理鼠标输入 (保持不变)
    /// </summary>
    void HandleMouseInput()
    {
        // 射线检测鼠标位置
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // 获取鼠标悬停的格子
            GridCell cell = GridManager.Instance.GetCellFromWorld(hit.point);

            if (cell != null)
            {
                // 如果是新的格子，更新高亮
                if (cell != currentHoveredCell)
                {
                    currentHoveredCell = cell;
                    OnCellHovered(cell);
                }

                // 检测鼠标点击
                if (Input.GetMouseButtonDown(0))  // 左键
                {
                    OnCellClicked(cell);
                }

                if (Input.GetMouseButtonDown(1))  // 右键
                {
                    OnCellRightClicked(cell);
                }
            }
        }
        else
        {
            // 鼠标不在格子上，取消高亮
            if (currentHoveredCell != null)
            {
                GridManager.Instance.ClearHighlight();
                currentHoveredCell = null;
            }
        }
    }

    /// <summary>
    /// 鼠标悬停在格子上
    /// </summary>
    void OnCellHovered(GridCell cell)
    {
        // 高亮格子
        GridManager.Instance.HighlightCell(cell);

        // 显示调试信息
        if (showDebugInfo)
        {
            string info = $"格子 [{cell.x}, {cell.y}]\n";
            info += $"类型: {cell.cellType}\n";
            // 【修正】: 移除 CanDeploy() 的调用
            info += $"已占据: {cell.isOccupied}";

            Debug.Log(info);
        }
    }

    /// <summary>
    /// 格子被左键点击
    /// </summary>
    void OnCellClicked(GridCell cell)
    {
        // 【修正】: 优先判断是否处于部署模式
        if (DeploymentManager.Instance != null && DeploymentManager.Instance.IsDeploying())
        {
            // 将部署任务交给 DeploymentManager 处理
            DeploymentManager.Instance.TryDeploy(cell);
            return;
        }

        // 非部署模式下的点击逻辑
        if (cell.isOccupied)
        {
            string objName = cell.occupyingOperator != null ? cell.occupyingOperator.operatorName : "未知";
            Debug.Log($"📍 格子 [{cell.x}, {cell.y}] 已被占据: {objName}");

            // TODO: 显示干员信息
            // UIManager.Instance.ShowOperatorInfo(cell.occupyingOperator);
        }
        else
        {
            Debug.Log($"格子 [{cell.x}, {cell.y}] 类型: {cell.cellType} (不可部署或未选择干员)");
        }
    }

    /// <summary>
    /// 格子被右键点击 (保持不变)
    /// </summary>
    void OnCellRightClicked(GridCell cell)
    {
        if (cell.isOccupied && cell.occupyingOperator != null)
        {
            Debug.Log($"↩️ 回撤: {cell.occupyingOperator.operatorName}");

            // TODO: 调用回撤逻辑
            // DeploymentManager.Instance.RetreatOperator(cell.occupyingOperator);
        }
    }

    /// <summary>
    /// 获取当前鼠标悬停的格子 (保持不变)
    /// </summary>
    public GridCell GetCurrentHoveredCell()
    {
        return currentHoveredCell;
    }
}