using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    /// <summary>
    /// 被弾ステート
    /// ダメージを受けた時のリアクション処理を行う
    /// </summary>
    public class S_Hit : State<AITester>
    {
        private float m_Timer = 0f;
        private float m_HitStunDuration = 0.5f; // 被弾硬直時間

        public S_Hit(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Hitに入りました: 被弾リアクション開始...");
            m_Timer = 0f;
            
            // 被弾アニメーション再生
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_HitAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_HitAnimName);
                }
            }
        }

        public override void Stay()
        {
            m_Timer += Time.deltaTime;
            
            // 硬直時間終了後、Idleへ遷移
            if (m_Timer >= m_HitStunDuration)
            {
                owner.ChangeState(AIState_Type.Idle);
            }
        }

        public override void Exit()
        {
            Debug.Log("S_Hitを終了します");
        }
    }
}
