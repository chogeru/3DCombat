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

    //移動トリガーを一度だけ送るためのフラグ
    bool m_IsMovingAnimator = false;

    private void Update()
    {
        bool isMoving = m_PC.m_MoveInput.sqrMagnitude > 0.01f;

        //攻撃中かチェック (!m_PC.m_IsAttack も考慮して二重チェック)
        bool isAttacking = m_PC.m_IsAttack || m_CS.m_InputReserved || m_Animator.GetInteger("AttackNo") > 0;
        
        // 武器を抜いているかチェック
        bool isWeaponDrawn = m_PC.GetComponent<WeaponSwitch>() != null && m_PC.GetComponent<WeaponSwitch>().IsWeaponDrawn;

        //移動も攻撃もしておらず、かつ刀をしまっている完全な待機状態の時だけ
        if (!isMoving && !isAttacking && !isWeaponDrawn)
        {
            m_IdleTimer += Time.deltaTime;

            if (m_IdleTimer > m_Timer)
            {
                PlayStandbyMotion();
                m_IdleTimer = 0f;
            }

            //停止したのでフラグを戻す
            m_IsMovingAnimator = false;
        }
        else
        {
            //移動した・攻撃した・あるいはチャージを開始した瞬間にタイマーを0にリセットする
            m_IdleTimer = 0f;

            // 移動中かつ攻撃中でなければ一度だけ CrossFade を送る
            if (isMoving && !isAttacking && !m_IsMovingAnimator)
            {
                m_Animator.CrossFade("Movement", 0.1f, 0, 0f);
                m_IsMovingAnimator = true;
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
        m_Animator.CrossFade("Angry", 0.1f);
    }
}
