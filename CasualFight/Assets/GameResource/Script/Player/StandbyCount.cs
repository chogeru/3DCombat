using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandbyCount : MonoBehaviour
{
    [Header("何秒放置で再生するか"), SerializeField]
    float m_Timer = 10f;

    [Header("待機時間"), SerializeField]
    float m_IdleTimer = 0f;

    [Space]

    [Header("プレイヤーオブジェクト"), SerializeField]
    PlayerController m_PC;

    [Header("プレイヤーオブジェクト"), SerializeField]
    ComboSystem m_CS;

    [Header("プレイヤーオブジェクト"), SerializeField]
    Animator m_Animator;

    private void Update()
    {
        bool isMoving = m_PC.m_MoveInput.sqrMagnitude > 0.01f;

        //攻撃中かチェック
        bool isAttacking = m_CS.m_InputReserved || m_Animator.GetInteger("AttackNo") > 0;

        //移動も攻撃もしていない完全な待機状態の時だけ
        if (!isMoving && !isAttacking)
        {
            m_IdleTimer += Time.deltaTime;

            if (m_IdleTimer > m_Timer)
            {
                PlayStandbyMotion();
                m_IdleTimer = 0f;
            }
        }
        else
        {
            //移動したあるいは攻撃した瞬間にタイマーを0にリセットする
            m_IdleTimer = 0f;

            // 移動中であればトリガーを送る
            if (isMoving)
            {
                m_Animator.SetTrigger("Moving");
            }
        }
    }

    /// <summary>
    /// ランダムな待機アニメーションを再生させる
    /// </summary>
    void PlayStandbyMotion()
    {
        //待機アニメーションの数
        //ランダムでアニメーションを流すか決める
        float no = Random.Range(0, 1);

        //アニメーション再生
        m_Animator.SetFloat("StandbyIndex", no);
        m_Animator.SetTrigger("Standby");
    }
}
