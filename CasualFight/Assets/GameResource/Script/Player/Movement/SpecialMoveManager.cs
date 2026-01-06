using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveManager : MonoBehaviour
{
    [Header("UIのバー"), SerializeField]
    Slider m_SpecialGaugeSlider;

    [Header("チャージ設定"), SerializeField]
    float m_MaxChargeTime = 10f;

    //現在の数値
    private float m_CurrentCharge = 0f;

    //チャージ中判定フラグ(歩くの遅くしたりとかに使う予定)
    private bool m_IsCharging = false;

    private void Update()
    {
        //スペースキーが押されている間
        if(Input.GetKey(KeyCode.Space))
        {
            StartChange();
        }
        //スペースキー離したら
        else
        {
            StopCharge();
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
        m_IsCharging=true;

        //ゲージの増加
        m_CurrentCharge += Time.deltaTime;
        
        //上限や下限を超えないように
        m_CurrentCharge = Mathf.Clamp(m_CurrentCharge, 0, m_MaxChargeTime);

        UpdateUI();
    }

    /// <summary>
    /// 離した判定処理
    /// </summary>
    void StopCharge()
    {
        m_IsCharging = false;
    }

    /// <summary>
    /// 必殺技処理
    /// </summary>
    void CheckChargeRelease()
    {
        if (m_CurrentCharge >= m_MaxChargeTime)
        {
            Debug.Log("必殺技発動！");
            // 大技の実行メソッドを呼ぶ
        }
        else
        {
            Debug.Log("チャージ不足...");
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
    }
}
