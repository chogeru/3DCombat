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
        //攻撃も移動もしていないなら
        if (m_PC.m_MoveInput.sqrMagnitude < 0.01f && !m_CS.m_InputReserved)
        {
            //時間加算
            m_IdleTimer += Time.deltaTime;

            //指定時間を超えたら
            if (m_IdleTimer > m_Timer)
            {
                //アニメーション再生
                PlayStandbyMotion();

                //初期化
                m_IdleTimer = 0f;
            }
        }
        else//動いたら
        {
            //初期化
            m_IdleTimer = 0f;
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
