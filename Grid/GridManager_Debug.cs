using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 格子系统管理器 - 调试版本
/// 添加了大量日志输出来排查问题
/// </summary>
public class GridManager_Debug : MonoBehaviour
{
    public static GridManager_Debug Instance;

    [Header("格子尺寸设置")]
    public int gridWidth = 12;
    public int gridHeight = 8;
    public float cellSize = 1f;

    [Header("可视化设置")]
    public bool showGrid = true;
    public Material cellMaterial;

    [Header("颜色设置")]
    public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color deployableColor = new Color(0.5f, 0.8f, 0.5f, 1f);
    public Color pathColor = new Color(0.8f, 0.6f, 0.4f, 1f);
    public Color startColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color endColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    [Header("路径点")]
    public List<Vector2Int> pathPoints = new List<Vector2Int>();

    private GridCell[,] grid;
    private GameObject gridParent;

    void Awake()
    {
        Debug.Log("===== GridManager_Debug Awake =====");

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ Instance 已设置");
        }
        else
        {
            Debug.LogError("❌ 已存在 GridManager_Debug 实例！");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("===== GridManager_Debug Start =====");
        Debug.Log($"Show Grid: {showGrid}");
        Debug.Log($"Cell Material: {(cellMaterial != null ? cellMaterial.name : "NULL")}");

        GenerateGrid();
        SetupPath();

        Debug.Log("===== GridManager_Debug 初始化完成 =====");
    }

    void GenerateGrid()
    {
        Debug.Log($">>> 开始生成格子: {gridWidth} x {gridHeight}, 格子大小: {cellSize}");

        // 创建父物体
        gridParent = new GameObject("Grid");
        gridParent.transform.SetParent(transform);
        Debug.Log($"✅ Grid 父物体已创建: {gridParent.name}");

        // 初始化数组
        grid = new GridCell[gridWidth, gridHeight];
        Debug.Log($"✅ 格子数组已初始化: {grid.Length} 个格子");

        int createdCount = 0;

        // 生成每个格子
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(
                    x * cellSize - (gridWidth * cellSize) / 2f + cellSize / 2f,
                    0,
                    y * cellSize - (gridHeight * cellSize) / 2f + cellSize / 2f
                );

                grid[x, y] = new GridCell(x, y, worldPos);

                if (showGrid)
                {
                    CreateCellVisual(grid[x, y]);
                    createdCount++;
                }
            }
        }

        Debug.Log($"✅ 创建了 {createdCount} 个格子可视化对象");
        Debug.Log($"✅ Grid 父物体子对象数量: {gridParent.transform.childCount}");
    }

    void CreateCellVisual(GridCell cell)
    {
        // 创建立方体（改用Cube，更容易看见）
        GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cellObj.name = $"Cell_{cell.x}_{cell.y}";
        cellObj.transform.SetParent(gridParent.transform);

        // 抬高一点避免Z-fighting
        Vector3 pos = cell.worldPosition;
        pos.y = 0.05f;
        cellObj.transform.position = pos;

        // 设置为扁平的立方体
        cellObj.transform.localScale = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);

        // 移除默认碰撞体
        Collider collider = cellObj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        // 添加BoxCollider（Cube自带，但我们调整大小）
        BoxCollider boxCollider = cellObj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = cellObj.AddComponent<BoxCollider>();
        }

        // 设置材质和颜色
        Renderer renderer = cellObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat;
            if (cellMaterial != null)
            {
                mat = new Material(cellMaterial);
            }
            else
            {
                // 如果没有材质，创建一个简单的 Unlit 材质
                Debug.LogWarning($"⚠️ Cell Material 为空，使用 Unlit/Color");
                mat = new Material(Shader.Find("Unlit/Color"));
            }

            // 设置初始颜色（深灰色）
            mat.color = emptyColor;

            // 确保材质启用了渲染
            mat.SetFloat("_Mode", 0); // Opaque mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;

            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Debug.Log($"✅ Cell [{cell.x},{cell.y}] 材质已设置，颜色: {mat.color}");
        }

        cell.cellObject = cellObj;
    }

    void SetupPath()
    {
        Debug.Log($">>> 设置路径，路径点数量: {pathPoints.Count}");

        // 如果没有路径点，创建默认路径
        if (pathPoints.Count == 0)
        {
            Debug.Log("创建默认水平路径...");
            int midY = gridHeight / 2;
            for (int x = 0; x < gridWidth; x++)
            {
                pathPoints.Add(new Vector2Int(x, midY));
            }
            Debug.Log($"✅ 默认路径已创建: {pathPoints.Count} 个点");
        }

        // 设置路径格子类型
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector2Int point = pathPoints[i];

            if (IsValidCell(point.x, point.y))
            {
                GridCell cell = grid[point.x, point.y];

                if (i == 0)
                {
                    cell.cellType = CellType.Start;
                }
                else if (i == pathPoints.Count - 1)
                {
                    cell.cellType = CellType.End;
                }
                else
                {
                    cell.cellType = CellType.Path;
                }

                UpdateCellColor(cell);
            }
        }

        // 设置可部署区域
        SetDeployableArea();

        Debug.Log("✅ 路径设置完成");
    }

    void SetDeployableArea()
    {
        int deployableCount = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridCell cell = grid[x, y];

                if (cell.cellType == CellType.Empty && IsNearPath(x, y))
                {
                    cell.cellType = CellType.Deployable;
                    UpdateCellColor(cell);
                    deployableCount++;
                }
            }
        }

        Debug.Log($"✅ 设置了 {deployableCount} 个可部署格子");
    }

    bool IsNearPath(int x, int y)
    {
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];

            if (IsValidCell(newX, newY))
            {
                CellType type = grid[newX, newY].cellType;
                if (type == CellType.Path || type == CellType.Start || type == CellType.End)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void UpdateCellColor(GridCell cell)
    {
        if (cell.cellObject == null) return;

        Renderer renderer = cell.cellObject.GetComponent<Renderer>();
        if (renderer == null) return;

        Color color;
        switch (cell.cellType)
        {
            case CellType.Deployable:
                color = deployableColor;
                break;
            case CellType.Path:
                color = pathColor;
                break;
            case CellType.Start:
                color = startColor;
                break;
            case CellType.End:
                color = endColor;
                break;
            default:
                color = emptyColor;
                break;
        }

        renderer.material.color = color;
    }

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public GridCell GetCell(int x, int y)
    {
        if (IsValidCell(x, y))
        {
            return grid[x, y];
        }
        return null;
    }

    public GridCell GetCellFromWorld(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x + (gridWidth * cellSize) / 2f) / cellSize - 0.5f);
        int y = Mathf.RoundToInt((worldPos.z + (gridHeight * cellSize) / 2f) / cellSize - 0.5f);

        if (IsValidCell(x, y))
        {
            return grid[x, y];
        }

        return null;
    }

    void OnDrawGizmos()
    {
        if (!showGrid || grid == null) return;

        // 绘制格子边框
        Gizmos.color = Color.white;
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(
                x * cellSize - (gridWidth * cellSize) / 2f,
                0,
                -(gridHeight * cellSize) / 2f
            );
            Vector3 end = new Vector3(
                x * cellSize - (gridWidth * cellSize) / 2f,
                0,
                (gridHeight * cellSize) / 2f
            );
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = new Vector3(
                -(gridWidth * cellSize) / 2f,
                0,
                y * cellSize - (gridHeight * cellSize) / 2f
            );
            Vector3 end = new Vector3(
                (gridWidth * cellSize) / 2f,
                0,
                y * cellSize - (gridHeight * cellSize) / 2f
            );
            Gizmos.DrawLine(start, end);
        }
    }
}