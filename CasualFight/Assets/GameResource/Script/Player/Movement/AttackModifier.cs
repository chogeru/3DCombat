using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 最も近い敵を探してその方向に向かせる処理
/// </summary>
public class AttackModifier : MonoBehaviour
{
    [Header("索敵範囲(振り向き用)"), SerializeField]
    float m_SearchRadius = 3f;

    [Header("ホーミング範囲(移動用)"), SerializeField]
    float m_HomingRange = 8f;

    [Header("停止距離"), SerializeField]
    float m_StopDistance = 1.5f;

    [Header("索敵するタグ")]
    [SerializeField]
    string m_EnemyTag = "Enemy";

    /// <summary>
    /// 敵の方を向く
    /// </summary>
    public void LookAtenemy()
    {
        // EnemyTagに指定されたタグの索敵
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(m_EnemyTag);
        GameObject closestEnemyRotation = null;
        GameObject closestEnemyHoming = null;

        float minDistanceRotation = m_SearchRadius;
        float minDistanceHoming = m_HomingRange;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            // 振り向き用の判定
            if (dist < minDistanceRotation)
            {
                minDistanceRotation = dist;
                closestEnemyRotation = enemy;
            }

            // ホーミング移動用の判定
            if (dist < minDistanceHoming)
            {
                minDistanceHoming = dist;
                closestEnemyHoming = enemy;
            }
        }

        // キャラクターコントローラー取得
        CharacterController cc = GetComponent<CharacterController>();

        // ホーミング移動（振り向きより優先または同時に行う）
        if (closestEnemyHoming != null)
        {
            Vector3 targetPos = closestEnemyHoming.transform.position;
            Vector3 diff = targetPos - transform.position;
            diff.y = 0;
            
            float distanceToTarget = diff.magnitude;

            if (distanceToTarget > m_StopDistance)
            {
                // 停止距離を引いた移動距離
                float moveDistance = distanceToTarget - m_StopDistance;
                Vector3 moveDir = diff.normalized;

                // 瞬時に移動させる（CharacterController.Moveを使用することで衝突判定を維持）
                // 1フレームで複数回Moveを呼ぶことで「一瞬で移動」をシミュレート
                if (cc != null)
                {
                    // ループ回数を調整することで精度と速度を制御可能
                    // ここでは一気に移動
                    cc.Move(moveDir * moveDistance);
                }
            }

            // 移動後に敵の方向を向く
            Vector3 lookTarget = targetPos;
            lookTarget.y = transform.position.y;
            transform.LookAt(lookTarget);
        }
        else if (closestEnemyRotation != null)
        {
            // 移動はしないが振り向きだけ行う場合
            Vector3 targetPos = closestEnemyRotation.transform.position;
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
        }
    }
}
