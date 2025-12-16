using UnityEngine;
using TowerDefence.Data;

public class DeploymentManager : MonoBehaviour
{
    public static DeploymentManager Instance;

    [Header("配置")]
    public Material ghostMaterial;

    private OperatorData selectedData;
    private GameObject ghostObject;
    private bool isDeploying = false;

    void Awake() { Instance = this; }

    // UI 调用的入口 (替代 Set B OperatorUI 中的调用)
    public void StartPlacement(OperatorData data)
    {
        if (isDeploying) CancelDeployment();

        selectedData = data;
        isDeploying = true;

        // 创建 Ghost
        CreateGhost();
    }

    void CreateGhost()
    {
        // 这里只是简单的用一个 Sprite 显示 Ghost
        ghostObject = new GameObject("Ghost");
        var sr = ghostObject.AddComponent<SpriteRenderer>();
        sr.sprite = selectedData.operatorIcon;
        sr.material = ghostMaterial;
        ghostObject.transform.localScale = Vector3.one * 0.8f;
    }

    void Update()
    {
        if (!isDeploying || ghostObject == null) return;

        // 获取鼠标下的格子 (Set A GridInputHandler)
        if (GridInputHandler.Instance != null)
        {
            GridCell cell = GridInputHandler.Instance.GetCurrentHoveredCell();

            if (cell != null)
            {
                ghostObject.transform.position = cell.worldPosition;
                ghostObject.SetActive(true);

                // 点击部署
                if (Input.GetMouseButtonDown(0))
                {
                    TryDeploy(cell);
                }
            }
            else
            {
                ghostObject.SetActive(false);
            }
        }

        // 右键取消
        if (Input.GetMouseButtonDown(1)) CancelDeployment();
    }

    void TryDeploy(GridCell cell)
    {
        // 1. 检查费用
        if (!CostManager.Instance.CanAfford(selectedData.baseStats.deploymentCost)) return;

        // 2. 检查地形匹配
        if (!cell.CanDeploy(selectedData.positionType)) return;

        // 3. 部署
        CostManager.Instance.DeductCost(selectedData.baseStats.deploymentCost);

        GameObject opObj = Instantiate(selectedData.operatorPrefab, cell.worldPosition, Quaternion.identity);
        Operator opScript = opObj.GetComponent<Operator>();
        if (opScript == null) opScript = opObj.AddComponent<Operator>(); // 防止 Prefab 没挂脚本

        opScript.Initialize(selectedData);
        cell.SetOccupied(opScript);
        opScript.occupiedCell = cell;

        CancelDeployment();
    }

    public void CancelDeployment()
    {
        isDeploying = false;
        if (ghostObject != null) Destroy(ghostObject);
        selectedData = null;
    }
}