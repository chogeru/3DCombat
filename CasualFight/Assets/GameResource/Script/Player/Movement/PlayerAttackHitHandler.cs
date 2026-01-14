using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// プレイヤーの攻撃判定処理(攻撃判定はアニメーションイベントで1から記入していく)
/// 「0」を「ダメージ判定なし（エフェクトのみ）」や「システム予約」として設計にする。
/// 「1」から使い始めることで、「0 ＝ 何も起きない」という安全策を講じている
/// </summary>
public class PlayerAttackHitHandler : MonoBehaviour
{
    [Header("判定させるレイヤー"), SerializeField]
    LayerMask m_LayerMaskEnemy;

    [Header("コンボ順に")]
    [Header("判定の大きさ(半径)"), SerializeField]
    float[] m_Radii = { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2.5f };
    [Header("判定の大きさ(奥行)"), SerializeField]
    float[] m_Distance = { 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.5f };

    /// <summary>
    /// アニメーションイベントで呼ばれる当たり判定処理
    /// </summary>
    /// <param name="hit"></param>
    void OnAttackHitCheck(int step)
    {
        //コンボ取得
        int index = Mathf.Clamp(step-1, 0, m_Radii.Length-1);

        //奥行の位置を決め、中心とする
        Vector3 hitCenter=transform.position+transform.forward*m_Distance[index];

        //
        Collider[] hitenemys = Physics.OverlapSphere(hitCenter, m_Radii[index],m_LayerMaskEnemy);
    }
}
