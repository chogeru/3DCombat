using Cysharp.Threading.Tasks;
using System.Linq;
using System.Resources;
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

    //攻撃中ダッシュ判定フラグ
    [HideInInspector]
    public bool m_IsFightDash = false;

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

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            //攻撃中でもダッシュキャンセルしてブリンク
            DashProcess().Forget();
        }

        //攻撃中またはガード中は歩行不可
        m_IsGuard = m_AC != null && m_AC.IsGuarding && !m_isBlink && !m_IsAttack;
        if ((m_IsAttack || m_IsGuard) && !m_isBlink)
        {
            m_MoveInput = Vector3.zero;
        }

        m_Animator.SetBool("Guard", m_IsGuard);

        //移動入力とダッシュキーの判定
        bool isDashPressed = m_MoveInput.sqrMagnitude > 0.01f && Input.GetKey(KeyCode.Mouse1);
        if (isDashPressed)
        {
            //戦闘中か武器を抜いているなら
            if (BattleManager.m_BattleInstance.m_IsCombat || m_WeaponSwitch.IsWeaponDrawn)
            {
                m_IsFightDash = true;
                m_IsDash = false;
            }
            //戦闘中じゃなければ
            else
            {
                m_IsFightDash = false;
                m_IsDash = true;
            }
        }
        else
        {
            m_IsDash = false;
            m_IsFightDash = false;
        }

        //スピードの変更（判定の後に計算する）
        if (m_IsFightDash)
        {
            m_Speed = m_DashMoveSpeed;
        }
        else if (m_IsDash)
        {
            m_Speed = m_MoveSpeed;
        }
        else
        {
            m_Speed = m_WalkSpeed;
        }

        //アニメーション変更
        m_Animator.SetBool("Dash", m_IsDash);
        m_Animator.SetBool("FightDash", m_IsFightDash);

        // FIGHT DASH中はルートモーションをOFFにする
        if (m_IsFightDash)
        {
            m_Animator.applyRootMotion = false;
        }
        else if (!m_IsAttack)
        {
            // ダッシュ中ではなく、かつ攻撃中でなければONに戻す
            m_Animator.applyRootMotion = true;
        }

        //動きのサウンド
        if (m_MoveInput.sqrMagnitude < 0.01)
        //停止
        {
            m_PMS.PlayerSoundMove(0);
        }
        //走り
        else if (m_IsDash || m_IsFightDash)
        {
            m_PMS.PlayerSoundMove(2);
        }
        //歩き
        else
        {
            m_PMS.PlayerSoundMove(1);
        }

        //待機タイマー処理
        bool isMoving = m_MoveInput.sqrMagnitude > 0.01f;
        // 何らかの活動を行っているか判定
        if (isMoving || m_IsAttack || m_isBlink || m_IsGuard)
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
                m_Animator.SetTrigger("Standby");
                m_IsStandbyTriggered = true;
            }
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
        m_Animator.SetFloat("X", m_MoveInput.x);
        m_Animator.SetFloat("Y", m_MoveInput.z);
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

        m_Animator.SetTrigger("BlinkDash");

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
            m_Animator.SetTrigger("Die");
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