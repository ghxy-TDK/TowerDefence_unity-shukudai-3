using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_GameManager : MonoBehaviour
{

    private static CS_GameManager instance = null;
    public static CS_GameManager Instance { get { return instance; } }

    [SerializeField] int myMaxLife = 10;
    private int myCurrentLife;

    [SerializeField] GameObject[] myPlayerPrefabs = null;
    private List<CS_Player> myPlayerList = new List<CS_Player>();

    [SerializeField] GameObject myDirectionObject = null;

    private CS_Player myCurrentPlayer;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        // 初始化总生命值
        myCurrentLife = myMaxLife;
        CS_UIManager.Instance.SetLife(myCurrentLife);

        // 初始化所有玩家单位
        foreach (GameObject f_prefab in myPlayerPrefabs)
        {
            GameObject f_object = Instantiate(f_prefab, this.transform);
            f_object.SetActive(false);
            // 获取玩家脚本
            CS_Player f_player = f_object.GetComponent<CS_Player>();
            // 将脚本添加到列表
            myPlayerList.Add(f_player);
        }

        // 初始化方向指示物体
        myDirectionObject.SetActive(false);
    }

    public void SetMyCurrentPlayer(int g_index)
    {
        // 如果正在设置方向，则不执行任何操作
        if (myDirectionObject.activeSelf == true)
        {
            return;
        }

        myCurrentPlayer = myPlayerList[g_index];
    }

    public void BeginDragPlayer()
    {
        // 如果正在设置方向，则不执行任何操作
        if (myDirectionObject.activeSelf == true)
        {
            return;
        }

        myCurrentPlayer.gameObject.SetActive(true);
        myCurrentPlayer.Arrange();
        myCurrentPlayer.ShowHighlight();

        // 设置慢动作模式
        Time.timeScale = 0.1f;
    }

    public void DragPlayer()
    {
        // 如果正在设置方向，则不执行任何操作
        if (myDirectionObject.activeSelf == true)
        {
            return;
        }

        // 执行射线检测
        RaycastHit t_hit;
        Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(t_ray, out t_hit))
        {
            CS_Tile t_tile = t_hit.collider.gameObject.GetComponentInParent<CS_Tile>();
            if (t_tile != null)
            {
                if (t_tile.GetType() == myCurrentPlayer.GetTileType())
                {
                    Vector3 t_position = t_tile.transform.position;
                    t_position.y = t_hit.point.y;
                    myCurrentPlayer.transform.position = t_position;
                    return;
                }
            }

            myCurrentPlayer.transform.position = t_hit.point;
        }
    }

    public void EndDragPlayer()
    {
        // 如果正在设置方向，则不执行任何操作
        if (myDirectionObject.activeSelf == true)
        {
            return;
        }

        // 隐藏高亮
        myCurrentPlayer.HideHighlight();

        // 执行射线检测
        RaycastHit t_hit;
        Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(t_ray, out t_hit))
        {
            CS_Tile t_tile = t_hit.collider.gameObject.GetComponentInParent<CS_Tile>();
            if (t_tile != null)
            {
                if (t_tile.GetType() == myCurrentPlayer.GetTileType())
                {
                    // 显示方向指示物体
                    myDirectionObject.transform.position = myCurrentPlayer.transform.position;
                    myDirectionObject.SetActive(true);
                    return;
                }
            }
        }

        // 重置当前玩家单位
        myCurrentPlayer.gameObject.SetActive(false);
        myCurrentPlayer = null;
    }

    public void BeginDragDirection()
    {
    }

    public void DragDirection()
    {

        // 执行射线检测
        RaycastHit t_hit;
        Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(t_ray, out t_hit))
        {
            Vector3 t_v2HitPos = new Vector3(t_hit.point.x, 0, t_hit.point.z);
            Vector3 t_v2PlayerPos = new Vector3(myCurrentPlayer.transform.position.x, 0, myCurrentPlayer.transform.position.z);
            if (Vector3.Distance(t_v2HitPos, t_v2PlayerPos) > 1)
            {
                // 计算朝向
                Vector3 t_forward = t_v2HitPos - t_v2PlayerPos;
                if (Mathf.Abs(t_forward.x) > Mathf.Abs(t_forward.z))
                {
                    t_forward.z = 0;
                }
                else
                {
                    t_forward.x = 0;
                }

                // 旋转
                myCurrentPlayer.transform.forward = t_forward;
                // 显示高亮
                myCurrentPlayer.ShowHighlight();
                return;
            }
        }
        // 隐藏高亮
        myCurrentPlayer.HideHighlight();
    }

    public void EndDragDirection()
    {
        // 执行射线检测
        RaycastHit t_hit;
        Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(t_ray, out t_hit))
        {
            Vector3 t_v2HitPos = new Vector3(t_hit.point.x, 0, t_hit.point.z);
            Vector3 t_v2PlayerPos = new Vector3(myCurrentPlayer.transform.position.x, 0, myCurrentPlayer.transform.position.z);
            if (Vector3.Distance(t_v2HitPos, t_v2PlayerPos) > 1)
            {
                // 隐藏高亮
                myCurrentPlayer.HideHighlight();
                // 隐藏方向指示
                myDirectionObject.SetActive(false);
                // 初始化玩家单位
                myCurrentPlayer.Init();
                myCurrentPlayer = null;
                // 恢复正常时间流速
                Time.timeScale = 1f;
                return;
            }
        }
    }

    public void OnClickTile(CS_Tile g_tile)
    {
        if (myCurrentPlayer != null)
        {
            myCurrentPlayer.transform.position = g_tile.transform.position;
            myCurrentPlayer.gameObject.SetActive(true);
            g_tile.Occupy(myCurrentPlayer);
        }
    }

    public void LoseLife()
    {
        myCurrentLife--;
        CS_UIManager.Instance.SetLife(myCurrentLife);

        if (myCurrentLife <= 0)
        {
            CS_UIManager.Instance.ShowPageFail();
            Time.timeScale = 0;
        }
    }

    public List<CS_Player> GetPlayerList()
    {
        return myPlayerList;
    }
}