using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("歩く移動設定"), SerializeField]
    float m_WalkSpeed = 2f;

    [Header("ダッシュ移動設定"), SerializeField]
    float m_MoveSpeed = 5f;

    [Header("戦闘中ダッシュ移動設定"), SerializeField]
    float m_DashMoveSpeed = 13f;

    [Header("1秒間の回転値"), SerializeField]
    float m_RotationSpeed = 9999f;

    [Header("アニメーター"), SerializeField]
    Animator m_Animator;

    [Header("キャラクターコントローラー"), SerializeField]
    CharacterController m_Controller;

    [Space]

    [Header("ブリンク")]
    [Header("ブリンク時の速度"), SerializeField]
    float m_BlinkDashSpeed = 20f;

    [Header("ブリンクしている時間"), SerializeField]
    float m_BlinkTime = 0.2f;

    [Space]

    [Header("PlayerMoveSound"), SerializeField]
    PlayerMoveSound m_PMS;

    [Header("ComboSystem"), SerializeField]
    ComboSystem m_CS;

    [Header("WeaponSwitch"), SerializeField]
    WeaponSwitch m_WeaponSwitch;

    [Header("AbilityAttackSystem"), SerializeField]
    AbilityAttackSystem m_AAS;



    [Header("ActionController"), SerializeField]
    ActionController m_AC;

    [Header("PlayerHitController"), SerializeField]
    PlayerHitController m_PHC;

    [Header("HP設定")]
    [SerializeField] int m_MaxHP = 100;
    protected int m_CurrentHP;

    [Header("ヒット時にエフェクトを出す場所")]
    public Transform m_HitPosition;

    // 死亡フラグ
    bool m_IsDead = false;
    public bool IsDead => m_IsDead;

    [Header("ダメージ設定")]
    [SerializeField] float m_InvincibleTime = 1.0f; // ダメージ後の無敵時間
    bool m_IsInvincible = false;
    public bool IsInvincible => m_IsInvincible;

    [SerializeField] HPBarController m_HPBar; // UI参照

    [Header("エフェクト設定"), SerializeField]
    GameObject m_DeathEffect;

    [Header("死亡時設定"), SerializeField]
    float m_DeathDelay = 3.0f; // 死亡アニメーション待ち時間

    [Header("ダッシュ制限設定"), SerializeField]
    int m_MaxConsecutiveDashes = 2; // 最大連続回数
    
    [SerializeField]
    float m_DashResetTime = 1.0f; // リセットまでのクールタイム
    
    int m_CurrentDashCount = 0; // 現在の使用回数
    float m_LastDashTime = -10f; // 最後にダッシュした時間

    [Header("待機設定")]
    [SerializeField, Tooltip("待機アニメーションを開始するまでの時間(秒)")]
    float m_StandbyThreshold = 5.0f;
    float m_IdleTimer = 0f;
    bool m_IsStandbyTriggered = false;

    //移動値
    float m_Speed = 0f;

    //ダッシュ判定フラグ
    [HideInInspector]
    public bool m_IsDash = false;

    //攻撃判定フラグ
    [HideInInspector]
    public bool m_IsAttack { get; set; }

    //ブリンク中判定フラグ
    bool m_isBlink = false;



    //その他プレイヤー情報
    Camera m_MainCamera;
    [HideInInspector, Tooltip("スティックやキーボードの入力値")]
    public Vector3 m_MoveInput;

    // 移動アニメーション再生中フラグ
    bool m_IsMovingAnimator = false;



    // イベント演出中の操作ロックフラグ
    bool m_IsEventLocked = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Awake()
    {
        //プレイヤー情報のコンポーネント
        m_MainCamera = Camera.main;

    //ダッシュ値代入
    m_Speed = m_WalkSpeed;
}

public bool IsEventLocked => m_IsEventLocked;

// PlayerHitControllerの状態を公開
public bool IsStunned => m_PHC != null && m_PHC.IsStunned;

    private void Start()
    {
        // HP初期化
        m_CurrentHP = m_MaxHP;

        // エフェクト初期化
        if (m_DeathEffect != null)
        {
            m_DeathEffect.SetActive(false);
        }
    }

    /// <summary>
    /// 入力と回転の更新
    /// </summary>
    private void Update()
    {
        // 死亡時は操作不能
        if (m_IsDead) return;

        // 硬直中は入力を無視
        if (m_PHC != null && m_PHC.IsStunned) return;

        // 【追加】イベントロック中は操作不能
        if (m_IsEventLocked) return;

        //入力取得
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //カメラの計算
        Vector3 camForward = m_MainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = m_MainCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        m_MoveInput = (v * camForward + h * camRight).normalized;

        // 右クリック入力処理 (BlinkDash と Dash)
        // 攻撃中でもダッシュでキャンセル可能
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            // 回数制限チェック
            if (m_CurrentDashCount >= m_MaxConsecutiveDashes)
            {
                return;
            }

            // ブリンク開始前にフラグを立てて移動アニメーションの上書きを防ぐ
            m_isBlink = true;

            // 攻撃中ならキャンセル
            if (m_IsAttack)
            {
                m_IsAttack = false;
                m_CS?.ForceResetCombo();
            }

            // 音再生
            if(m_PMS != null)
            {
                 m_PMS.PlayBlinkSound();
            }

            // ダッシュ開始
            m_Animator.CrossFade("Dash_Start", 0.1f);
            DashProcess().Forget();
            m_IsDash = true;

            // 回数加算
            m_CurrentDashCount++;
            m_LastDashTime = Time.time;
            
            // リセットタイマー開始
            DashCountResetTimer().Forget();
        }
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            if (m_MoveInput.sqrMagnitude <= 0.01f)
            {
                StopDash();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopDash();
        }

        // 移動入力がなくなったらダッシュ解除
        if (m_MoveInput.sqrMagnitude < 0.01f)
        {
            StopDash();
        }

        // 武器装備状態の同期 (InEquipped: 0=未装備, 1=装備)
        float inEquippedValue = (m_WeaponSwitch != null && m_WeaponSwitch.IsWeaponDrawn) ? 1.0f : 0.0f;
        m_Animator.SetFloat("InEquipped", inEquippedValue);

        //スピードの変更
        if (m_IsDash)
        {
            // 装備中なら戦闘ダッシュ速度、未装備なら通常走り速度
            bool isDrawn = m_WeaponSwitch != null && m_WeaponSwitch.IsWeaponDrawn;
            m_Speed = isDrawn ? m_DashMoveSpeed : m_MoveSpeed;
        }
        else
        {
            // ダッシュ中でなければ歩行速度
            m_Speed = m_WalkSpeed;
        }

        //アニメーションパラメータ更新
        m_Animator.SetBool("Dash", m_IsDash);

        // RootMotionの制御 (戦闘ダッシュ時のみOFFにして手動移動の精度を優先)
        bool isCombatDash = m_IsDash && (m_WeaponSwitch != null && m_WeaponSwitch.IsWeaponDrawn);
        if (isCombatDash)
        {
            m_Animator.applyRootMotion = false;
        }
        else if (!m_IsAttack)
        {
            // 攻撃中でなければONに戻す
            m_Animator.applyRootMotion = true;
        }

        // 攻撃中またはスキルアクション中は移動不可
        if ((m_IsAttack || (m_AAS != null && m_AAS.IsSkillActive)) && !m_isBlink)
        {
            m_MoveInput = Vector3.zero;
        }

        bool isWeaponDrawn = m_WeaponSwitch != null && m_WeaponSwitch.IsWeaponDrawn;

        // 移動入力がある場合、一度だけ Move に Play する
        bool isMoving = (h != 0 || v != 0); // zeroにされる前の入力値で判定

        // 【修正】実際のステートが "Move" でない場合、フラグをリセットして再生を促す（自己修復）
        if (m_IsMovingAnimator)
        {
            var currentState = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (!currentState.IsName("Move") && !m_Animator.IsInTransition(0))
            {
                m_IsMovingAnimator = false;
            }
        }

        // ダッシュ中やブリンク中は移動アニメーションを再生しない（ダッシュアニメーションを優先）
        if (isMoving && !m_IsAttack && !m_isBlink && !m_IsDash && !m_IsMovingAnimator)
        {
            m_Animator.Play("Move", 0, 0f);
            m_IsMovingAnimator = true;
        }
        else if (!isMoving || m_IsAttack || m_isBlink || m_IsDash)
        {
            m_IsMovingAnimator = false;
        }


        //動きのサウンド
        if (m_MoveInput.sqrMagnitude < 0.01)
        {
            m_PMS.PlayerSoundMove(0);
        }
        else if (m_IsDash)
        {
            m_PMS.PlayerSoundMove(2);
        }
        else
        {
            m_PMS.PlayerSoundMove(1);
        }

        //待機タイマー処理
        isMoving = m_MoveInput.sqrMagnitude > 0.01f;
        // 何らかの活動を行っている、または刀を抜いているか判定
        if (isMoving || m_IsAttack || m_isBlink || m_IsDash || isWeaponDrawn)
        {
            m_IdleTimer = 0f;
            m_IsStandbyTriggered = false;
        }
        else
        {
            m_IdleTimer += Time.deltaTime;
            // 一定時間を超えたらStandbyトリガーを発動
            if (m_IdleTimer >= m_StandbyThreshold && !m_IsStandbyTriggered)
            {
                m_Animator.CrossFade("Angry", 0.1f);
                m_IsStandbyTriggered = true;
            }
        }
    }

    void StopDash()
    {
        if (m_IsDash)
        {
            if (!m_IsAttack && m_MoveInput.sqrMagnitude < 0.01f)
            {
                m_Animator.CrossFade("Run_Fast_Stop", 0.1f);
            }
            m_IsDash = false;
        }
    }

    /// <summary>
    /// 物理の更新
    /// </summary>
    private void FixedUpdate()
    {
        // 死亡時は物理挙動も停止
        if (m_IsDead) return;

        // 硬直中は物理移動も停止（念のため）
        if (m_PHC != null && m_PHC.IsStunned) return;

        // 【追加】イベントロック中は物理移動も停止
        if (m_IsEventLocked) return;

        //ブリンク中はRigidbodyによる移動を停止
        if (m_isBlink)
            return;

        Vector3 gravityMove = Vector3.zero;
        if (!m_Controller.isGrounded)
        {
            gravityMove.y = -9.81f * Time.fixedDeltaTime; // 重力
        }

        //移動の計算
        Vector3 moveStep = m_MoveInput * m_Speed * Time.fixedDeltaTime;

        //CharacterControllerで動かす
        m_Controller.Move(moveStep + gravityMove);

        //回転処理
        if (m_MoveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(m_MoveInput);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_RotationSpeed * Time.fixedDeltaTime);
        }

        //キャラアニメーションで動く
        //ダッシュ時はX,Yを2倍にする
        float animationSpeedMultiplier = m_IsDash ? 2.0f : 1.0f;
        m_Animator.SetFloat("X", m_MoveInput.x * animationSpeedMultiplier);
        m_Animator.SetFloat("Y", m_MoveInput.z * animationSpeedMultiplier);
    }


    async UniTaskVoid DashProcess()
    {
        // 注意: m_isBlink は呼び出し側 (Update) で既に true に設定されている

        //攻撃中ならキャンセル
        m_IsAttack = false;

        //強制リセット
        m_CS.ForceResetCombo();


        float timer = 0f;

        //一定期間中高速移動
        while (timer < m_BlinkTime)
        {
            // 死亡していたら中断
            if (m_IsDead) return;

            //入力値取得
            Vector3 inputDir = new Vector3(m_MoveInput.x, 0f, m_MoveInput.z);

            //少しでも入力されていたら
            if (inputDir.magnitude > 0.1)
            {
                //キャラクターのキャラの正面を上書き
                transform.forward = inputDir.normalized;
            }

            //向いている方向に物理的にFixedUpdateのタイミングで移動
            m_Controller.Move(transform.forward * m_BlinkDashSpeed * Time.fixedDeltaTime);

            //加算
            timer += Time.fixedDeltaTime;

            //1フレーム待機
            await UniTask.WaitForFixedUpdate();
        }

        m_isBlink = false;

        // ブリンク終了後、ダッシュ継続中かどうかにかかわらず、攻撃中でなければ Move に戻してブレンドツリーを有効化する
        // これにより「最後のポーズで固まる」現象を防ぐ
        if (m_Animator != null && !m_IsAttack)
        {
            m_Animator.CrossFade("Move", 0.1f);
            // Move再生中フラグを立てておくことで Update での重複 Play を防ぐ
            m_IsMovingAnimator = true;
        }
    }

    /// <summary>
    /// 攻撃終了時に呼ぶ
    /// </summary>
    public void OnAttackEnd()
    {
        Debug.Log("OnAttackEnd 呼ばれた");
        m_IsAttack = false;
    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (m_IsInvincible || m_CurrentHP <= 0) return;

        // 待機リセット
        m_IdleTimer = 0f;
        m_IsStandbyTriggered = false;



        // 武器を構える
        m_WeaponSwitch?.DrawWeapon();

        // HP減少
        m_CurrentHP = Mathf.Max(m_CurrentHP - damage, 0);

        // UI更新 (0.0 ～ 1.0 の割合で渡す)
        if (m_HPBar != null)
        {
            m_HPBar.OnTakeDamage((float)m_CurrentHP / m_MaxHP);
        }

        // 死亡判定
        if (m_CurrentHP <= 0)
        {
            Die();
        }
        else
        {
            // 無敵時間開始
            StartInvincibility().Forget();
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    private async UniTaskVoid Die()
    {
        Debug.Log("プレイヤーが死亡しました。");
        m_IsDead = true;

        // エフェクトの有効化
        if (m_DeathEffect != null)
        {
            m_DeathEffect.SetActive(true);
        }

        // アニメーションがある場合はここで発動
        if (m_Animator != null)
        {
            m_Animator.CrossFade("Hit_Death", 0.1f);
        }

        // 死亡演出待ち
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_DeathDelay));

        // テレポート先検索
        if (TeleportManager.TPInstance != null)
        {
            Vector3? targetPos = TeleportManager.TPInstance.GetNearestUnlockedPosition(transform.position);

            //変数の中身がnullじゃなければ
            if (targetPos.HasValue)
            {
                //変数の中身を取得
                Vector3 dest = targetPos.Value;
                // Y座標はプレイヤーの現在地を使用（リクエスト対応）
                Vector3 finalPos = new Vector3(dest.x, transform.position.y, dest.z);

                // 移動処理 (CharacterController使用時はenabledを切る必要がある)
                m_Controller.enabled = false;
                transform.position = finalPos;
                m_Controller.enabled = true;

                Debug.Log($"最寄りのテレポート地点に移動しました: {finalPos}");

                // 復活処理
                Revive();
            }
            else
            {
                Debug.LogWarning("開放済みのテレポート地点が見つかりませんでした。");
            }
        }
    }

    /// <summary>
    /// 復活処理
    /// </summary>
    private void Revive()
    {
        m_IsDead = false;
        m_CurrentHP = m_MaxHP; // 全回復

        // HPバー更新
        if (m_HPBar != null)
        {
            m_HPBar.OnTakeDamage(1.0f); // 100%
        }
        
        // エフェクトを戻す
        if (m_DeathEffect != null)
        {
            m_DeathEffect.SetActive(false);
        }

        // アニメーションを待機に戻す
        if (m_Animator != null)
        {
             m_Animator.Play("Idle"); // または Move
        }
    }

    /// <summary>
    /// 無敵時間の制御
    /// </summary>
    private async UniTaskVoid StartInvincibility()
    {
        m_IsInvincible = true;
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_InvincibleTime));
        m_IsInvincible = false;
    }

    /// <summary>
    /// 外部から無敵状態を設定する
    /// </summary>
    public void SetInvincible(bool isInvincible)
    {
        m_IsInvincible = isInvincible;
    }

    /// <summary>
    /// イベント演出などによる操作ロックを設定する
    /// </summary>
    public void SetEventLock(bool isLocked)
    {
        m_IsEventLocked = isLocked;

        // ロック時は入力をリセットして移動アニメーションも止める
        if (isLocked)
        {
            m_MoveInput = Vector3.zero;
            if (m_Animator != null)
            {
                m_Animator.SetFloat("X", 0);
                m_Animator.SetFloat("Y", 0);
            }
        }
    }

    /// <summary>
    /// 設定画面から呼び出す手動リスポーン処理
    /// 最寄りのテレポート地点に移動し、全快する
    /// </summary>
    public void ManualRespawn()
    {
        if (TeleportManager.TPInstance != null)
        {
            Vector3? targetPos = TeleportManager.TPInstance.GetNearestUnlockedPosition(transform.position);

            if (targetPos.HasValue)
            {
                Vector3 dest = targetPos.Value;
                // Y座標は現在地維持ではなく、安全のためテレポート地点のYを使うか、あるいは現在地か。
                // Die()では現在地Yを使っていたが、スタック脱出の意味合いもあるため、
                // テレポート地点の高さに合わせるのが無難だが、
                // Die()の実装に合わせて `dest.x, transform.position.y, dest.z` にするか、
                // 完全な安全地帯への移動なら `dest` そのものが良い。
                // ここでは Die() と同じく現在地Yを維持する。（地形抜けしている場合は危険だが、テレポート地点が地面にある前提）
                Vector3 finalPos = new Vector3(dest.x, transform.position.y, dest.z);

                // 移動
                m_Controller.enabled = false;
                transform.position = finalPos;
                m_Controller.enabled = true;

                Debug.Log($"手動リスポーン実行: {finalPos}");

                // 回復
                Revive();
            }
            else
            {
                Debug.LogWarning("開放済みのテレポート地点が見つかりませんでした。");
            }
        }
    }

    /// <summary>
    /// ダッシュ回数のリセットタイマー
    /// </summary>
    private async UniTaskVoid DashCountResetTimer()
    {
        // クールタイム待機
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_DashResetTime));

        // 待機完了後、「最終ダッシュ時間」から十分経過しているか再確認
        // (待機中に再度ダッシュしていた場合、このタスクは何もしない)
        if (Time.time - m_LastDashTime >= m_DashResetTime)
        {
            m_CurrentDashCount = 0;
        }
    }
}