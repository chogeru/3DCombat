using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    public class S_GlobalMonitor : State<AITester>
    {
        public S_GlobalMonitor(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_GlobalMonitor: 監視を開始します (Spaceキーで攻撃テスト)");
        }

        public override void Stay()
        {
            // HP監視: 0になったら死亡
            if (owner.m_EnemyHP <= 0 && !owner.IsCurrentState(AIState_Type.Die))
            {
                owner.ChangeState(AIState_Type.Die);
                return;
            }

            // テスト用: Spaceキーで攻撃へ遷移
            // ※ 死んでいないときのみ
            if (Input.GetKeyDown(KeyCode.Space) && !owner.IsCurrentState(AIState_Type.Die) && !owner.IsCurrentState(AIState_Type.Attack))
            {
                Debug.Log("GlobalMonitor: 攻撃トリガー検知");
                owner.ChangeState(AIState_Type.Attack);
            }
        }

        public override void Exit()
        {
            // グローバルステートが外れることはあまりないが念のため
        }
    }
}
