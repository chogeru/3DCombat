using Cysharp.Threading.Tasks;
using System.Linq;
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

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            //攻撃中でもダッシュキャンセルしてブリンク
            DashProcess().Forget();
        }

        //攻撃中は移動禁止
        if (m_IsAttack&&!m_isBlink)
        {
            m_MoveInput = Vector3.zero;
        }

        //移動入力がある間は継続ダッシュ
        m_IsDash = m_MoveInput.sqrMagnitude>0.01f&&Input.GetKey(KeyCode.LeftShift);

        //スピードの変更
        if (m_IsDash)
        {
            m_Speed = m_MoveSpeed;
        }
        else if (m_IsFightDash)
        {
            m_Speed = m_DashMoveSpeed;
        }
        else
        {
            m_Speed = m_WalkSpeed;
        }

        //アニメーション変更
        m_Animator.SetBool("Dash", m_IsDash);
        m_Animator.SetBool("FightDash", m_IsFightDash);
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
            gravityMove.y = -9.81f * Time.deltaTime; // 重力
        }

        //移動の計算
        Vector3 moveStep = m_MoveInput * m_Speed * Time.deltaTime;

        //CharacterControllerで動かす
        m_Controller.Move(moveStep + gravityMove);

        //回転処理
        if (m_MoveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(m_MoveInput);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_RotationSpeed * Time.deltaTime);
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
            if(inputDir.magnitude>0.1)
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
        m_isBlink=false;
    }

    /// <summary>
    /// 攻撃終了時に呼ぶ
    /// </summary>
    public void OnAttackEnd()
    {
        Debug.Log("OnAttackEnd 呼ばれた");
        m_IsAttack = false;
    }
}