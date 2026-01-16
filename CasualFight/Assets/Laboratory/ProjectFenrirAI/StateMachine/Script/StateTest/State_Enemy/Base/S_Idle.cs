using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    public class S_Idle : State<AITester>
    {
        public S_Idle(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Idleに入りました: 待機中...");
            // ここで待機モーションなどを再生
            // 待機モーション再生
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_IdleAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_IdleAnimName);
                }
            }
        }

        public override void Stay()
        {
            // 待機中の処理があればここに記述
        }

        public override void Exit()
        {
            Debug.Log("S_Idleを終了します");
        }
    }
}
