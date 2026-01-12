using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuardSystem : MonoBehaviour
{
    [Header("UI設定")]
    [Header("ガードゲージのスライダー"), SerializeField]
    Slider m_GuardGaugeSlider;

    [Header("ガードゲージのFill"), SerializeField]
    Image m_FillImage;

    [Header("プレイヤーのアニメーター"), SerializeField]
    Animator m_Animator;

    [Header("ガードブレイク設定")]
    [Header("マックスゲージ量"), SerializeField]
    float m_MaxGauge = 10f;

    [Header("ガードブレイク時のゲージ減少速度(毎秒)"), SerializeField]
    float m_GuardBreakRecoverySpeed = 5.0f;

    [Header("アニメーション再生の最低保証時間"), SerializeField]
    float m_MinGuardBreakTime = 1.0f;

    [Header("ガード成功時のゲージ加算量"), SerializeField]
    float m_GuardChargeAmount = 1.0f;

    // 現在の数値
    private float m_CurrentGauge = 0f;

    // ガードブレイク中かどうかのフラグ
    private bool m_IsGuardBreaking = false;

    public bool IsGuardBreaking => m_IsGuardBreaking;

    /// <summary>
    /// ガードが可能かどうか（ヨロケ中でなく、かつゲージが完全に回復していること）
    /// </summary>
    public bool CanGuard => !m_IsGuardBreaking && m_CurrentGauge <= 0f;

    /// <summary>
    /// ガード成功時に呼ばれる
    /// </summary>
    public void OnGuardSuccess()
    {
        AddGauge(m_GuardChargeAmount);
        Debug.Log($"ガード成功！ガードゲージを {m_GuardChargeAmount} 加算しました。");
    }

    /// <summary>
    /// ガードゲージを加算する（ガードブレイクの蓄積）
    /// </summary>
    /// <param name="amount"></param>
    public void AddGauge(float amount)
    {
        // ガードブレイク中は加算しない
        if (m_IsGuardBreaking) return;

        m_CurrentGauge += amount;

        if (m_CurrentGauge >= m_MaxGauge)
        {
            m_CurrentGauge = m_MaxGauge;
            OnGuardBreak();
        }
        else
        {
            m_CurrentGauge = Mathf.Clamp(m_CurrentGauge, 0, m_MaxGauge);
        }
        UpdateUI();
    }

    /// <summary>
    /// ガードブレイク発生時の処理
    /// </summary>
    private void OnGuardBreak()
    {
        if (m_IsGuardBreaking) return;

        m_IsGuardBreaking = true;
        Debug.Log("ガードブレイク発生！");

        if (m_Animator != null)
        {
            // ガードブレイクアニメーションへ遷移
            m_Animator.Play("ARPG_Samurai_Guard_B_Break", 0,0);
        }

        StartGuardBreakRecoveryAsync().Forget();
    }

    /// <summary>
    /// ガードブレイクからの自動回復処理
    /// </summary>
    private async UniTaskVoid StartGuardBreakRecoveryAsync()
    {
        // 1. まずはヨロケ（移動不能）の最低再生時間を待つ
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_MinGuardBreakTime));
        
        // 移動フラグのみ先に解除
        m_IsGuardBreaking = false;
        Debug.Log("ガードブレイク：ヨロケ終了、移動可能になりました（ただしゲージ回復までガード不可）");

        // 2. その後、ゲージが 0 になるまで徐々に減らす（この間は CanGuard が false になる）
        while (m_CurrentGauge > 0f)
        {
            m_CurrentGauge -= m_GuardBreakRecoverySpeed * Time.deltaTime;
            m_CurrentGauge = Mathf.Max(m_CurrentGauge, 0f);

            UpdateUI();
            await UniTask.Yield();
        }

        Debug.Log("ガードブレイク：ゲージ全回復によりガード可能になりました");
    }

    /// <summary>
    /// 強制リセット
    /// </summary>
    public void ResetGauge()
    {
        m_CurrentGauge = 0f;
        m_IsGuardBreaking = false;
        UpdateUI();
    }

    /// <summary>
    /// UIの更新処理
    /// </summary>
    public void UpdateUI()
    {
        if (m_GuardGaugeSlider != null)
        {
            m_GuardGaugeSlider.value = m_CurrentGauge / m_MaxGauge;
        }

        if (m_FillImage == null) return;

        // ガードブレイク中は色を変えるなどの処理
        if (m_IsGuardBreaking)
        {
             m_FillImage.color = Color.red; // ブレイク中
        }
        else
        {
             m_FillImage.color = Color.gray; // 通常
        }
    }
}
