using Cysharp.Threading.Tasks;
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

    [Header("SpecialMoveManager"), SerializeField]
    SpecialMoveManager m_SMM;

    [Header("GuardSystem"), SerializeField]
    GuardSystem m_GS;

    [Header("ActionController"), SerializeField]
    ActionController m_AC;

    [Header("HP設定")]
    [SerializeField] int m_MaxHP = 100;
    int m_CurrentHP;

    [Header("ダメージ設定")]
    [SerializeField] float m_InvincibleTime = 1.0f; // ダメージ後の無敵時間
    bool m_IsInvincible = false;

    [SerializeField] HPBarController m_HPBar; // UI参照

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

    //ガード中判定フラグ
    bool m_IsGuard = false;

    //その他プレイヤー情報
    Rigidbody m_Rb;
    Camera m_MainCamera;
    [HideInInspector, Tooltip("スティックやキーボードの入力値")]
    public Vector3 m_MoveInput;

    // 移動アニメーション再生中フラグ
    bool m_IsMovingAnimator = false;

    // ガードアニメーション発火済みフラグ
    bool m_IsGuardAnimatorTriggered = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Awake()
    {
        //プレイヤー情報のコンポーネント
        m_Rb = GetComponent<Rigidbody>();
        m_Rb.interpolation = RigidbodyInterpolation.Interpolate;
        m_MainCamera = Camera.main;

        //ダッシュ値代入
        m_Speed = m_WalkSpeed;

    }

    private void Start()
    {
        // HP初期化
        m_CurrentHP = m_MaxHP;
    }

    /// <summary>
    /// 入力と回転の更新
    /// </summary>
    private void Update()
    {
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
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            // 攻撃中でもダッシュキャンセルしてブリンク
            m_Animator.CrossFade("Dash_Start", 0.1f);
            DashProcess().Forget();
            m_IsDash = true;
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

        // 攻撃中またはガード中は歩行不可
        bool isGuardBreaking = m_SMM != null && m_SMM.IsGuardBreaking;
        bool isWeaponDrawn = m_WeaponSwitch != null && m_WeaponSwitch.IsWeaponDrawn;
        bool isGuardPrev = m_IsGuard;
        m_IsGuard = m_AC != null && m_AC.IsGuarding && !m_isBlink && !m_IsAttack && !isGuardBreaking && isWeaponDrawn;
        
        if (m_IsGuard)
        {
            // ガードアニメーションを一度だけ発火
            if (!m_IsGuardAnimatorTriggered)
            {
                m_Animator.CrossFade("ARPG_Samurai_Guard_B", 0.1f);
                m_IsGuardAnimatorTriggered = true;
            }
        }
        else
        {
            // ガード解除時：フラグリセットと移動遷移
            if (m_IsGuardAnimatorTriggered)
            {
                m_IsGuardAnimatorTriggered = false;
                // 移動入力があればMoveへ遷移
                if (h != 0 || v != 0)
                {
                    m_Animator.CrossFade("Move", 0.1f);
                    m_IsMovingAnimator = true;
                }
            }
        }

        if ((m_IsAttack || m_IsGuard) && !m_isBlink)
        {
            m_MoveInput = Vector3.zero;
        }

        // 移動入力がある場合、一度だけ Move に CrossFade する
        bool isMoving = (h != 0 || v != 0); // zeroにされる前の入力値で判定
        if (isMoving && !m_IsAttack && !m_IsGuard && !m_isBlink && !m_IsMovingAnimator)
        {
            m_Animator.CrossFade("Move", 0.1f);
            m_IsMovingAnimator = true;
        }
        else if (!isMoving || m_IsAttack || m_IsGuard || m_isBlink)
        {
            m_IsMovingAnimator = false;
        }

        m_Animator.SetBool("Guard", m_IsGuard);

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
        if (isMoving || m_IsAttack || m_isBlink || m_IsGuard || m_IsDash || isWeaponDrawn)
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
        if (m_isBlink)
            return;

        m_isBlink = true;

        //攻撃中ならキャンセル
        m_IsAttack = false;

        //強制リセット
        m_CS.ForceResetCombo();

        //ブリンク中だけ速度をリセットして干渉を防ぐ
        m_Rb.velocity = Vector3.zero;

        float timer = 0f;

        //一定期間中高速移動
        while (timer < m_BlinkTime)
        {
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

        // ガード判定
        if (m_IsGuard)
        {
            Debug.Log("ガード成功！ダメージを無効化しました。");
            if (m_GS != null)
            {
                m_GS.OnGuardSuccess();
            }
            return; // ダメージを受けずに終了
        }

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
    private void Die()
    {
        Debug.Log("プレイヤーが死亡しました。");
        // アニメーションがある場合はここで発動
        if (m_Animator != null)
        {
            m_Animator.CrossFade("Die", 0.1f);
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
}