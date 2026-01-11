using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Playerの地面(下)に対する重力処理
/// </summary>
public class PlayerGravity : MonoBehaviour
{
    [Header("通常の重力"), SerializeField]
    float m_Gravity;

    [Header("地面に吸い付く力"), SerializeField]
    float m_StickToGroundForce;

    [Header("斜面で吸着を試みる距離"), SerializeField]
    float m_SnapDistance;

    [Header("地面レイヤー"), SerializeField]
    LayerMask m_LayerMask;

    [SerializeField]
    CharacterController m_CController;

    //速度
    Vector3 m_Velocity;

    //地面についているかの判定フラグ
    bool m_WasGrounded=false;

    private void Awake()
    {
        //コンポーネントの取得
        if(m_CController==null)
        {
            m_CController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        HandleGravity();
    }

    /// <summary>
    /// 地面に対する重力処理
    /// </summary>
    void HandleGravity()
    {
        //地面に接地しているかどうか(true:足元が地面に触れている)
        bool isGround = m_CController.isGrounded;

        //地面に着いた瞬間と地面にいる間
        if (isGround&&m_Velocity.y<0)
        {
            //常にわずかな力で地面に押し付け続けることで接地を安定させる
            m_Velocity.y = -2f;
        }

        //さっきまで接地していた、かつ今は浮いている、かつジャンプ（上方向への速度）中ではない
        if (m_WasGrounded&&!isGround&&m_Velocity.y<=0)
        {
            if(Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit,m_SnapDistance,m_LayerMask))
            {
                //めり込み防止(地面までの全距離 - CharacterControllerの厚み)
                float snapAmount = hit.distance - m_CController.skinWidth;

                //スナップ(地形への吸着)処理、無理やり地面にピッタリくっつける(坂道で浮いた瞬間だけ)
                m_CController.Move(Vector3.down * snapAmount);

                //(次のフレームでも)接地状態を維持させる対策
                m_Velocity.y = -2f;
            }
        }
        //落下速度を更新する
        m_Velocity.y = m_Gravity * Time.deltaTime;

        //物理的な落下、重力に従って下に加速しながら落ちる(毎フレーム必ず)
        m_CController.Move(m_Velocity*Time.deltaTime);

        //接地状態を次のフレームのために記録
        m_WasGrounded = isGround;
    }
}
