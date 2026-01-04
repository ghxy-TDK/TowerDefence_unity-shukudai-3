using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class CS_MapGenerator : MonoBehaviour
{

    public enum PositionType
    {
        Distance,
        BottomLeftAndTopRight,
        BottomLeftAndDistance,
    }

    static float TILE_SIZE_MULTIPLIER = 1;
    static float TILE_SIZE = 1;

    [SerializeField] GameObject[] myGridPrefab = null;
    [SerializeField] List<GameObject> myGrids = new List<GameObject>();
    [Header("Position")]
    [SerializeField] int myColumns = 16;
    [SerializeField] int myRows = 9;
    [SerializeField] float myPositionY = 10;
    [SerializeField] PositionType myPositionType = PositionType.Distance;
    [SerializeField] Vector2 myPosition_Distance = Vector2.one;
    [SerializeField] Vector2 myPosition_BottomLeft = new Vector2(-8, -4);
    [SerializeField] Vector2 myPosition_TopRight = new Vector2(8, 4);
    [Header("Size")]
    [SerializeField] Vector2 mySiza_RandomRange = new Vector2(1, 1.5f);
    [Header("Parents")]
    [SerializeField] Transform[] myTransformParents = null;

    [ContextMenu("Create Grid")]
    void CreateGrid()
    {

        Debug.Log("创建网格!");

        Clear();

        // 计算左下角位置和间距
        Vector2 t_BL = Vector2.zero;
        Vector2 t_distance = Vector2.one;
        if (myPositionType == PositionType.Distance)
        {
            t_BL = new Vector2((myColumns - 1) * -0.5f * myPosition_Distance.x,
                                (myRows - 1) * -0.5f * myPosition_Distance.y);
            t_distance = myPosition_Distance;
        }
        else if (myPositionType == PositionType.BottomLeftAndTopRight)
        {
            t_BL = myPosition_BottomLeft;
            t_distance = new Vector2((myPosition_TopRight.x - myPosition_BottomLeft.x) / (myColumns - 1),
                                      (myPosition_TopRight.y - myPosition_BottomLeft.y) / (myRows - 1));
        }
        else if (myPositionType == PositionType.BottomLeftAndDistance)
        {
            t_BL = myPosition_BottomLeft;
            t_distance = myPosition_Distance;
        }

        // 创建网格
        for (int i = 0; i < myRows; i++)
        {
            for (int j = 0; j < myColumns; j++)
            {
                // 获取一个预制体
                GameObject t_prefab = myGridPrefab[Random.Range(0, myGridPrefab.Length)];

                // 计算位置
                Vector3 t_position = new Vector3(t_distance.x * j + t_BL.x, myPositionY, t_distance.y * i + t_BL.y);

                GameObject t_grid = PrefabUtility.InstantiatePrefab(t_prefab, this.transform) as GameObject;
                t_grid.transform.position = t_position;
                t_grid.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 5), 0);
                myGrids.Add(t_grid); // 添加到列表
                t_grid.name = "(" + i + ")(" + j + ") " + t_prefab.name; // 命名
            }
        }
    }

    [ContextMenu("Clear Grid")]
    void Clear()
    {
        // 清除现有网格
        foreach (GameObject t_grid in myGrids)
        {
            DestroyImmediate(t_grid);
        }
        myGrids.Clear();
    }

    [ContextMenu("Snap")]
    void Snap()
    {
        Debug.Log("对齐");
        foreach (Transform f_parent in myTransformParents)
        {
            for (int i = 0; i < f_parent.childCount; i++)
            {
                Vector3 f_postion = f_parent.GetChild(i).position;
                f_postion.x = Mathf.Round(f_postion.x * TILE_SIZE_MULTIPLIER) * TILE_SIZE;
                f_postion.z = Mathf.Round(f_postion.z * TILE_SIZE_MULTIPLIER) * TILE_SIZE;
                f_postion.y = 0;
                f_parent.GetChild(i).position = f_postion;
            }
        }
    }

    [ContextMenu("Random Rotation")]
    void RandomRotation()
    {
        Debug.Log("随机旋转");
        foreach (Transform f_parent in myTransformParents)
        {
            for (int i = 0; i < f_parent.childCount; i++)
            {
                f_parent.GetChild(i).rotation = Quaternion.Euler(0, 90 * Random.Range(0, 5), 0);
            }
        }
    }

    [ContextMenu("Random Size")]
    void RandomSize()
    {
        Debug.Log("随机大小");
        foreach (Transform f_parent in myTransformParents)
        {
            for (int i = 0; i < f_parent.childCount; i++)
            {
                f_parent.GetChild(i).localScale =
                    Vector3.one * Random.Range(mySiza_RandomRange.x, mySiza_RandomRange.y);
            }
        }
    }
}
#endif