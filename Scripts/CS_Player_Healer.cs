using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_Player_Healer : CS_Player
{

    [Header("Healer")]
    private CS_Player myTargetPlayer;

    protected override void Update_Attack()
    {
        // 更新攻击计时器
        if (myAttackTimer > 0)
        {
            myAttackTimer -= Time.fixedDeltaTime;
            return;
        }

        // 如果目标玩家已消失，清除目标
        if (myTargetPlayer != null && myTargetPlayer.gameObject.activeSelf == false)
        {
            myTargetPlayer = null;
        }

        // 如果没有目标，遍历玩家列表寻找目标
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

                // 如果玩家生命值已满，不将其设为目标
                if (f_player.GetHealthPercent() >= 1)
                {
                    continue;
                }

                if (CheckInRange(f_player.transform) == true)
                {
                    myTargetPlayer = f_player;
                    break;
                }
            }
        }

        // 如果范围内没有目标，不攻击
        if (myTargetPlayer == null)
        {
            return;
        }

        // 如果目标玩家生命值已满，清除目标
        if (myTargetPlayer.GetHealthPercent() >= 1)
        {
            myTargetPlayer = null;
            return;
        }

        // 如果目标移出范围，停止治疗该目标
        if (CheckInRange(myTargetPlayer.transform) == false)
        {
            myTargetPlayer = null;
            return;
        }

        // 播放音效
        myAudioSource_Attack.Play();

        // 播放特效
        myEffect.Kill();
        myEffect.transform.position = myTargetPlayer.transform.position;
        myEffect.gameObject.SetActive(true);

        // 治疗目标（使用负值伤害进行治疗）
        myTargetPlayer.TakeDamage(myStatus_Attack * -1);
        myAttackTimer += myStatus_AttackTime;
        myAnimator.SetTrigger("Attack");
    }
}