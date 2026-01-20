using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    /// <summary>
    /// 後退ステート
    /// プレイヤーから一定時間、距離を取るように移動する
    /// </summary>
    public class S_Retreat : State<AITester>
    {
        private float m_Timer = 0f;

        public S_Retreat(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Retreat: 後退を開始します。");
            m_Timer = 0f;

            // アニメーション再生（設定されていれば）
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_RetreatAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_RetreatAnimName);
                }
            }
        }

        public override void Stay()
        {
            // プレイヤーがいない、またはデータがない場合は即座にIdleへ
            if (owner.m_Player == null || owner.m_EnemyData == null)
            {
                owner.ChangeState(AIState_Type.Idle);
                return;
            }

            // タイマー更新
            m_Timer += Time.deltaTime;

            // 指定時間が経過したら終了
            if (m_Timer >= owner.m_EnemyData.m_RetreatDuration)
            {
                Debug.Log("S_Retreat: 後退終了。次の行動を決定します。");
                
                // プレイヤーとの距離を確認
                float distance = Vector3.Distance(owner.transform.position, owner.m_Player.position);

                // 索敵範囲内なら追跡へ遷移（戦闘継続）
                if (distance <= owner.m_EnemyData.m_SearchRange)
                {
                    owner.ChangeState(AIState_Type.Tracking);
                }
                else
                {
                    // 範囲外なら見失い処理
                    Debug.Log("S_Retreat: プレイヤーを見失いました。");
                    
                    if (BattleManager.m_BattleInstance != null)
                    {
                        BattleManager.m_BattleInstance.EnemyLostPlayer(owner.transform);
                    }

                    owner.m_IsSearching = true; // 索敵フラグON
                    owner.ChangeState(AIState_Type.Idle);
                }
                return;
            }

            // 移動処理: プレイヤーと逆方向へ
            // 方向ベクトル: 自分 - プレイヤー = プレイヤーから自分へのベクトル
            Vector3 direction = (owner.transform.position - owner.m_Player.position).normalized;
            direction.y = 0; // 高さは無視

            // 移動実行
            owner.transform.position += direction * owner.m_EnemyData.m_RetreatSpeed * Time.deltaTime;

            // 視線はプレイヤーに向けたまま後退する
            Vector3 lookDir = (owner.m_Player.position - owner.transform.position).normalized;
            if (lookDir != Vector3.zero)
            {
                // Y軸回転のみ適用し、常にプレイヤーの方を向く
                lookDir.y = 0;
                owner.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        public override void Exit()
        {
            Debug.Log("S_Retreat: 終了処理");
        }
    }
}
