using UnityEngine;

// 确保你已经在 Operator.cs 或一个独立文件定义了 OperatorPosition 枚举：
// public enum OperatorPosition { HighGround, Ground } 

/// <summary>
/// 单个格子的数据类
/// </summary>
[System.Serializable]
public class GridCell
{
    // 世界坐标位置
    public Vector3 worldPosition;
    // 格子索引
    public int x;
    public int y;
    // 格子类型
    public CellType cellType;
    // 是否被占据
    public bool isOccupied;
    // 占据这个格子的干员
    public Operator occupyingOperator;
    // 格子的GameObject引用（用于可视化）
    public GameObject cellObject;

    // 构造函数
    public GridCell(int x, int y, Vector3 worldPos)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPos;
        this.cellType = CellType.Empty;
        this.isOccupied = false;
        this.occupyingOperator = null;
    }

    /// <summary>
    /// 【替换】判断是否可以根据干员类型部署
    /// </summary>
    public bool CanDeploy(OperatorPosition opType)
    {
        if (isOccupied) return false; // 已被占据，不可部署

        // 地面干员：只能部署在路径格子上
        if (opType == OperatorPosition.Ground)
        {
            return cellType == CellType.Path;
        }
        // 高台干员：只能部署在可部署区格子上
        else // opType == OperatorPosition.HighGround
        {
            return cellType == CellType.Deployable;
        }
    }

    // 判断是否是路径
    public bool IsPath()
    {
        return cellType == CellType.Path;
    }

    // 设置占据状态
    public void SetOccupied(Operator op)
    {
        isOccupied = true;
        occupyingOperator = op;
    }

    // 清除占据
    public void ClearOccupied()
    {
        isOccupied = false;
        occupyingOperator = null;
    }
}

/// <summary>
/// 格子类型枚举
/// </summary>
public enum CellType
{
    Empty,          // 空地（不可部署）
    Deployable,     // 可部署 (高台位)
    Path,           // 敌人路径 (地面位)
    Start,          // 起点
    End             // 终点
}