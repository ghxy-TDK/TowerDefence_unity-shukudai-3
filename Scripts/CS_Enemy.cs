using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_Enemy : MonoBehaviour
{
    enum State
    {
        Move = 0,
        Attack = 1,
        Dead = 9,
    }

    [SerializeField] SpriteRenderer mySpriteRenderer;
    private State myState;
    private List<Vector3> myPath;
    [SerializeField] float myMoveSpeed = 10;
    [SerializeField] Animator myAnimator = null;

    [SerializeField] Transform myTransform_HPBar = null;
    [SerializeField] GameObject myObject_HPCanvas = null;

    private CS_Player myTargetPlayer;

    [SerializeField] AudioSource myAudioSource_Attack;

    [Header("Status")]
    [SerializeField] int myStatus_MaxHealth = 1000;
    private int myCurrentHealth;
    [SerializeField] int myStatus_Attack = 200;
    [SerializeField] float myStatus_AttackTime = 0.5f;
    private float myAttackTimer = 0;


    public void Init()
    {
        // 获取路径
        myPath = CS_EnemyManager.Instance.GetPath();
        // 移动到起始点
        this.transform.position = myPath[0];
        myPath.RemoveAt(0);
        // 设置状态为移动
        myState = State.Move;
        myAnimator.SetInteger("State", 0);

        // 初始化生命值
        myCurrentHealth = myStatus_MaxHealth;
        myTransform_HPBar.localScale = Vector3.one;
        myObject_HPCanvas.SetActive(false);

        // 激活敌人
        this.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (myState == State.Move)
        {
            Update_Move();
        }

        Update_Attack();
    }

    private void Update_Attack()
    {

        // 更新攻击计时器
        if (myAttackTimer > 0)
        {
            myAttackTimer -= Time.fixedDeltaTime;
            return;
        }

        // 如果目标敌人已消失，清除目标
        if (myTargetPlayer != null && myTargetPlayer.gameObject.activeSelf == false)
        {
            myTargetPlayer = null;
        }

        // 如果没有目标，遍历敌人列表寻找目标
        if (myTargetPlayer == null)
        {
            List<CS_Player> t_playerList = CS_GameManager.Instance.GetPlayerList();
            foreach (CS_Player f_player in t_playerList)
            {
                if (f_player == null || f_player.gameObject.activeSelf == false ||
                    f_player.GetState() == CS_Player.State.Dead ||
                    f_player.GetState() == CS_Player.State.Arrange)
                {
                    continue;
                }
                if (Vector3.Distance(f_player.transform.position, this.transform.position) < 0.5f)
                {
                    myTargetPlayer = f_player;
                    break;
                }
            }
        }

        // 如果范围内没有敌人，不攻击
        if (myTargetPlayer == null)
        {
            if (myState != State.Dead)
            {
                myState = State.Move;
                myAnimator.SetInteger("State", 0);
            }
            return;
        }

        // 播放音效
        myAudioSource_Attack.Play();

        // 攻击敌人
        myTargetPlayer.TakeDamage(myStatus_Attack);
        myAttackTimer += myStatus_AttackTime;
        myAnimator.SetTrigger("Attack");
        myState = State.Attack;
        myAnimator.SetInteger("State", 1);
    }

    public void Update_Move()
    {
        // 确保至少有一个目标点
        if (myPath == null || myPath.Count <= 0)
        {
            // 隐藏敌人
            this.gameObject.SetActive(false);
            // 通知管理器敌人逃脱
            CS_EnemyManager.Instance.LoseEnemy(this);
            // 扣除生命
            CS_GameManager.Instance.LoseLife();
            return;
        }

        // 获取当前位置和目标位置
        Vector3 t_currentPosition = this.transform.position;
        Vector3 t_targetPosition = myPath[0];

        // 计算移动方向
        Vector3 t_direction = (t_targetPosition - t_currentPosition).normalized;

        // 移动
        Vector3 t_nextPosition = this.transform.position + t_direction * myMoveSpeed * Time.fixedDeltaTime;

        // 检查是否移动超过目标点
        Vector3 t_nextDirection = (t_targetPosition - t_nextPosition).normalized;
        if (t_nextDirection != t_direction)
        {
            // 到达目标点
            t_nextPosition = t_targetPosition;
            // 从列表中移除该点
            myPath.RemoveAt(0);
        }

        // 设置位置
        this.transform.position = t_nextPosition;

        // 更新动画
        // 仅在水平移动时翻转精灵
        if (Mathf.Abs(t_direction.x) > Mathf.Abs(t_direction.y))
        {
            if (t_direction.x > 0)
            {
                mySpriteRenderer.flipX = false;
            }
            else
            {
                mySpriteRenderer.flipX = true;
            }
        }
    }

    public void TakeDamage(int g_damage)
    {
        myCurrentHealth -= g_damage;

        if (myCurrentHealth <= 0)
        {
            myCurrentHealth = 0;
            // 设置为死亡状态
            myState = State.Dead;
            // 隐藏敌人
            this.gameObject.SetActive(false);
            // 通知管理器敌人被击败
            CS_EnemyManager.Instance.LoseEnemy(this);
        }

        // 激活生命值画布
        myObject_HPCanvas.SetActive(true);
        // 更新生命值条UI
        myTransform_HPBar.localScale = new Vector3((float)myCurrentHealth / myStatus_MaxHealth, 1, 1);
    }
}