using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    /// <summary>
    /// 追跡ステート
    /// プレイヤーを追いかける処理を行う
    /// </summary>
    public class S_Tracking : State<AITester>
    {
        public S_Tracking(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Trackingに入りました: 追跡開始...");
            
            // 移動アニメーション再生
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_MoveAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_MoveAnimName);
                }
            }
        }

        public override void Stay()
        {
            // プレイヤーが設定されていない場合は処理しない
            if (owner.m_Player == null || owner.m_EnemyData == null)
                return;

            // プレイヤーとの距離を計算
            float distance = Vector3.Distance(owner.transform.position, owner.m_Player.position);

            // 攻撃範囲内に入ったら攻撃へ遷移
            if (distance <= owner.m_EnemyData.m_AttackRange)
            {
                Debug.Log($"S_Tracking: 攻撃範囲内！距離: {distance:F2}");
                owner.ChangeState(AIState_Type.Attack);
                return;
            }

            // 索敵範囲外に出たら索敵フラグをONにしてIdleへ
            if (distance > owner.m_EnemyData.m_SearchRange)
            {
                Debug.Log($"S_Tracking: プレイヤー見失った！距離: {distance:F2}");
                owner.m_IsSearching = true; // 索敵フラグON

                // BattleManagerから敵を削除（UI連携）
                if (BattleManager.m_BattleInstance != null)
                {
                    BattleManager.m_BattleInstance.EnemyLostPlayer(owner.transform);
                }

                owner.ChangeState(AIState_Type.Idle);
                return;
            }

            // プレイヤーに向かって移動
            Vector3 direction = (owner.m_Player.position - owner.transform.position).normalized;
            direction.y = 0; // Y軸は無視して水平移動
            
            // 移動
            owner.transform.position += direction * owner.m_EnemyData.m_MoveSpeed * Time.deltaTime;
            
            // プレイヤーの方向を向く
            if (direction != Vector3.zero)
            {
                owner.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public override void Exit()
        {
            Debug.Log("S_Trackingを終了します");
        }
    }
}
