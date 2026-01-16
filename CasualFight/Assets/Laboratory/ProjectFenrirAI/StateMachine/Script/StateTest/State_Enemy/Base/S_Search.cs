using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    /// <summary>
    /// グローバル索敵ステート
    /// 常時プレイヤーを監視し、発見時に適切なステートへ遷移させる
    /// </summary>
    public class S_Search : State<AITester>
    {
        public S_Search(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Search(グローバル): 監視開始");
        }

        public override void Stay()
        {
            // 死亡チェック
            if (owner.m_EnemyHP <= 0 && !owner.IsCurrentState(AIState_Type.Die))
            {
                owner.ChangeState(AIState_Type.Die);
                return;
            }

            // 死亡中・被弾中は処理しない
            if (owner.IsCurrentState(AIState_Type.Die) || owner.IsCurrentState(AIState_Type.Hit))
                return;

            // プレイヤーが設定されていない場合は処理しない
            if (owner.m_Player == null || owner.m_EnemyData == null)
                return;

            // 索敵フラグがOFFなら処理しない（既に追跡/攻撃中）
            if (!owner.m_IsSearching)
                return;

            // プレイヤーとの距離を計算
            float distance = Vector3.Distance(owner.transform.position, owner.m_Player.position);

            // 索敵範囲内にプレイヤーがいる場合
            if (distance <= owner.m_EnemyData.m_SearchRange)
            {
                Debug.Log($"S_Search: プレイヤー発見！距離: {distance:F2}");
                owner.m_IsSearching = false; // 索敵フラグOFF

                // BattleManagerに敵を登録（UI連携）
                if (BattleManager.m_BattleInstance != null)
                {
                    BattleManager.m_BattleInstance.EnemyFoundPlayer(owner.transform);
                }

                // 距離によって遷移先を分岐
                if (distance <= owner.m_EnemyData.m_AttackRange)
                {
                    owner.ChangeState(AIState_Type.Attack);
                }
                else
                {
                    owner.ChangeState(AIState_Type.Tracking);
                }
            }
        }

        public override void Exit()
        {
            // グローバルステートなので通常は呼ばれない
        }
    }
}
