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

    //移動値
    float m_Speed = 0f;

    //攻撃中ダッシュ判定フラグ
    [HideInInspector]
    public bool m_IsFightDash = false;

    //ダッシュ判定フラグ
    [HideInInspector]
    public bool m_IsDash = false;

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
        //ダッシュ判定
        if (m_MoveInput.sqrMagnitude > 0.01f)
        {
            //左シフトキー押したとき
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                m_IsDash = true;
            }
        }
        else
        {
            m_IsDash = false;
        }

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
    }

    /// <summary>
    /// 物理の更新
    /// </summary>
    private void FixedUpdate()
    {
        //移動処理
        Vector3 newVelocity = m_MoveInput * m_Speed;
        newVelocity.y = m_Rb.velocity.y;
        m_Rb.velocity = newVelocity;

        //回転処理
        if (m_MoveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(m_MoveInput);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_RotationSpeed * Time.deltaTime);
        }

        //キャラアニメーションで動く
        m_Animator.SetFloat("X", Input.GetAxis("Horizontal"));
        m_Animator.SetFloat("Y", Input.GetAxis("Vertical"));
    }
}