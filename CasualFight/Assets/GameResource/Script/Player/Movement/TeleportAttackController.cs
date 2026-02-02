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
        // 開始位置を保存（帰還用・および方向計算用）
        m_OriginalPosition = transform.position;
        m_OriginalRotation = transform.rotation;

        // 1. ルートモーションをONにする（アニメーション依存）
        if (m_Animator != null) m_Animator.applyRootMotion = true;

        // 2. 入力をロックする
        // 2. 入力をロックする
        if (m_PlayerController != null)
        {
             m_PlayerController.SetEventLock(true);
             // 【追加】攻撃フラグを立ててスーパーアーマー化（クールダウン消失対策）
             m_PlayerController.m_IsAttack = true;
        }

        // 【追加】攻撃開始前に、敵の方を向く（ルートモーションの進行方向を敵に合わせるため）
        if (m_TargetEnemy != null)
        {
            Vector3 lookTarget = m_TargetEnemy.position;
            lookTarget.y = transform.position.y; // 高さは自分のまま（上や下を向かないようにする）
            transform.LookAt(lookTarget);
        }
        
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
            if (m_PlayerController != null) 
            {
                // フラグを下ろす（念のため）
                m_PlayerController.SetEventLock(false);
                // 【追加】攻撃中フラグを解除（これで移動可能になる）
                m_PlayerController.m_IsAttack = false;
            }
        }
    }

    /// <summary>
    /// 【アニメーションイベントから呼ぶ】
    /// 敵の背後にワープする
    /// </summary>
    public void OnTeleportToBehind()
    {
        // ルートモーション化に伴い、処理を無効化
        // if (m_TargetEnemy == null) return;

        // // 【修正】安全な位置を探して移動
        // Vector3 finalPos = GetSafeTeleportPosition(m_TargetEnemy);
        // transform.position = finalPos;

        // // 敵の方を向く
        // transform.LookAt(new Vector3(m_TargetEnemy.position.x, transform.position.y, m_TargetEnemy.position.z));

        // Debug.Log("Teleport: Moved to Behind Enemy");
    }

    /// <summary>
    /// 【アニメーションイベントから呼ぶ】
    /// 2回目の攻撃用：再度、敵の背後にワープする（元に戻らず追撃）
    /// </summary>
    public void OnReturnToOriginalPosition()
    {
        // ルートモーション化に伴い、処理を無効化
        // if (m_TargetEnemy == null) return;

        // // 【修正】元の位置に戻るのではなく、再度「敵の背後（安全な位置）」へ移動する
        // Vector3 finalPos = GetSafeTeleportPosition(m_TargetEnemy);
        // transform.position = finalPos;

        // // 敵の方を向く
        // Vector3 lookTarget = new Vector3(m_TargetEnemy.position.x, transform.position.y, m_TargetEnemy.position.z);
        // transform.LookAt(lookTarget);

        // Debug.Log("Teleport 2nd Attack: Moved to Behind Enemy again (Safe Position)");
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

    /// <summary>
    /// ターゲット周辺の安全なテレポート位置（背後優先）を探す
    /// </summary>
    private Vector3 GetSafeTeleportPosition(Transform target)
    {
        if (target == null) return transform.position;

        Vector3 targetPos = target.position;
        Vector3 targetForward = target.forward;
        float yPos = transform.position.y;

        // 【変更】敵の向きではなく、「開始位置(P) -> 敵(E) の延長線上」を基準にする
        // つまり、プレイヤーから見て「敵の奥（Pink Heart）」をターゲットにする
        
        Vector3 directionToEnemy = (targetPos - m_OriginalPosition);
        // 距離が近すぎる(ほぼ重なっている)場合は、敵の背後(forwardの逆)をフォールバックとして使うが
        // ここでは敵のforwardを使って奥側を計算する
        if (directionToEnemy.sqrMagnitude < 0.1f)
        {
             // 敵が向いている方向の逆にプレイヤーがいると仮定 -> 敵の正面が奥
             // いや、単純に敵の背後（標準）にしたいなら -forward
             // しかし「突き抜ける」なら forward方向か？ 
             // 画像の意図としては「挟んで反対側」なので、近すぎる場合は標準の「背後」でよいとする
             directionToEnemy = targetForward; 
        }
        
        // 正規化して方向ベクトルにする
        Vector3 forwardVec = directionToEnemy.normalized; 
        Vector3 rightVec = Vector3.Cross(Vector3.up, forwardVec).normalized;

        // 候補地点のリスト（優先度順：奥、右奥、左奥、右横、左横）
        Vector3[] candidates = new Vector3[]
        {
            targetPos + (forwardVec * m_TeleportOffset),           // 真奥 (Pink Heart)
            targetPos + (forwardVec * m_TeleportOffset) + (rightVec * 0.5f), // 右奥
            targetPos + (forwardVec * m_TeleportOffset) - (rightVec * 0.5f), // 左奥
            targetPos + (rightVec * m_TeleportOffset),             // 右横
            targetPos - (rightVec * m_TeleportOffset),             // 左横
        };

        // 衝突判定用の半径（キャラクターサイズに合わせて調整、余裕を持たせる）
        float checkRadius = 0.4f; 

        // レイヤーマスク（Characters, Obstacleなどを想定。Defaultも含める）
        // 必要に応じて調整してください。自分自身は含めないように注意が必要だが、
        // Physics.CheckSphereは自分自身のColliderも拾う可能性があるため、
        // 実際は「移動先」になにもないかを確認する。
        int layerMask = Physics.DefaultRaycastLayers;

        foreach (var pos in candidates)
        {
            // 高さ合わせ
            Vector3 checkPos = new Vector3(pos.x, yPos, pos.z);
            Vector3 center = checkPos + Vector3.up * 1.0f; // 中心を少し上げる

            // 球体判定で障害物があるかチェック
            // CheckSphereだと自分や敵のコライダーも拾ってしまう可能性があるため、OverlapSphereで個別に確認する
            Collider[] hits = Physics.OverlapSphere(center, checkRadius, layerMask, QueryTriggerInteraction.Ignore);
            bool isHit = false;

            foreach (var hit in hits)
            {
                // 自分自身のコライダーは無視
                if (hit.transform.root == transform.root) continue;

                // ターゲット（敵）のコライダーも無視（接近して攻撃したいため）
                if (target != null && hit.transform.root == target.root) continue;

                // それ以外に当たっていたら「障害物あり」とみなす
                isHit = true;
                break;
            }

            if (!isHit)
            {
                return checkPos; // 空いていれば採用
            }
        }

        // 全てダメだった場合は、仕方なく真後ろ（または現在地）を返す
        // 強引に背後に出る
        Vector3 fallbackPos = targetPos - (targetForward * m_TeleportOffset);
        return new Vector3(fallbackPos.x, yPos, fallbackPos.z);
    }
}
