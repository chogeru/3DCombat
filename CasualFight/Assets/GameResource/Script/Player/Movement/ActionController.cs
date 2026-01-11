using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの入力制御
/// </summary>
public class ActionController : MonoBehaviour
{
    [Header("プレイヤーのアニメーター"), SerializeField]
    Animator m_Animator;

    [Header("コンボシステム"), SerializeField]
    ComboSystem m_ComboSystem;

    // クリックの経過時間
    float m_ClickTimer = 0f;

    // マウス左ボタンを押し続けているか
    bool m_IsPressing = false;

    // ガード中フラグ
    bool m_InGuardMode = false;

    [Header("ガード移行までの長押し時間"), SerializeField]
    float m_ChangeGuardTime = 0.2f;

    /// <summary>
    /// ガード中かどうかを取得
    /// </summary>
    public bool IsGuarding => m_InGuardMode;

    private void Update()
    {
        // 左クリック開始
        if (Input.GetMouseButtonDown(0))
        {
            m_IsPressing = true;
            m_InGuardMode = false;
            m_ClickTimer = 0f;
        }

        if (m_IsPressing)
        {
            m_ClickTimer += Time.deltaTime;

            // 一定時間経過かつ攻撃中でないならガードモードへ
            if (m_ClickTimer >= m_ChangeGuardTime)
            {
                m_InGuardMode = true;
            }

            // マウスを離したとき
            if (Input.GetMouseButtonUp(0))
            {
                // まだガードに移行していなければ通常攻撃
                if (!m_InGuardMode)
                {
                    m_ComboSystem.InputAttack();
                }

                // リセット
                m_IsPressing = false;
                m_InGuardMode = false;
                m_ClickTimer = 0f;
            }
        }
        else
        {
            // ボタンが押されていない間は確実にリセット
            m_IsPressing = false;
            m_InGuardMode = false;
            m_ClickTimer = 0f;
        }

        // 一定時間操作がなければコンボをリセット
        if (m_ComboSystem != null)
        {
            m_ComboSystem.ResetCombo(m_Animator);
        }
    }
}
