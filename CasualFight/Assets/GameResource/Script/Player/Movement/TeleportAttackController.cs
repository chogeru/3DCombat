using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// テレポート攻撃（裏回り連撃）を制御するクラス。
/// Eキーのアビリティ発動時に呼び出され、敵の背後へのワープ攻撃と帰還攻撃を行う。
/// </summary>
public class TeleportAttackController : MonoBehaviour
{
    [Header("必要なコンポーネント")]
    [SerializeField] Animator m_Animator;
    [SerializeField] PlayerController m_PlayerController;
    [SerializeField] BattleManager m_BattleManager;

    [Header("攻撃設定")]
    [SerializeField, Tooltip("敵の背後どれくらいの距離に出現するか")]
    float m_TeleportOffset = 1.0f; // 敵の後ろ1m

    [Header("アニメーション設定")]
    [SerializeField, Tooltip("攻撃のアニメーションステート名")]
    string m_AttackStateName = "SpeedSlash";

    // 内部保持用
    private Vector3 m_OriginalPosition;
    private Quaternion m_OriginalRotation;
    private Transform m_TargetEnemy;

    /// <summary>
    /// テレポート攻撃のシーケンスを実行する
    /// </summary>
    public async UniTaskVoid ExecuteTeleportAttack()
    {
        // ターゲット（最寄りの敵）を取得
        m_TargetEnemy = GetNearestEnemy();

        // ターゲットがいない場合：その場でアニメーション再生（ルートモーションON、イベント無視）
        if (m_TargetEnemy == null)
        {
            Debug.Log("No Target: Playing Animation in place (RootMotion ON)");
            
            // 入力ロックだけかける（攻撃中なので）
            if (m_PlayerController != null) m_PlayerController.SetEventLock(true);

            try
            {
                if (m_Animator != null)
                {
                    // ルートモーションはONのまま（デフォルト）
                    m_Animator.Play(m_AttackStateName);
                }

                // アニメーション終了待ち
                await UniTask.Yield();
                var stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
                await UniTask.Delay(System.TimeSpan.FromSeconds(stateInfo.length));
            }
            finally
            {
                if (m_PlayerController != null) m_PlayerController.SetEventLock(false);
            }
            return;
        }

        // --- 以下、ターゲットがいる場合の通常処理 ---

        // 開始位置を保存（帰還用）
        m_OriginalPosition = transform.position;
        m_OriginalRotation = transform.rotation;

        // 1. ルートモーションをOFFにする
        if (m_Animator != null) m_Animator.applyRootMotion = false;

        // 2. 入力をロックする
        if (m_PlayerController != null) m_PlayerController.SetEventLock(true);
        
        try
        {
            // 3. 攻撃アニメーション再生（開始位置でスタート）
            if (m_Animator != null) m_Animator.Play(m_AttackStateName);
            Debug.Log("Teleport Attack Start: Animation Started");

            // 4. アニメーション終了まで待機
            await UniTask.Yield();
            var stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            await UniTask.Delay(System.TimeSpan.FromSeconds(stateInfo.length));
        }
        finally
        {
            // 終了処理
            if (m_Animator != null) m_Animator.applyRootMotion = true;
            if (m_PlayerController != null) m_PlayerController.SetEventLock(false);
        }
    }

    /// <summary>
    /// 【アニメーションイベントから呼ぶ】
    /// 敵の背後にワープする
    /// </summary>
    public void OnTeleportToBehind()
    {
        if (m_TargetEnemy == null) return;

        // 敵の背後位置 = 敵の位置 - (敵の正面 * オフセット)
        Vector3 teleportPos = m_TargetEnemy.position - (m_TargetEnemy.forward * m_TeleportOffset);
        teleportPos.y = transform.position.y;
        transform.position = teleportPos;

        // 敵の方を向く
        transform.LookAt(new Vector3(m_TargetEnemy.position.x, transform.position.y, m_TargetEnemy.position.z));

        Debug.Log("Teleport: Moved to Behind Enemy");
    }

    /// <summary>
    /// 【アニメーションイベントから呼ぶ】
    /// 元の位置に戻り、敵の方を向く
    /// </summary>
    public void OnReturnToOriginalPosition()
    {
        transform.position = m_OriginalPosition;

        // ターゲットがいればそちらを向く
        if (m_TargetEnemy != null)
        {
            transform.LookAt(new Vector3(m_TargetEnemy.position.x, transform.position.y, m_TargetEnemy.position.z));
        }
        else
        {
            transform.rotation = m_OriginalRotation;
        }

        Debug.Log("Teleport Return: Front of Enemy (via Event)");
    }

    /// <summary>
    /// 【アニメーションイベントから呼ぶ】
    /// 操作ロックを解除し、移動可能にする
    /// </summary>
    public void OnUnlockMovement()
    {
        // 終了処理と同じことを行う
        if (m_Animator != null) m_Animator.applyRootMotion = true;
        
        if (m_PlayerController != null) m_PlayerController.SetEventLock(false);
        
        Debug.Log("Teleport Attack: Unlocked via Event");
    }

    /// <summary>
    /// 最寄りの敵を探す
    /// </summary>
    private Transform GetNearestEnemy()
    {
        if (m_BattleManager == null) return null;
        if (m_BattleManager.m_ActiveEnemies == null || m_BattleManager.m_ActiveEnemies.Count == 0) return null;

        Transform nearest = null;
        float minDist = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (var enemy in m_BattleManager.m_ActiveEnemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(currentPos, enemy.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
}
