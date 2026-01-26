using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 最も近い敵を探してその方向に向かせる処理
/// </summary>
public class AttackModifier : MonoBehaviour
{
    [Header("索敵範囲(敵に近づきはしないが、その場で振り向くだけの対象を探す範囲)"), SerializeField]
    float m_SearchRadius = 3f;

    [Header("ホーミング範囲(攻撃ボタンを押したとき、どれくらい離れた敵まで自動で移動するかの距離)"), SerializeField]
    float m_HomingRange = 8f;

    [Header("停止距離(敵からどれくらい離れた位置で止まるかの距離)"), SerializeField]
    float m_StopDistance = 1.5f;

    [Header("索敵するタグ")]
    [SerializeField]
    string m_EnemyTag = "Enemy";

    [Header("吸い付き移動にかける時間"), SerializeField]
    float m_HomingDuration = 0.1f;

    private CancellationTokenSource m_HomingCts;

    private void OnDestroy()
    {
        CancelHomingTask();
    }

    private void CancelHomingTask()
    {
        if (m_HomingCts != null)
        {
            m_HomingCts.Cancel();
            m_HomingCts.Dispose();
            m_HomingCts = null;
        }
    }

    /// <summary>
    /// 近くにいる敵を探索し、攻撃時の自動追尾（移動・回転）を行うメソッド
    /// コンボ攻撃時などに呼び出され、攻撃を敵に当てやすくする
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
            // 敵をターゲットしたら、まずはRootMotionをOFFにする
            if (anim != null) anim.applyRootMotion = false;

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

                // 滑らかな移動を開始
                StartSmoothHoming(destination, cc, anim).Forget();
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

    /// <summary>
    /// 攻撃時の踏み込み移動（ホーミング）を非同期で行う処理
    /// 指定された時間(m_HomingDuration)で目標地点へ滑らかに移動する
    /// </summary>
    /// <param name="destination">移動先の座標（敵の手前など）</param>
    /// <param name="cc">移動に使用するCharacterController</param>
    /// <param name="anim">RootMotion制御用のAnimator</param>
    private async UniTaskVoid StartSmoothHoming(Vector3 destination, CharacterController cc, Animator anim)
    {
        CancelHomingTask();
        m_HomingCts = new CancellationTokenSource();
        CancellationToken token = m_HomingCts.Token;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        try
        {
            // 移動開始時にRootMotionを一時停止
            if (anim != null) anim.applyRootMotion = false;

            while (elapsed < m_HomingDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / m_HomingDuration);

                // 線形補間
                Vector3 targetPos = Vector3.Lerp(startPos, destination, t);
                Vector3 moveAmount = targetPos - transform.position;

                if (cc != null)
                {
                    cc.Move(moveAmount);
                }
                else
                {
                    transform.position = targetPos;
                }

                await UniTask.Yield(token);
            }

            // 最後に位置を微調整
            if (cc != null)
            {
                cc.Move(destination - transform.position);
            }
            else
            {
                transform.position = destination;
            }
        }
        catch (System.OperationCanceledException)
        {
            // キャンセル時は中断
        }
        finally
        {
            // RootMotionを戻す判断はPlayerController側のアニメーション終了などで行われるが、
            // ここでも念のため一旦終了処理としての扱いに留める
        }
    }
}
