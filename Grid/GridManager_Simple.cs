using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 超简化格子系统 - 不依赖外部材质，一定能看见！
/// </summary>
public class GridManager_Simple : MonoBehaviour
{
    public static GridManager_Simple Instance;

    [Header("格子尺寸")]
    public int gridWidth = 12;
    public int gridHeight = 8;
    public float cellSize = 1f;

    private GridCell[,] grid;
    private GameObject gridParent;
    private List<Vector2Int> pathPoints = new List<Vector2Int>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("===== GridManager_Simple 开始初始化 =====");
        GenerateGrid();
        SetupPath();
        Debug.Log("===== 初始化完成！应该能看到格子了 =====");
    }

    void GenerateGrid()
    {
        gridParent = new GameObject("Grid");
        gridParent.transform.SetParent(transform);

        grid = new GridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(
                    x * cellSize - (gridWidth * cellSize) / 2f + cellSize / 2f,
                    0.05f,  // 稍微抬高避免 Z-fighting
                    y * cellSize - (gridHeight * cellSize) / 2f + cellSize / 2f
                );

                grid[x, y] = new GridCell(x, y, worldPos);
                CreateCellVisual(grid[x, y]);
            }
        }

        Debug.Log($"✅ 创建了 {gridWidth * gridHeight} 个格子");
    }

    void CreateCellVisual(GridCell cell)
    {
        // 创建立方体
        GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cellObj.name = $"Cell_{cell.x}_{cell.y}";
        cellObj.transform.SetParent(gridParent.transform);
        cellObj.transform.position = cell.worldPosition;
        cellObj.transform.localScale = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);

        // 获取 Renderer
        Renderer renderer = cellObj.GetComponent<Renderer>();

        // 创建最简单的 Unlit 材质（一定能看见！）
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 深灰色，完全不透明

        renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        cell.cellObject = cellObj;
    }

    void SetupPath()
    {
        // 创建默认水平路径
        int midY = gridHeight / 2;
        for (int x = 0; x < gridWidth; x++)
        {
            pathPoints.Add(new Vector2Int(x, midY));
        }

        // 设置路径颜色
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector2Int point = pathPoints[i];
            GridCell cell = grid[point.x, point.y];

            Color color;
            if (i == 0)
            {
                color = Color.green;  // 起点
                cell.cellType = CellType.Start;
            }
            else if (i == pathPoints.Count - 1)
            {
                color = Color.red;    // 终点
                cell.cellType = CellType.End;
            }
            else
            {
                color = new Color(1f, 0.6f, 0.2f); // 橙色路径
                cell.cellType = CellType.Path;
            }

            // 更新颜色
            Renderer renderer = cell.cellObject.GetComponent<Renderer>();
            renderer.material.color = color;
        }

        // 设置可部署区域
        SetDeployableArea();

        Debug.Log($"✅ 路径已设置: {pathPoints.Count} 个点");
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

                    // 设置为浅绿色
                    Renderer renderer = cell.cellObject.GetComponent<Renderer>();
                    renderer.material.color = new Color(0.5f, 1f, 0.5f, 1f);

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

            if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
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

    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        return null;
    }

    public GridCell GetCellFromWorld(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x + (gridWidth * cellSize) / 2f) / cellSize - 0.5f);
        int y = Mathf.RoundToInt((worldPos.z + (gridHeight * cellSize) / 2f) / cellSize - 0.5f);

        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];

        return null;
    }
}