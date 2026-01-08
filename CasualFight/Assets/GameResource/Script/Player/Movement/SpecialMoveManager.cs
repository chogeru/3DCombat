using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveManager : MonoBehaviour
{
    // 技のデータをひとまとめにする「設計図」
    [Serializable]
    public struct SkillLevelData
    {
        [Header("技名")]
        public string name;
        [Header("どこまで溜めたらこの技発動できるか")]
        public float chargeTime;
        [Header("クールタイムの秒数")]
        public float coolTime;
        [Header("表示用Text")]
        public Text coolTimeText;
        [HideInInspector] public bool isCoolingDown;
    }
    [Header("各レベルのスキル設定")]
    [SerializeField] SkillLevelData m_MiddleSkill; // 中
    [SerializeField] SkillLevelData m_StrongSkill; // 強

    [Header("UIのバー"), SerializeField]
    Slider m_SpecialGaugeSlider;

    [Header("UIのバーのFillの部分"), SerializeField]
    Image m_FillImage;

    [Header("プレイヤーのアニメーター"), SerializeField]
    Animator m_Animator;

    [Header("武器切り替えシステム"), SerializeField]
    WeaponSwitch m_WeaponSwitch;

    [Header("プレイヤーコントローラー"), SerializeField]
    PlayerController m_PC;

    [Header("チャージ設定")]
    [Header("マックスチャージ"), SerializeField]
    float m_MaxChargeTime = 10f;

    //現在の数値
    private float m_CurrentCharge = 0f;

    //チャージ中判定フラグ(歩くの遅くしたりとかに使う予定)
    private bool m_IsCharging = false;

    //クールタイム中かどうかのフラグ
    private bool m_IsCoolingDown = false;

    public bool IsCharging => m_IsCharging;

    private void Update()
    {
    }

    /// <summary>
    /// ガード成功時などに外部からゲージを加算する
    /// </summary>
    /// <param name="amount"></param>
    public void AddGauge(float amount)
    {
        m_CurrentCharge += amount;
        m_CurrentCharge = Mathf.Clamp(m_CurrentCharge, 0, m_MaxChargeTime);
        UpdateUI();
    }

    /// <summary>
    /// アクションなどによる強制リセット
    /// </summary>
    public void ResetGauge()
    {
        m_CurrentCharge = 0f;
        UpdateUI();
    }

    /// <summary>
    /// 必殺技処理
    /// </summary>
    void CheckChargeRelease()
    {
        if (m_CurrentCharge >= m_StrongSkill.chargeTime)
        {
            if (!m_StrongSkill.isCoolingDown)
            {
                Debug.Log($"【{m_StrongSkill.name}】発動！");
                StartCoolTimeAsync(m_StrongSkill).Forget();
            }
            else
            {
                Debug.Log($"{m_StrongSkill.name}はクールタイム中です。");
            }
        }
        else if (m_CurrentCharge >= m_MiddleSkill.chargeTime)
        {
            if (!m_MiddleSkill.isCoolingDown)
            {
                Debug.Log($"【{m_MiddleSkill.name}】発動！");
                StartCoolTimeAsync(m_MiddleSkill).Forget();
            }
            else
            {
                Debug.Log($"{m_MiddleSkill.name}はクールタイム中です。");
            }
        }

        // 離したらゲージをリセット（または徐々に減らす）
        m_CurrentCharge = 0f;
        UpdateUI();
    }

    /// <summary>
    /// UIの更新処理
    /// </summary>
    public void UpdateUI()
    {
        m_SpecialGaugeSlider.value = m_CurrentCharge / m_MaxChargeTime;

        if (m_CurrentCharge >= m_StrongSkill.chargeTime)
        {
            //Lv3
            m_FillImage.color = Color.red;
        }
        else if (m_CurrentCharge >= m_MiddleSkill.chargeTime)
        {
            //Lv2
            m_FillImage.color = Color.yellow;
        }
        else
        {
            //溜め始め
            m_FillImage.color = Color.gray;
        }
    }

    /// <summary>
    /// 指定されたスキルデータでクールタイムを実行する
    /// </summary>
    async UniTask StartCoolTimeAsync(SkillLevelData data)
    {
        // 構造体は値渡しなので、フラグ管理のために参照が必要な場合は注意が必要ですが、
        // ここでは個別にフラグを立てるために ref は使わず、呼び出し元のインスタンス自体を操作する形にします。
        // ※SkillLevelDataをstructにしているため、フィールドとして保持しているものを直接書き換える必要があります。

        if (data.name == m_MiddleSkill.name) m_MiddleSkill.isCoolingDown = true;
        else if (data.name == m_StrongSkill.name) m_StrongSkill.isCoolingDown = true;

        float remainingTime = data.coolTime;

        while (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            remainingTime = Mathf.Max(remainingTime, 0);

            if (data.coolTimeText != null)
            {
                data.coolTimeText.text = remainingTime.ToString("F1");
            }

            await UniTask.Yield();
        }

        if (data.coolTimeText != null)
        {
            data.coolTimeText.text = "";
        }

        if (data.name == m_MiddleSkill.name) m_MiddleSkill.isCoolingDown = false;
        else if (data.name == m_StrongSkill.name) m_StrongSkill.isCoolingDown = false;
    }
}
