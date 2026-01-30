using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AbilityAttackSystem : MonoBehaviour
{
    [System.Serializable]
    class SkillData
    {
        [Header("クールタイムの秒数")]
        public float m_CoolTime;
        [Header("表示用Text")]
        public Text m_CoolTimeText;
        [Header("表示用UI")]
        public Image m_KeyCodeImage;
        [HideInInspector] public bool m_IsCoolingDown;
    }

    [Header("アニメーション"), SerializeField]
    Animator m_Animator;

    [Header("アビリティアニメーション名"), SerializeField]
    string m_SpeedSlash = "SpeedSlash";

    [Header("アルティメットアニメーション名"), SerializeField]
    string m_UltraSlash = "UltraSlash";

    [Header("中攻撃(アビリティ攻撃)"), SerializeField]
    SkillData m_Ability;

    [Header("必殺技"), SerializeField]
    SkillData m_Ult;

    [Header("ステータス"), SerializeField]
    float m_CurrentEnergy;
    [SerializeField]
    float m_MaximumEnergy = 100f;
    [SerializeField]
    Slider m_EnergySlider;

    [Header("武器切り替え"), SerializeField]
    WeaponSwitch m_WeaponSwitch;

    [Header("プレイヤーコントローラー"), SerializeField]
    PlayerController m_PC;

    //Ultが発動できるか判定
    bool m_IsUlt = false;

    private void Update()
    {
        //ゲージがMaxになったら
        if (!m_IsUlt && m_CurrentEnergy == m_MaximumEnergy)
        {
            m_IsUlt = true;
        }

        // 【追加】イベント中や会話中は入力をブロックするが、クールタイム処理（UniTask）は動き続ける
        // これにより、UIが表示されていなくても裏でクールダウンは進行する
        if (GameStateManager.Instance != null)
        {
            var state = GameStateManager.Instance.CurrentState;
            if (state == GameStateManager.GameState.Event || state == GameStateManager.GameState.Dialogue)
            {
                return;
            }
        }

        // ダッシュ中は発動不可
        bool isDashing = m_PC != null && m_PC.m_IsDash;

        //Eキーを押したらアビリティ発動
        if (Input.GetKeyDown(KeyCode.E) && !m_Ability.m_IsCoolingDown && m_WeaponSwitch.IsWeaponDrawn && !isDashing)
        {
            AbilityAttack();
        }

        //Qキーを押したら必殺技発動
        if (Input.GetKeyDown(KeyCode.Q) && !m_Ult.m_IsCoolingDown && m_IsUlt && m_WeaponSwitch.IsWeaponDrawn && !isDashing)
        {
            UltimateAttack();
        }
    }

    /// <summary>
    /// アビリティ処理
    /// </summary>

    void AbilityAttack()
    {
        Debug.Log("アビリティ発動");

        //ゲージ追加
        AddEnergy(10f);

        //アニメーション再生
        if (m_Animator != null)
        {
            m_Animator.Play(m_SpeedSlash);
            // 移動制限（攻撃フラグON）
            if (m_PC != null) m_PC.m_IsAttack = true;
            
            // アニメーション終了後にフラグ解除
            WaitForAnimationEnd(m_SpeedSlash).Forget();
        }

        AbilityCoolTimer(m_Ability).Forget();
    }

    void UltimateAttack()
    {
        Debug.Log("アルティメット発動");

        //蓄積値0に（リセット）
        m_CurrentEnergy = 0f;
        m_IsUlt = false;
        UpdateEnergyUI();

        //アニメーション再生
        if (m_Animator != null)
        {
            m_Animator.Play(m_UltraSlash);
            // 移動制限（攻撃フラグON）
            if (m_PC != null) m_PC.m_IsAttack = true;
            
            // アニメーション終了後にフラグ解除
            WaitForAnimationEnd(m_UltraSlash).Forget();
        }

        AbilityCoolTimer(m_Ult).Forget();
    }

    /// <summary>
    /// アニメーション終了を待ってフラグを解除する
    /// </summary>
    async UniTaskVoid WaitForAnimationEnd(string stateName)
    {
        // 1フレーム待ってアニメーション状態を取得
        await UniTask.Yield();
        
        // 現在のアニメーション状態を取得
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        
        // アニメーション長を取得して待機
        float animLength = stateInfo.length;
        await UniTask.Delay(TimeSpan.FromSeconds(animLength));
        
        // 攻撃フラグ解除
        if (m_PC != null)
        {
            m_PC.m_IsAttack = false;
            Debug.Log($"{stateName} アニメーション終了：攻撃フラグを解除しました。");
        }
    }

    /// <summary>
    /// クールタイム処理
    /// </summary>
    /// <returns></returns>
    async UniTask AbilityCoolTimer(SkillData data)
    {
        //クールタイム開始
        data.m_IsCoolingDown = true;

        //非表示
        data.m_KeyCodeImage.enabled = false;

        //クールタイム代入
        float timer = data.m_CoolTime;

        //指定した時間待機
        while (timer > 0)
        {
            //0.1まで表示
            data.m_CoolTimeText.text = timer.ToString("F1");

            //減算
            timer -= Time.deltaTime;

            //1フレームの待機
            await UniTask.Yield();
        }
        //カウントが終わったので空白
        data.m_CoolTimeText.text = "";

        //表示
        data.m_KeyCodeImage.enabled = true;

        //bool値解除
        data.m_IsCoolingDown = false;
    }

    /// <summary>
    /// チャージ処理
    /// </summary>
    /// <param name="amount"></param>
    void AddEnergy(float amount)
    {
        //溢れないように
        m_CurrentEnergy = Mathf.Clamp(m_CurrentEnergy + amount, 0, m_MaximumEnergy);
        UpdateEnergyUI();
    }

    /// <summary>
    /// スライダーに描画
    /// </summary>
    void UpdateEnergyUI()
    {
        m_EnergySlider.value = m_CurrentEnergy / m_MaximumEnergy;
    }

    /// <summary>
    /// デバッグ用：エネルギーを満タンにする
    /// </summary>
    public void MaximizeEnergy()
    {
        m_CurrentEnergy = m_MaximumEnergy;
        m_IsUlt = true;
        UpdateEnergyUI();
        Debug.Log("Energy Maxed Out via Debug Command");
    }

    /// <summary>
    /// いずれかのアビリティが動作中（クールダウン中）か
    /// </summary>
    public bool IsAnyAbilityActive()
    {
        return m_Ability.m_IsCoolingDown || m_Ult.m_IsCoolingDown;
    }
}
