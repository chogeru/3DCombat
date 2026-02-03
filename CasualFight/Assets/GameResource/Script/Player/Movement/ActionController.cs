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

    [Header("設定管理マネージャー"), SerializeField]
    SettingsManager m_SettingsManager;

    [Header("プレイヤーコントローラー"), SerializeField]
    PlayerController m_PlayerController;

    [Header("PlayerHitController"), SerializeField]
    PlayerHitController m_PHC;

    [Header("AbilityAttackSystem")]
    [SerializeField] AbilityAttackSystem m_AAS;

    // クリックの経過時間
    float m_ClickTimer = 0f;

    // マウス左ボタンを押し続けているか
    bool m_IsPressing = false;



    private void Start()
    {
        if (m_AAS == null && m_PlayerController != null)
        {
            m_AAS = m_PlayerController.GetComponent<AbilityAttackSystem>();
        }
    }

    private void Update()
    {
        // テレポートUIが開いているかどうかチェック
        bool isTeleportUIOpen = TeleportManager.TPInstance != null && TeleportManager.TPInstance.IsUIOpen;

        // 設定画面が開いている、またはプレイヤーが死亡している、またはテレポートUIが開いている場合は入力を受け付けない
        if ((m_SettingsManager != null && m_SettingsManager.IsMenuOpen) || 
            (m_PlayerController != null && m_PlayerController.IsDead) ||
            isTeleportUIOpen)
        {
            // 押しっぱなし状態などが残らないようにリセット
            m_IsPressing = false;
            m_ClickTimer = 0f;
            return;
        }

        // 硬直中は入力を受け付けない
        if (m_PHC != null && m_PHC.IsStunned)
        {
            m_IsPressing = false;
            m_ClickTimer = 0f;
            return;
        }

        // スキル攻撃中（必殺技など）は入力を受け付けない
        if (m_AAS != null && m_AAS.IsSkillActive)
        {
             m_IsPressing = false;
             m_ClickTimer = 0f;
             return;
        }
        // 左クリック開始
        if (Input.GetMouseButtonDown(0))
        {
            m_IsPressing = true;
            m_ClickTimer = 0f;
        }

        if (m_IsPressing)
        {
            m_ClickTimer += Time.deltaTime;

            // マウスを離したとき
            if (Input.GetMouseButtonUp(0))
            {
                // 通常攻撃
                 m_ComboSystem.InputAttack();

                // リセット
                m_IsPressing = false;
                m_ClickTimer = 0f;
            }
        }
        else
        {
            // ボタンが押されていない間は確実にリセット
            m_IsPressing = false;
            m_ClickTimer = 0f;
        }

        // 一定時間操作がなければコンボをリセット
        if (m_ComboSystem != null)
        {
            m_ComboSystem.ResetCombo(m_Animator);
        }
    }
}
