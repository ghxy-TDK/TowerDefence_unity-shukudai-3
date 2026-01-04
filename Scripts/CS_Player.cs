using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_Player : MonoBehaviour
{
    public enum State
    {
        Idle = 0,
        Attack = 1,
        Dead = 9,
        Arrange = 10,
    }

    private State myState = State.Arrange;
    [SerializeField] Transform myRangeParent = null;
    [SerializeField] Transform myRotateTransform = null;
    private CS_Enemy myTargetEnemy;

    [SerializeField] protected Animator myAnimator = null;

    [SerializeField] Transform myTransform_HPBar = null;

    [SerializeField] protected GameObject myEffectPrefab = null;
    protected CS_Effect myEffect = null;

    [SerializeField] protected AudioSource myAudioSource_Attack;

    [Header("Status")]
    [SerializeField] CS_Tile.Type myTileType = CS_Tile.Type.Ground;
    [SerializeField] int myStatus_MaxHealth = 2400;
    private int myCurrentHealth;
    [SerializeField] protected int myStatus_Attack = 700;
    [SerializeField] protected float myStatus_AttackTime = 0.5f;
    protected float myAttackTimer = 0;

    public void Arrange()
    {
        myState = State.Arrange;
    }

    public void Init()
    {
        myState = State.Idle;
        // 隐藏高亮
        HideHighlight();
        // 面向相机
        FaceCamera();
        // 初始化生命值
        myCurrentHealth = myStatus_MaxHealth;
        myTransform_HPBar.localScale = Vector3.one;
        // 初始化特效
        if (myEffect == null)
        {
            myEffect = Instantiate(myEffectPrefab).GetComponent<CS_Effect>();
            myEffect.Kill();
        }
    }

    private void FixedUpdate()
    {
        Debug.Log(myState);
        if (myState == State.Arrange || myState == State.Dead)
        {
            return;
        }
        Update_Attack();
    }

    private void Update()
    {
        if (myState == State.Arrange)
        {
            FaceCamera();
        }
    }

    private void FaceCamera()
    {
        // 面向相机
        myRotateTransform.rotation = Quaternion.identity;
    }

    public void ShowHighlight()
    {
        for (int i = 0; i < myRangeParent.childCount; i++)
        {
            myRangeParent.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void HideHighlight()
    {
        for (int i = 0; i < myRangeParent.childCount; i++)
        {
            myRangeParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    protected virtual void Update_Attack()
    {
        // 更新攻击计时器
        if (myAttackTimer > 0)
        {
            myAttackTimer -= Time.fixedDeltaTime;
            return;
        }

        // 如果敌人已消失，清除目标
        if (myTargetEnemy != null && myTargetEnemy.gameObject.activeSelf == false)
        {
            myTargetEnemy = null;
        }

        // 如果没有目标，遍历敌人列表寻找目标
        if (myTargetEnemy == null)
        {
            List<CS_Enemy> t_enemyList = CS_EnemyManager.Instance.GetEnemyList();
            foreach (CS_Enemy f_enemy in t_enemyList)
            {
                if (CheckInRange(f_enemy.transform) == true)
                {
                    myTargetEnemy = f_enemy;
                    break;
                }
            }
        }

        // 如果范围内没有敌人，不攻击
        if (myTargetEnemy == null)
        {
            return;
        }

        // 如果敌人移出范围，停止攻击该敌人
        if (CheckInRange(myTargetEnemy.transform) == false)
        {
            myTargetEnemy = null;
            Debug.Log("超出范围");
            return;
        }

        // 播放音效
        myAudioSource_Attack.Play();

        // 播放特效
        myEffect.Kill();
        myEffect.transform.position = myTargetEnemy.transform.position;
        myEffect.gameObject.SetActive(true);

        // 攻击敌人
        myTargetEnemy.TakeDamage(myStatus_Attack);
        myAttackTimer += myStatus_AttackTime;
        myAnimator.SetTrigger("Attack");
    }

    protected bool CheckInRange(Transform g_transform)
    {
        Vector3 t_position = g_transform.position;

        for (int i = 0; i < myRangeParent.childCount; i++)
        {
            Vector3 t_rangeCenter = myRangeParent.GetChild(i).position;
            if (t_position.x > t_rangeCenter.x - 0.5f && t_position.x < t_rangeCenter.x + 0.5f &&
                t_position.y > t_rangeCenter.y - 0.5f && t_position.y < t_rangeCenter.y + 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public void TakeDamage(int g_damage)
    {
        myCurrentHealth -= g_damage;

        if (myCurrentHealth <= 0)
        {
            myCurrentHealth = 0;
            // 设置为死亡状态
            myState = State.Dead;
            // 隐藏单位
            this.gameObject.SetActive(false);
        }

        if (myCurrentHealth > myStatus_MaxHealth)
        {
            myCurrentHealth = myStatus_MaxHealth;
        }

        // 更新生命值条UI
        myTransform_HPBar.localScale = new Vector3(GetHealthPercent(), 1, 1);
    }

    public float GetHealthPercent()
    {
        return (float)myCurrentHealth / myStatus_MaxHealth;
    }

    public CS_Tile.Type GetTileType()
    {
        return myTileType;
    }

    public State GetState()
    {
        return myState;
    }
}