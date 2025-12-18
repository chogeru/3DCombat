using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// コンボ攻撃担当
/// </summary>
public class ComboSystem : MonoBehaviour
{
    [Header("攻撃コンボ番号"), SerializeField]
    int m_ComboNo = 1;

    [Header("最後にクリックした時間"), SerializeField]
    float m_ClickLastTime = 0f;

    [Header("コンボが途切れるまでの猶予時間"), SerializeField]
    float m_ComboDelay = 0.7f;

    [Header("プレイヤーのアニメーションコントローラー"), SerializeField]
    Animator m_PlayerAnim;

    //クリックされたか判定フラグ
    bool m_InputReserved = false;

    //ゲーム側がOK出してるか判定フラグ
    bool m_CanNextCombo=true;

    /// <summary>
    /// クリックされたときの処理
    /// </summary>
    public void InputAttack()
    {
        m_InputReserved=true;

        //攻撃OKなら
        if(m_CanNextCombo)
        {
            ComboCount();
        }
    }

    /// <summary>
    /// アニメーションから次の攻撃OKと言われた時の処理
    /// </summary>
    /// <param name="anim"></param>
    public void OnComboWindow()
    {
        m_CanNextCombo = true;

        if (m_InputReserved)
        {
            ComboCount();
        }
    }

    /// <summary>
    /// 条件がそろったとき、次の攻撃を出す
    /// </summary>
    public void ComboCount()
    {
        //アニメ前半での誤発動防止
        if (!m_CanNextCombo)
            return;

        m_InputReserved = false;
        m_CanNextCombo = false;

        //加算
        m_ComboNo++;

        if (m_ComboNo > 5) m_ComboNo = 0;

        // 何打目であっても「今クリックされた」という合図を送る
        m_PlayerAnim.SetTrigger("Attack_Combo");

        // 今のコンボ番号をセットする
        m_PlayerAnim.SetInteger("AttackNo", m_ComboNo);

        //クリック時の時間代入
        m_ClickLastTime = Time.time;
    }

    /// <summary>
    /// 一定時間操作がなかったらコンボを終了する
    /// </summary>
    public void ResetCombo(Animator anim)
    {
        //攻撃してないなら何もしない
        if (m_ComboNo == 0)
            return;

        //最後の攻撃から指定時間を超えたら
        if (Time.time - m_ClickLastTime > m_ComboDelay)
        {
            //リセット処理
            m_ComboNo = 0;
            anim.SetInteger("AttackNo", 0);
            anim.SetTrigger("Attack_Break");
        }
    }



    /// <summary>
    /// アニメーションがおわったので、コンボ状態初期化
    /// </summary>
    public void OnAttackEnd()
    {
        m_InputReserved = false;
        m_CanNextCombo = true;
        m_ComboNo = 0;
    }
}
