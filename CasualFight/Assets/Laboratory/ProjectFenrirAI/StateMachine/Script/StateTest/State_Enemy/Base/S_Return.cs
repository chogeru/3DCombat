using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    /// <summary>
    /// 初期位置へ帰還するステート
    /// </summary>
    public class S_Return : State<AITester>
    {
        private float m_WaitTimer = 0f;
        private bool m_IsMoving = false;
        private const float WAIT_DURATION = 1.5f;

        public S_Return(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Return: 索敵を開始します（1.5秒待機）。");
            m_WaitTimer = 0f;
            m_IsMoving = false;

            // 索敵アニメーション再生
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_SearchAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_SearchAnimName);
                }
            }
        }

        public override void Stay()
        {
            // プレイヤーが近くにいるか監視（索敵継続）
            if (owner.m_Player != null && owner.m_EnemyData != null)
            {
                float distanceToPlayer = Vector3.Distance(owner.transform.position, owner.m_Player.position);
                if (distanceToPlayer <= owner.m_EnemyData.m_SearchRange)
                {
                    Debug.Log("S_Return: 帰還中にプレイヤーを発見！追跡を再開します。");
                    
                    // BattleManagerに再登録
                    if (BattleManager.m_BattleInstance != null)
                    {
                        BattleManager.m_BattleInstance.EnemyFoundPlayer(owner.transform);
                    }

                    owner.m_IsSearching = false; // 追跡モードへ
                    owner.ChangeState(AIState_Type.Tracking);
                    return;
                }
            }

            // --- 待機フェーズ ---
            if (!m_IsMoving)
            {
                m_WaitTimer += Time.deltaTime;
                if (m_WaitTimer >= WAIT_DURATION)
                {
                    m_IsMoving = true;
                    Debug.Log("S_Return: 待機終了、初期位置へ移動を開始します。");

                    // 移動アニメーションへ切り替え
                    if (owner.m_Animator != null && owner.m_EnemyData != null && !string.IsNullOrEmpty(owner.m_EnemyData.m_MoveAnimName))
                    {
                        owner.m_Animator.Play(owner.m_EnemyData.m_MoveAnimName);
                    }
                }
                else
                {
                    return; // 待機中は移動処理を行わない
                }
            }

            // --- 移動フェーズ ---

            // 初期位置までの距離
            float distanceToSpawn = Vector3.Distance(owner.transform.position, owner.m_SpawnPosition);

            // 到着判定 (例えば 0.5f 以内)
            if (distanceToSpawn <= 0.5f)
            {
                Debug.Log("S_Return: 初期位置に到着しました。待機状態に戻ります。");
                
                // 位置と回転を正確に戻す
                owner.transform.position = owner.m_SpawnPosition;
                owner.transform.rotation = owner.m_SpawnRotation;

                owner.ChangeState(AIState_Type.Idle);
                return;
            }

            // 初期位置へ移動
            Vector3 direction = (owner.m_SpawnPosition - owner.transform.position).normalized;
            direction.y = 0; // 高さは無視

            if (owner.m_EnemyData != null)
            {
                owner.transform.position += direction * owner.m_EnemyData.m_MoveSpeed * Time.deltaTime;
            }

            // 進行方向を向く
            if (direction != Vector3.zero)
            {
                owner.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public override void Exit()
        {
            Debug.Log("S_Return: 終了");
        }
    }
}
