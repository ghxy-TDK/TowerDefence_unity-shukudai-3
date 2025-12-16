#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 格子系统编辑器 - 完整可视化编辑工具
/// 支持点击/拖拽编辑格子类型和路径
/// </summary>
[CustomEditor(typeof(GridManager))]
public class GridEditor : Editor
{
    private GridManager gridManager;
    private bool editMode = false;
    private CellType currentBrushType = CellType.Path;
    private bool isDragging = false;
    private Vector2Int lastEditedCell = new Vector2Int(-1, -1);

    // 预览网格
    private Dictionary<Vector2Int, GameObject> previewCells = new Dictionary<Vector2Int, GameObject>();
    private GameObject previewParent;

    void OnEnable()
    {
        gridManager = (GridManager)target;
    }

    void OnDisable()
    {
        CleanupPreview();
    }

    public override void OnInspectorGUI()
    {
        // 绘制默认Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("═══════════════════════════", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("🎨 格子可视化编辑器", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("═══════════════════════════", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        // 编辑模式切换
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = editMode ? Color.green : Color.cyan;

        string buttonText = editMode ? "✅ 保存并退出编辑" : "✏️ 进入编辑模式";
        if (GUILayout.Button(buttonText, GUILayout.Height(35)))
        {
            if (editMode)
            {
                // 退出编辑模式时保存数据
                SaveCellTypesToManager();
                CleanupPreview();
                editMode = false;
            }
            else
            {
                // 进入编辑模式
                editMode = true;
                CreatePreviewGrid();
                LoadCellTypesFromManager();
            }

            SceneView.RepaintAll();
        }

        GUI.backgroundColor = originalColor;

        if (editMode)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "🖱️ 在 Scene 视图中操作：\n" +
                "• 左键点击/拖拽：绘制选中类型\n" +
                "• 右键点击/拖拽：擦除（设为空地）\n" +
                "• 使用下方按钮选择格子类型\n" +
                "• 完成后点击「保存并退出」保存修改",
                MessageType.Info
            );

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎨 画笔类型", EditorStyles.boldLabel);

            // 画笔类型选择
            EditorGUILayout.BeginVertical("box");

            DrawBrushButton(CellType.Path, "🟡 路径", "设置为敌人路径", Color.yellow);
            DrawBrushButton(CellType.Deployable, "🟢 可部署", "设置为可部署区域", Color.green);
            DrawBrushButton(CellType.Empty, "⚫ 空地", "设置为空地（不可用）", Color.gray);
            DrawBrushButton(CellType.Start, "🔵 起点", "设置为敌人起点", Color.cyan);
            DrawBrushButton(CellType.End, "🔴 终点", "设置为敌人终点", Color.red);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚡ 快速操作", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🗑️ 清空所有", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空所有格子设置吗？", "确定", "取消"))
                {
                    ClearAllCells();
                }
            }
            if (GUILayout.Button("📋 生成路径", GUILayout.Height(25)))
            {
                GeneratePathFromCells();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("➡️ 水平路径模板", GUILayout.Height(25)))
            {
                CreateHorizontalPathTemplate();
            }
            if (GUILayout.Button("〰️ Z字路径模板", GUILayout.Height(25)))
            {
                CreateZigzagPathTemplate();
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("点击「进入编辑模式」开始编辑格子\n编辑完成后记得点击「保存并退出」", MessageType.None);

            // 显示已保存的格子数量
            var savedTypes = serializedObject.FindProperty("savedCellTypes");
            if (savedTypes.arraySize > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"💾 已保存 {savedTypes.arraySize} 个格子配置", EditorStyles.boldLabel);
            }
        }

        // 显示路径点信息
        if (gridManager.pathPoints.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"📋 当前路径: {gridManager.pathPoints.Count} 个点", EditorStyles.boldLabel);
        }
    }

    void DrawBrushButton(CellType type, string label, string tooltip, Color color)
    {
        Color originalBg = GUI.backgroundColor;

        if (currentBrushType == type)
        {
            GUI.backgroundColor = color;
        }

        if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Height(30)))
        {
            currentBrushType = type;
            Repaint();
        }

        GUI.backgroundColor = originalBg;
    }

    void OnSceneGUI()
    {
        if (!editMode) return;

        // 获取控制ID，防止选择其他物体
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;

        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        // 显示当前画笔提示
        Handles.BeginGUI();
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 5, 5);

        string brushInfo = $"🎨 画笔: {GetBrushTypeName(currentBrushType)}";
        Vector2 size = style.CalcSize(new GUIContent(brushInfo));
        GUI.Box(new Rect(10, 10, size.x, size.y), brushInfo, style);
        Handles.EndGUI();

        // 处理鼠标输入
        HandleMouseInput(e);

        // 绘制预览网格
        DrawPreviewGrid();
    }

    void HandleMouseInput(Event e)
    {
        if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1))
        {
            isDragging = true;
            lastEditedCell = new Vector2Int(-1, -1);
            e.Use();
        }

        if (e.type == EventType.MouseUp)
        {
            isDragging = false;
            e.Use();
        }

        if (e.type == EventType.MouseDrag || (e.type == EventType.MouseDown && isDragging))
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Vector2Int cellPos = WorldToCell(hit.point);

                if (IsValidCell(cellPos) && cellPos != lastEditedCell)
                {
                    lastEditedCell = cellPos;

                    Undo.RecordObject(gridManager, "Edit Cell");

                    if (e.button == 0)  // 左键 - 绘制
                    {
                        SetCellTypeInPreview(cellPos, currentBrushType);
                    }
                    else if (e.button == 1)  // 右键 - 擦除
                    {
                        SetCellTypeInPreview(cellPos, CellType.Empty);
                    }

                    EditorUtility.SetDirty(gridManager);
                    e.Use();
                }
            }
        }
    }

    void CreatePreviewGrid()
    {
        CleanupPreview();

        previewParent = new GameObject("GridPreview_EDITOR");
        previewParent.hideFlags = HideFlags.HideAndDontSave;

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                CreatePreviewCell(x, y);
            }
        }

        Debug.Log($"✅ 创建预览网格: {gridManager.gridWidth}x{gridManager.gridHeight}");
    }

    void LoadCellTypesFromManager()
    {
        // 从 GridManager 的保存数据加载格子类型
        if (!Application.isPlaying)
        {
            // 编辑模式下，从 savedCellTypes 加载
            var savedTypes = serializedObject.FindProperty("savedCellTypes");

            for (int i = 0; i < savedTypes.arraySize; i++)
            {
                var element = savedTypes.GetArrayElementAtIndex(i);
                int x = element.FindPropertyRelative("x").intValue;
                int y = element.FindPropertyRelative("y").intValue;
                int typeInt = element.FindPropertyRelative("cellType").intValue;
                CellType type = (CellType)typeInt;

                if (previewCells.ContainsKey(new Vector2Int(x, y)))
                {
                    SetCellTypeInPreview(new Vector2Int(x, y), type);
                }
            }

            Debug.Log($"✅ 加载了 {savedTypes.arraySize} 个保存的格子类型");
        }
    }

    void SaveCellTypesToManager()
    {
        Undo.RecordObject(gridManager, "Save Cell Types");

        // 清空旧数据
        var savedTypes = serializedObject.FindProperty("savedCellTypes");
        savedTypes.ClearArray();

        // 保存所有非空格子
        int count = 0;
        foreach (var kvp in previewCells)
        {
            Vector2Int pos = kvp.Key;
            Color color = kvp.Value.GetComponent<Renderer>().material.color;
            CellType type = GetTypeFromColor(color);

            if (type != CellType.Empty)
            {
                savedTypes.InsertArrayElementAtIndex(count);
                var element = savedTypes.GetArrayElementAtIndex(count);
                element.FindPropertyRelative("x").intValue = pos.x;
                element.FindPropertyRelative("y").intValue = pos.y;
                element.FindPropertyRelative("cellType").intValue = (int)type;
                count++;
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(gridManager);

        Debug.Log($"✅ 保存了 {count} 个格子类型到 GridManager");
    }

    void CreatePreviewCell(int x, int y)
    {
        Vector3 worldPos = CellToWorld(x, y);

        GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cell.name = $"PreviewCell_{x}_{y}";
        cell.transform.SetParent(previewParent.transform);
        cell.transform.position = worldPos;
        cell.transform.localScale = new Vector3(gridManager.cellSize * 0.95f, 0.1f, gridManager.cellSize * 0.95f);
        cell.hideFlags = HideFlags.HideAndDontSave;

        // 设置材质
        Renderer renderer = cell.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = GetColorForType(CellType.Empty);
        renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        previewCells[new Vector2Int(x, y)] = cell;
    }

    void SetCellTypeInPreview(Vector2Int pos, CellType type)
    {
        if (previewCells.ContainsKey(pos))
        {
            GameObject cell = previewCells[pos];
            Renderer renderer = cell.GetComponent<Renderer>();
            renderer.material.color = GetColorForType(type);
        }
    }

    void DrawPreviewGrid()
    {
        // 绘制网格线
        Handles.color = new Color(1, 1, 1, 0.3f);

        for (int x = 0; x <= gridManager.gridWidth; x++)
        {
            Vector3 start = CellToWorld(x, 0);
            Vector3 end = CellToWorld(x, gridManager.gridHeight);
            start.x -= gridManager.cellSize / 2f;
            end.x -= gridManager.cellSize / 2f;
            Handles.DrawLine(start, end);
        }

        for (int y = 0; y <= gridManager.gridHeight; y++)
        {
            Vector3 start = CellToWorld(0, y);
            Vector3 end = CellToWorld(gridManager.gridWidth, y);
            start.z -= gridManager.cellSize / 2f;
            end.z -= gridManager.cellSize / 2f;
            Handles.DrawLine(start, end);
        }

        // 绘制格子坐标
        Handles.color = Color.white;
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 10;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                Vector3 pos = CellToWorld(x, y);
                pos.y += 0.2f;
                Handles.Label(pos, $"{x},{y}", labelStyle);
            }
        }
    }

    void CleanupPreview()
    {
        if (previewParent != null)
        {
            DestroyImmediate(previewParent);
        }

        previewCells.Clear();
    }

    void ClearAllCells()
    {
        foreach (var kvp in previewCells)
        {
            Renderer renderer = kvp.Value.GetComponent<Renderer>();
            renderer.material.color = GetColorForType(CellType.Empty);
        }

        Undo.RecordObject(gridManager, "Clear All Cells");
        gridManager.pathPoints.Clear();
        EditorUtility.SetDirty(gridManager);
    }

    void GeneratePathFromCells()
    {
        Undo.RecordObject(gridManager, "Generate Path");

        List<Vector2Int> pathCells = new List<Vector2Int>();
        Vector2Int? startCell = null;
        Vector2Int? endCell = null;

        // 收集所有路径格子
        foreach (var kvp in previewCells)
        {
            Color color = kvp.Value.GetComponent<Renderer>().material.color;
            Vector2Int pos = kvp.Key;

            if (ColorMatch(color, GetColorForType(CellType.Start)))
            {
                startCell = pos;
            }
            else if (ColorMatch(color, GetColorForType(CellType.End)))
            {
                endCell = pos;
            }
            else if (ColorMatch(color, GetColorForType(CellType.Path)))
            {
                pathCells.Add(pos);
            }
        }

        // 构建路径
        gridManager.pathPoints.Clear();

        if (startCell.HasValue)
        {
            gridManager.pathPoints.Add(startCell.Value);
        }

        // 简单排序（从左到右，从下到上）
        pathCells.Sort((a, b) => {
            if (a.x != b.x) return a.x.CompareTo(b.x);
            return a.y.CompareTo(b.y);
        });

        gridManager.pathPoints.AddRange(pathCells);

        if (endCell.HasValue)
        {
            gridManager.pathPoints.Add(endCell.Value);
        }

        EditorUtility.SetDirty(gridManager);

        Debug.Log($"✅ 生成路径: {gridManager.pathPoints.Count} 个点");
    }

    void CreateHorizontalPathTemplate()
    {
        int midY = gridManager.gridHeight / 2;

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            CellType type = CellType.Path;
            if (x == 0) type = CellType.Start;
            else if (x == gridManager.gridWidth - 1) type = CellType.End;

            SetCellTypeInPreview(new Vector2Int(x, midY), type);
        }

        GeneratePathFromCells();
    }

    void CreateZigzagPathTemplate()
    {
        int startY = 2;
        int endY = gridManager.gridHeight - 3;
        int midX = gridManager.gridWidth / 2;

        // 左边向右
        for (int x = 0; x < midX; x++)
        {
            SetCellTypeInPreview(new Vector2Int(x, startY), x == 0 ? CellType.Start : CellType.Path);
        }

        // 向上
        for (int y = startY; y <= endY; y++)
        {
            SetCellTypeInPreview(new Vector2Int(midX, y), CellType.Path);
        }

        // 向右
        for (int x = midX; x < gridManager.gridWidth; x++)
        {
            SetCellTypeInPreview(new Vector2Int(x, endY), x == gridManager.gridWidth - 1 ? CellType.End : CellType.Path);
        }

        GeneratePathFromCells();
    }

    // 工具函数
    Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(
            x * gridManager.cellSize - (gridManager.gridWidth * gridManager.cellSize) / 2f + gridManager.cellSize / 2f,
            0.05f,
            y * gridManager.cellSize - (gridManager.gridHeight * gridManager.cellSize) / 2f + gridManager.cellSize / 2f
        );
    }

    Vector2Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x + (gridManager.gridWidth * gridManager.cellSize) / 2f) / gridManager.cellSize - 0.5f);
        int y = Mathf.RoundToInt((worldPos.z + (gridManager.gridHeight * gridManager.cellSize) / 2f) / gridManager.cellSize - 0.5f);
        return new Vector2Int(x, y);
    }

    bool IsValidCell(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.gridWidth && pos.y >= 0 && pos.y < gridManager.gridHeight;
    }

    Color GetColorForType(CellType type)
    {
        switch (type)
        {
            case CellType.Start: return Color.cyan;
            case CellType.End: return Color.red;
            case CellType.Path: return Color.yellow;
            case CellType.Deployable: return Color.green;
            default: return new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

    string GetBrushTypeName(CellType type)
    {
        switch (type)
        {
            case CellType.Start: return "起点";
            case CellType.End: return "终点";
            case CellType.Path: return "路径";
            case CellType.Deployable: return "可部署";
            default: return "空地";
        }
    }

    CellType GetTypeFromColor(Color color)
    {
        if (ColorMatch(color, GetColorForType(CellType.Start))) return CellType.Start;
        if (ColorMatch(color, GetColorForType(CellType.End))) return CellType.End;
        if (ColorMatch(color, GetColorForType(CellType.Path))) return CellType.Path;
        if (ColorMatch(color, GetColorForType(CellType.Deployable))) return CellType.Deployable;
        return CellType.Empty;
    }

    bool ColorMatch(Color a, Color b)
    {
        return Vector4.Distance(new Vector4(a.r, a.g, a.b, a.a), new Vector4(b.r, b.g, b.b, b.a)) < 0.1f;
    }
}
#endif