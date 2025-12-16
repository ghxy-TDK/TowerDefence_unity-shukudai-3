using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 路径可视化器 - 在游戏中显示路径箭头
/// 附加到 GridManager 物体上
/// </summary>
[RequireComponent(typeof(GridManager))]
public class PathVisualizer : MonoBehaviour
{
    [Header("可视化设置")]
    public bool showPathInGame = true;
    public float arrowHeight = 0.3f;
    public float arrowWidth = 0.15f;
    public Color pathLineColor = Color.yellow;
    public Color arrowColor = new Color(1f, 0.8f, 0f, 1f);

    [Header("材质设置")]
    public Material lineMaterial;  // 可选：自定义线条材质

    private GridManager gridManager;
    private GameObject pathParent;
    private List<LineRenderer> pathLines = new List<LineRenderer>();
    private List<GameObject> pathArrows = new List<GameObject>();

    void Start()
    {
        gridManager = GetComponent<GridManager>();

        if (showPathInGame)
        {
            // 等待格子系统初始化完成
            Invoke("CreatePathVisualization", 0.1f);
        }
    }

    /// <summary>
    /// 创建路径可视化
    /// </summary>
    void CreatePathVisualization()
    {
        if (gridManager == null || gridManager.pathPoints.Count < 2)
        {
            Debug.LogWarning("PathVisualizer: 路径点不足，无法创建可视化");
            return;
        }

        // 创建父物体
        pathParent = new GameObject("PathVisualization");
        pathParent.transform.SetParent(transform);

        // 创建路径线条和箭头
        for (int i = 0; i < gridManager.pathPoints.Count - 1; i++)
        {
            Vector2Int current = gridManager.pathPoints[i];
            Vector2Int next = gridManager.pathPoints[i + 1];

            GridCell currentCell = gridManager.GetCell(current.x, current.y);
            GridCell nextCell = gridManager.GetCell(next.x, next.y);

            if (currentCell != null && nextCell != null)
            {
                CreatePathSegment(currentCell.worldPosition, nextCell.worldPosition, i);
            }
        }

        Debug.Log($"✅ 路径可视化已创建: {pathLines.Count} 条线段");
    }

    /// <summary>
    /// 创建路径段（线条 + 箭头）
    /// </summary>
    void CreatePathSegment(Vector3 from, Vector3 to, int index)
    {
        // 创建线条
        GameObject lineObj = new GameObject($"PathLine_{index}");
        lineObj.transform.SetParent(pathParent.transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();

        // 配置 LineRenderer
        if (lineMaterial != null)
        {
            line.material = lineMaterial;
        }
        else
        {
            // 使用默认的 Unlit 材质
            line.material = new Material(Shader.Find("Unlit/Color"));
            line.material.color = pathLineColor;
        }

        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = 2;
        line.startColor = pathLineColor;
        line.endColor = pathLineColor;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        // 设置位置（稍微抬高避免与地面重叠）
        from.y = arrowHeight;
        to.y = arrowHeight;

        line.SetPosition(0, from);
        line.SetPosition(1, to);

        pathLines.Add(line);

        // 创建箭头
        CreateArrow(to, (to - from).normalized, index);
    }

    /// <summary>
    /// 创建箭头
    /// </summary>
    void CreateArrow(Vector3 position, Vector3 direction, int index)
    {
        // 创建箭头容器
        GameObject arrowObj = new GameObject($"Arrow_{index}");
        arrowObj.transform.SetParent(pathParent.transform);
        arrowObj.transform.position = position;

        // 计算箭头的旋转
        arrowObj.transform.rotation = Quaternion.LookRotation(direction);

        // 创建箭头的两个翅膀（使用LineRenderer）
        CreateArrowWing(arrowObj, true);   // 左翅膀
        CreateArrowWing(arrowObj, false);  // 右翅膀

        pathArrows.Add(arrowObj);
    }

    /// <summary>
    /// 创建箭头翅膀
    /// </summary>
    void CreateArrowWing(GameObject parent, bool isLeft)
    {
        GameObject wingObj = new GameObject(isLeft ? "LeftWing" : "RightWing");
        wingObj.transform.SetParent(parent.transform);
        wingObj.transform.localPosition = Vector3.zero;

        LineRenderer wing = wingObj.AddComponent<LineRenderer>();

        // 配置
        if (lineMaterial != null)
        {
            wing.material = lineMaterial;
        }
        else
        {
            wing.material = new Material(Shader.Find("Unlit/Color"));
            wing.material.color = arrowColor;
        }

        wing.startWidth = 0.08f;
        wing.endWidth = 0.08f;
        wing.positionCount = 2;
        wing.startColor = arrowColor;
        wing.endColor = arrowColor;
        wing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        wing.receiveShadows = false;

        // 计算翅膀位置（本地坐标）
        Vector3 tip = Vector3.zero;  // 箭头尖端
        Vector3 base1 = new Vector3(isLeft ? arrowWidth : -arrowWidth, 0, -0.2f);  // 翅膀根部

        wing.SetPosition(0, tip);
        wing.SetPosition(1, base1);
    }

    /// <summary>
    /// 清除路径可视化
    /// </summary>
    public void ClearPathVisualization()
    {
        pathLines.Clear();
        pathArrows.Clear();

        if (pathParent != null)
        {
            Destroy(pathParent);
        }
    }

    /// <summary>
    /// 重新创建路径可视化
    /// </summary>
    public void RefreshPathVisualization()
    {
        ClearPathVisualization();
        CreatePathVisualization();
    }

    /// <summary>
    /// 切换显示状态
    /// </summary>
    public void ToggleVisibility(bool visible)
    {
        showPathInGame = visible;

        if (pathParent != null)
        {
            pathParent.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        ClearPathVisualization();
    }
}