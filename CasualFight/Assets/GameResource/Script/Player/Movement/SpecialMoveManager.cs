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
    //中
    [SerializeField] SkillLevelData m_MiddleSkill;
    //強
    [SerializeField] SkillLevelData m_StrongSkill;

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

    [Header("ガードブレイク設定")]
    [Header("ガードブレイク時のゲージ減少速度(毎秒)"), SerializeField]
    float m_GuardBreakRecoverySpeed = 5.0f;

    //現在の数値
    private float m_CurrentCharge = 0f;

    //チャージ中判定フラグ(歩くの遅くしたりとかに使う予定)
    private bool m_IsCharging = false;

    //クールタイム中かどうかのフラグ
    private bool m_IsCoolingDown = false;

    //ガードブレイク中かどうかのフラグ
    private bool m_IsGuardBreaking = false;

    public bool IsCharging => m_IsCharging;
    public bool IsGuardBreaking => m_IsGuardBreaking;

    private void Update()
    {
    }

    /// <summary>
    /// ガード成功時などに外部からゲージを加算する
    /// </summary>
    /// <param name="amount"></param>
    public void AddGauge(float amount)
    {
        // ガードブレイク中は加算しない
        if (m_IsGuardBreaking) return;

        m_CurrentCharge += amount;

        if (m_CurrentCharge >= m_MaxChargeTime)
        {
            m_CurrentCharge = m_MaxChargeTime;
            OnGuardBreak();
        }
        else
        {
            m_CurrentCharge = Mathf.Clamp(m_CurrentCharge, 0, m_MaxChargeTime);
        }
        UpdateUI();
    }

    /// <summary>
    /// ガードブレイク発生時の処理
    /// </summary>
    void OnGuardBreak()
    {
        if (m_IsGuardBreaking) return;

        m_IsGuardBreaking = true;
        Debug.Log("ガードブレイク発生！");

        if (m_Animator != null)
        {
            m_Animator.SetTrigger("GuardBreak");
        }

        StartGuardBreakRecoveryAsync().Forget();
    }

    /// <summary>
    /// ガードブレイクからの回復処理
    /// </summary>
    async UniTaskVoid StartGuardBreakRecoveryAsync()
    {
        while (m_CurrentCharge > 0f)
        {
            // インスペクターで設定した速度で減算
            m_CurrentCharge -= m_GuardBreakRecoverySpeed * Time.deltaTime;
            m_CurrentCharge = Mathf.Max(m_CurrentCharge, 0f); // 0以下にはしない

            UpdateUI();
            await UniTask.Yield();
        }

        m_IsGuardBreaking = false;
        Debug.Log("ガードブレイク回復完了");
    }

    /// <summary>
    /// アクションなどによる強制リセット
    /// </summary>
    public void ResetGauge()
    {
        m_CurrentCharge = 0f;
        m_IsGuardBreaking = false;
        UpdateUI();
    }

    /// <summary>
    /// 必殺技処理
    /// </summary>
    void CheckChargeRelease()
    {
        // ガードブレイク中は技を出せないならここで弾く
        if (m_IsGuardBreaking) return;

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
        if (m_SpecialGaugeSlider != null)
        {
            m_SpecialGaugeSlider.value = m_CurrentCharge / m_MaxChargeTime;
        }

        if (m_FillImage == null) return;

        // ガードブレイク中は色を変えるなどの処理を追加しても良いかもしれません
        if (m_IsGuardBreaking)
        {
             // 例: ブレイク中は点滅させるとか、特定の色にするとか
             // ここでは一旦既存ロジックの上書きは最小限に留めますが、
             // MAX状態なので放置すると赤(StrongSkill)などの色になります。
        }

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
