using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VInspector.VInspectorData;

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

    [Space]

    [Header("歩いているときの刀とキャラの離れる距離"), SerializeField]
    float m_BetweenWalk = 0.1f;
    [Header("走っているときの刀とキャラの離れる距離"), SerializeField]
    float m_BetweenDash = 0.15f;

    [Space]

    [Header("待機時の刀の角度設計"),SerializeField]
    Vector3 m_IdleRotation = new Vector3(0, 0, 0);
    [Header("歩いている時の刀の角度設計"), SerializeField]
    Vector3 m_WalkRotation = new Vector3(0, 40, 0);
    [Header("走っている時刀の角度設計"), SerializeField]
    Vector3 m_DashRotation = new Vector3(0, 80, 0);

    [Space]

    //最初のオブジェクトの位置を覚えておくため
    Vector3 m_WeaponDefaultPos;
    //ベースの回転
    Quaternion m_BaseRot = Quaternion.Euler(7, 0, 163);

    [Header("プレイヤーオブジェクト"), SerializeField]
    GameObject m_PlayerObj;
    [Header("プレイヤーオブジェクト"), SerializeField]
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
        if (m_PlayerObj == null || m_PC == null)
            return;

        //プレイヤーが移動しているかのチェック
        bool isMoving = m_PC.m_MoveInput.sqrMagnitude > 0.01f;

        //待機状態
        Vector3 targetPos = m_WeaponDefaultPos;
        Vector3 targetRotOffset = Vector3.zero;

        if (isMoving)
        {
            //揺れを反映する変数
            float currentSpeed = m_PC.m_IsDash ? m_DashWeaponSpeed : m_WeaponSpeed;
            float currentShaking = m_PC.m_IsDash ? m_DashWeaponShaking : m_WeaponShaking;

            //揺れの計算
            float wabe = Mathf.Sin(Time.time * currentSpeed);

            //移動中だけ背中から武器を離す
            //そしてダッシュ中なら離す距離変更
            float between = m_PC.m_IsDash ? -m_BetweenDash : -m_BetweenWalk;

            //移動中だけ武器の傾きを変える
            //そしてダッシュ中なら傾く距離変更
            Vector3 targetRot = m_PC.m_IsDash ? m_DashRotation : m_WalkRotation;

            //追加したい回転値を先に変換
            Quaternion addRotation = Quaternion.Euler(targetRot);

            //座標を反映
            targetPos.y += wabe * currentShaking;
            targetPos.z += between;

            //元の回転値との合成
            Quaternion targetRotQ = addRotation * m_BaseRot;

            //反映
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 5f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotQ, Time.deltaTime * 5f);
        }
        else
        {
            //自然に戻す
            m_WeaponTf.localPosition = Vector3.Lerp(transform.localPosition, m_WeaponDefaultPos, Time.deltaTime * 5f);

            //元の角度に戻す
            Quaternion idleQ = Quaternion.Euler(7, 0, 163);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, idleQ, Time.deltaTime * 5f);
        }
    }
}
