using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    public class S_Die : State<AITester>
    {
        public S_Die(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Die: 死亡しました...ぐふっ");
            owner.m_IsDead = true;
            
            // BattleManagerから敵を削除（UI連携）
            if (BattleManager.m_BattleInstance != null)
            {
                BattleManager.m_BattleInstance.EnemyLostPlayer(owner.transform);
            }
            
            // 死亡アニメーション
            if (owner.m_Animator != null)
            {
                // owner.m_Animator.Play("Die");
            }

            // コライダーを消すなどの処理
            if (owner.m_Rigidbody != null)
            {
                owner.m_Rigidbody.isKinematic = true;
            }
        }

        public override void Stay()
        {
            // 死体は語らない
        }

        public override void Exit()
        {
            // 蘇生処理などがない限り呼ばれない
        }
    }
}
