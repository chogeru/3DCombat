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
            if (owner.m_Animator != null)
            {
                // owner.m_Animator.Play("Idle"); // アニメーションがあれば
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
