using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveManager : MonoBehaviour
{
    [Header("UIのバー"), SerializeField]
    Slider m_SpecialGaugeSlider;

    [Header("UIのバーのFillの部分"), SerializeField]
    Image m_FillImage;

    [Header("プレイヤーのアニメーター"), SerializeField]
    Animator m_Animator;

    [Header("チャージ設定")]
    [Header("マックスチャージ"), SerializeField]
    float m_MaxChargeTime = 10f;

    [Header("弱チャージ"), SerializeField]
    float m_ChargeWeak = 0.01f;

    [Header("中チャージ"), SerializeField]
    float m_ChargeMiddle = 4f;

    [Header("強チャージ"), SerializeField]
    float m_ChargeStrong = 7f;

    //現在の数値
    private float m_CurrentCharge = 0f;

    //チャージ中判定フラグ(歩くの遅くしたりとかに使う予定)
    private bool m_IsCharging = false;
    public bool IsCharging => m_IsCharging;

    private void Update()
    {
        //スペースキーが押されている間
        if (Input.GetKey(KeyCode.Space))
        {
            StartChange();
            m_CurrentCharge += Time.deltaTime;
        }
        //スペースキー離したら
        else
        {
            StopCharge();
        }

        //上限や下限を超えないように
        m_CurrentCharge = Mathf.Clamp(m_CurrentCharge, 0, m_MaxChargeTime);

        // AbilityChargeパラメータの更新 (0.2秒以上でtrue)
        if (m_Animator != null)
        {
            m_Animator.SetBool("AbilityCharge", m_IsCharging && m_CurrentCharge >= 0.2f);
        }

        //スペースキーが離されたとき
        if (Input.GetKeyUp(KeyCode.Space))
        {
            CheckChargeRelease();
        }
    }

    /// <summary>
    /// ゲージの増加処理
    /// </summary>
    void StartChange()
    {
        m_IsCharging = true;

        //ゲージの増加
        m_CurrentCharge += Time.deltaTime;

        //上限や下限を超えないように
        m_CurrentCharge = Mathf.Clamp(m_CurrentCharge, 0, m_MaxChargeTime);

        UpdateUI();
    }

    /// <summary>
    /// 離した判定処理
    /// </summary>
    public void StopCharge()
    {
        m_IsCharging = false;
        
        if (m_Animator != null)
        {
            m_Animator.SetBool("AbilityCharge", false);
        }
    }

    /// <summary>
    /// アクションなどによる強制停止
    /// </summary>
    public void StopChargeAbility()
    {
        m_CurrentCharge = 0f;
        StopCharge();
        UpdateUI();
    }

    /// <summary>
    /// 必殺技処理
    /// </summary>
    void CheckChargeRelease()
    {
        if (m_CurrentCharge >= m_ChargeStrong)
        {
            Debug.Log("必殺技発動！");
        }
        else if(m_CurrentCharge>= m_ChargeMiddle)
        {
            Debug.Log("中技発動");
        }
        else if(m_CurrentCharge>= m_ChargeWeak)
        {
            Debug.Log("弱技発動");
        }

            // 離したらゲージをリセット（または徐々に減らす）
            m_CurrentCharge = 0f;
        UpdateUI();
    }

    /// <summary>
    /// UIの更新処理
    /// </summary>
    void UpdateUI()
    {
        m_SpecialGaugeSlider.value = m_CurrentCharge / m_MaxChargeTime;

        if (m_CurrentCharge >= m_ChargeStrong)
        {
            //Lv3
            m_FillImage.color = Color.red;
        }
        else if (m_CurrentCharge >= m_ChargeMiddle)
        {
            //Lv2
            m_FillImage.color = Color.yellow;
        }
        else if (m_CurrentCharge >= m_ChargeWeak)
        {
            //Lv1
            m_FillImage.color = Color.white;
        }
        else
        {
            //溜め始め
            m_FillImage.color = Color.gray;
        }
    }
}
