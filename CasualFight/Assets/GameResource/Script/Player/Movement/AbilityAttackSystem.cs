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
        [Header("表示用UI")]
        public Image m_KeyCodeImage;
        [Header("チャージ(Fill)用画像")]
        public Image m_FillImage;
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

    [Header("テレポート攻撃用コントローラー"), SerializeField]
    TeleportAttackController m_TeleportController;

    [Header("必殺技演出コントローラー"), SerializeField]
    UltimateSequenceController m_USC;

    //Ultが発動できるか判定
    bool m_IsUlt = false;

    // スキル（アビリティ・必殺技）のアニメーション実行中フラグ
    bool m_IsSkillActive = false;
    public bool IsSkillActive => m_IsSkillActive;

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

        // 硬直中は発動不可
        bool isStunned = m_PC != null && m_PC.IsStunned;

        //Eキーを押したらアビリティ発動
        if (Input.GetKeyDown(KeyCode.E) && !m_Ability.m_IsCoolingDown && m_WeaponSwitch.IsWeaponDrawn && !isDashing && !isStunned && !m_IsSkillActive)
        {
            AbilityAttack();
        }

        //Qキーを押したら必殺技発動
        if (Input.GetKeyDown(KeyCode.Q) && !m_Ult.m_IsCoolingDown && m_IsUlt && m_WeaponSwitch.IsWeaponDrawn && !isDashing && !isStunned && !m_IsSkillActive)
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



        // テレポート攻撃コントローラーがあればそちらを実行
        if (m_TeleportController != null)
        {
            m_TeleportController.ExecuteTeleportAttack().Forget();
            AbilityCoolTimer(m_Ability).Forget();
            return;
        }

        //アニメーション再生
        if (m_Animator != null)
        {
            m_IsSkillActive = true; // スキル実行中フラグON
            SetIgnoreCollision(true); // 衝突無効化
            
            // 【修正】ルートモーションを確実に優先（ダッシュ直後などOFFの場合があるため）
            m_Animator.applyRootMotion = true;

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
            m_IsSkillActive = true; // スキル実行中フラグON
            SetIgnoreCollision(true); // 衝突無効化

            // 【修正】ルートモーションを確実に優先
            m_Animator.applyRootMotion = true;

            m_Animator.Play(m_UltraSlash);
            // 移動制限（攻撃フラグON）
            if (m_PC != null)
            {
                m_PC.m_IsAttack = true;
                // 必殺技中は無敵（スーパーアーマー）にする
                m_PC.SetInvincible(true);
            }
            
            // 修正：ここにWaitForAnimationEndを入れると、最初の予備動作クリップが終わった時点で
            // 強制終了処理（OnSwingEndEvent）が走ってしまい、本番の攻撃やカメラワークが中断される。
            // そのため、通常のWaitは行わず、UltimateSequenceControllerからの完了通知（OnSwingEndEvent）を待つ。
            // ただし、万が一のために長時間のフェイルセーフのみ仕掛けておく。
            UltimateFailsafeTimer().Forget();
        }

        AbilityCoolTimer(m_Ult).Forget();
    }

    /// <summary>
    /// 必殺技のフェイルセーフ（何らかの理由でイベントが来なかった場合、5秒後に強制解除）
    /// </summary>
    async UniTaskVoid UltimateFailsafeTimer()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(5.0f));
        if (m_IsSkillActive)
        {
            Debug.LogWarning("必殺技の完了イベントが来なかったため、フェイルセーフで強制終了します。");
            ResetSkillFlags();
            if (m_USC != null) m_USC.OnSwingEndEvent();
        }
    }

    /// <summary>
    /// 外部（UltimateSequenceController）から呼ばれる終了処理
    /// </summary>
    public void ResetSkillFlags()
    {
        if (m_PC != null)
        {
             m_PC.m_IsAttack = false;
             // 無敵解除はUSC側で行われるが念のため
             m_PC.SetInvincible(false);
        }
        SetIgnoreCollision(false); // 衝突有効化
        m_IsSkillActive = false;
        Debug.Log("ResetSkillFlags: スキル実行中フラグを解除しました。");
    }

    /// <summary>
    /// アニメーション終了を待ってフラグを解除する
    /// </summary>
    /// <summary>
    /// アニメーション終了を待ってフラグを解除する
    /// </summary>
    async UniTaskVoid WaitForAnimationEnd(string stateName)
    {
        // アニメーションが切り替わるまで待機（タイムアウト付き）
        float timeout = 0.5f; // 最大0.5秒待つ
        while (timeout > 0f)
        {
            await UniTask.Yield();
            timeout -= Time.deltaTime;

            if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                break;
            }
        }
        
        // 現在のアニメーション状態を取得
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        
        // アニメーション長を取得して待機
        float animLength = stateInfo.length;
        
        // 念のため、長さが極端に短い（取得失敗）場合はデフォルト値を設定
        if (animLength < 0.1f) animLength = 1.0f;

        await UniTask.Delay(TimeSpan.FromSeconds(animLength));
        
        // 攻撃フラグ解除
        if (m_PC != null)
        {
            m_PC.m_IsAttack = false;
            Debug.Log($"{stateName} アニメーション終了：攻撃フラグを解除しました。");
        }

        // 必殺技の終了時は、カメラや演出の強制リセットを行う（アニメーションイベント漏れ対策）
        if (stateName == m_UltraSlash && m_USC != null)
        {
            m_USC.OnSwingEndEvent();
            Debug.Log("必殺技終了：UltimateSequenceControllerの終了処理を呼び出しました。");
        }
        
        m_IsSkillActive = false; // スキル実行中フラグOFF
        SetIgnoreCollision(false); // 衝突有効化
    }

    int m_PlayerLayer;
    int m_EnemyLayer;

    private void Start()
    {
        if (m_USC == null)
        {
            m_USC = GetComponent<UltimateSequenceController>();
        }
        // まだ見つからない場合（別オブジェクトにある場合など）はシーン内から検索
        if (m_USC == null)
        {
            m_USC = FindObjectOfType<UltimateSequenceController>();
        }

        // レイヤーインデックスを取得
        m_PlayerLayer = LayerMask.NameToLayer("Player");
        m_EnemyLayer = LayerMask.NameToLayer("Enemy");
    }

    /// <summary>
    /// スキル中のプレイヤー対敵の衝突無効化切替
    /// </summary>
    /// <param name="ignore"></param>
    void SetIgnoreCollision(bool ignore)
    {
        if (m_PlayerLayer >= 0 && m_EnemyLayer >= 0)
        {
            Physics.IgnoreLayerCollision(m_PlayerLayer, m_EnemyLayer, ignore);
            Debug.Log($"SetIgnoreCollision: Player(Layer {m_PlayerLayer}) vs Enemy(Layer {m_EnemyLayer}) collision ignored = {ignore}");
        }
        else
        {
            Debug.LogError($"SetIgnoreCollision Error: Invalid Layers - Player: {m_PlayerLayer}, Enemy: {m_EnemyLayer}");
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
        
        // アビリティの場合: FillAmountでクールダウン表現（Ultはエネルギー連動なのでここでは操作しない）
        if (data != m_Ult && data.m_FillImage != null)
        {
            data.m_FillImage.fillAmount = 0f;
        }

        //クールタイム代入
        float timer = data.m_CoolTime;
        float maxTime = data.m_CoolTime;

        //指定した時間待機
        while (timer > 0)
        {
            //0.1まで表示
            if (data.m_CoolTimeText != null)
            {
                data.m_CoolTimeText.text = timer.ToString("F1");
            }

            //減算
            timer -= Time.deltaTime;

            // FillAmount更新（0から1へ回復）※Ult以外
            if (data != m_Ult && data.m_FillImage != null)
            {
                data.m_FillImage.fillAmount = 1.0f - (timer / maxTime);
            }

            //1フレームの待機
            await UniTask.Yield();
        }
        
        //カウントが終わったので空白
        if (data.m_CoolTimeText != null)
        {
            data.m_CoolTimeText.text = "";
        }

        // FillAmountを確実に1にする ※Ult以外
        if (data != m_Ult && data.m_FillImage != null)
        {
            data.m_FillImage.fillAmount = 1.0f;
        }

        //bool値解除
        data.m_IsCoolingDown = false;
    }

    /// <summary>
    /// チャージ処理
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="amount"></param>
    public void AddEnergy(float amount)
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
        float ratio = m_CurrentEnergy / m_MaximumEnergy;
        
        if (m_EnergySlider != null)
        {
            m_EnergySlider.value = ratio;
        }

        // 必殺技アイコンのFillAmountも更新
        if (m_Ult.m_FillImage != null)
        {
            m_Ult.m_FillImage.fillAmount = ratio;
        }
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
