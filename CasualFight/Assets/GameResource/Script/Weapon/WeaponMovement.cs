using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器を揺らす処理
/// </summary>
public class WeaponMovement : MonoBehaviour
{
    [Header("武器の参照"), SerializeField]
    Transform m_WeaponTf;

    [Header("揺れる速さ"), SerializeField]
    float m_WeaponSpeed = 10f;

    [Header("揺れる大きさ"), SerializeField]
    float m_WeaponShaking = 0.05f;

    [Header("走っているときの揺れる速さ"), SerializeField]
    float m_DashWeaponSpeed = 7f;

    [Header("走っているときの揺れる大きさ"), SerializeField]
    float m_DashWeaponShaking = 0.1f;

    //最初のオブジェクトの位置を覚えておくため
    Vector3 m_WeaponDefaultPos;

    [Header("プレイヤーオブジェクト"),SerializeField]
    GameObject m_PlayerObj;
    [Header("プレイヤーオブジェクト"),SerializeField]
    PlayerController m_PC;

    private void Start()
    {
        if (m_WeaponTf != null)
        {
            m_WeaponDefaultPos = m_WeaponTf.localPosition;
        }
    }
    private void Update()
    {
        if (m_PlayerObj = null)
            return;

        //プレイヤーが移動しているかのチェック
        bool isMoving = m_PC.m_MoveInput.sqrMagnitude > 0.01f;



        if (isMoving)
        {
            //揺れを反映する変数
            float currentSpeed=m_PC.m_IsDash?m_DashWeaponSpeed:m_WeaponSpeed;
            float currentShaking= m_PC.m_IsDash ? m_DashWeaponShaking : m_WeaponShaking;

            //揺れの計算
            float wabe = Mathf.Sin(Time.time * currentSpeed);

            //座標を反映
            Vector3 pos = m_WeaponDefaultPos;
            pos.y += wabe * currentShaking;

            //反映
            transform.localPosition = pos;
        }
        else
        {
            //自然に戻す
            m_WeaponTf.localPosition = Vector3.Lerp(transform.localPosition, m_WeaponDefaultPos, Time.deltaTime * 5f);
        }
    }
}
