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
        Animator anim = GetComponent<Animator>();

        // ホーミング移動（振り向きより優先または同時に行う）
        if (closestEnemyHoming != null)
        {
            Vector3 targetPos = closestEnemyHoming.transform.position;
            Vector3 playerPos = transform.position;
            
            // Y軸を揃える
            targetPos.y = playerPos.y;
            
            Vector3 diff = targetPos - playerPos;
            float distanceToTarget = diff.magnitude;

            if (distanceToTarget > m_StopDistance)
            {
                // 目標地点：敵の座標から停止距離分だけ手前の位置
                Vector3 moveDir = diff.normalized;
                Vector3 destination = targetPos - (moveDir * m_StopDistance);

                // ルートモーションを一時的にOFF（スクリプトによる移動を優先）
                if (anim != null)
                {
                    anim.applyRootMotion = false;
                }

                // ルートモーションや物理演算による上書きを防ぐため、
                // 一時的にCharacterControllerを無効化して直接座標を書き換える
                if (cc != null)
                {
                    cc.enabled = false;
                    transform.position = destination;
                    cc.enabled = true;
                }
                else
                {
                    transform.position = destination;
                }
                
                // 物理演算とトランスフォームの同期を強制（埋まり防止と即時反映のため）
                Physics.SyncTransforms();
            }

            // 移動後に敵の方向を向く
            if (diff != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(diff);
            }
        }
        else if (closestEnemyRotation != null)
        {
            // 移動はしないが振り向きだけ行う場合
            Vector3 targetPos = closestEnemyRotation.transform.position;
            Vector3 diff = targetPos - transform.position;
            diff.y = 0;

            if (diff != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(diff);
            }
        }
    }
}
