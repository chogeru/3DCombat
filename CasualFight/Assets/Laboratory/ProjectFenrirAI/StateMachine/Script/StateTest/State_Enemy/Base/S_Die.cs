using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    public class S_Die : State<AITester>
    {
        private bool m_IsDissolveStarted = false;

        public S_Die(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Die: 死亡しました...ぐふっ");
            owner.m_IsDead = true;
            m_IsDissolveStarted = false;

            // タグを変更してターゲットから外す
            owner.gameObject.tag = "Untagged";
            
            // BattleManagerから敵を削除（UI連携）
            if (BattleManager.m_BattleInstance != null)
            {
                BattleManager.m_BattleInstance.EnemyLostPlayer(owner.transform);
            }
            
            // 死亡アニメーション
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_DieAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_DieAnimName);
                }
            }

            // 物理挙動の停止（落下防止）
            if (owner.m_Rigidbody != null)
            {
                owner.m_Rigidbody.isKinematic = true;
                owner.m_Rigidbody.velocity = Vector3.zero;
            }

            // コライダーの無効化（当たり判定削除）
            Collider collider = owner.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        public override void Stay()
        {
            if (m_IsDissolveStarted) return;
            if (owner.m_Animator == null || owner.m_EnemyData == null) return;

            // アニメーションステートの監視
            AnimatorStateInfo stateInfo = owner.m_Animator.GetCurrentAnimatorStateInfo(0);

            // 現在のステートが死亡アニメーションであり、かつ再生完了しているか
            if (stateInfo.IsName(owner.m_EnemyData.m_DieAnimName) && stateInfo.normalizedTime >= 1.0f)
            {
                m_IsDissolveStarted = true;
                if (owner.m_DissolveController != null)
                {
                    owner.m_DissolveController.StartDissolve();
                }
            }
        }

        public override void Exit()
        {
            // 蘇生処理などがない限り呼ばれない
        }
    }
}
