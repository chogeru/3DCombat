using UnityEngine;
using StateMachineAI;

namespace StateMachineAI
{
    // ==========================================
    // 親ステート: 攻撃全体を管理
    // ==========================================
    public class S_Attack : State<AITester>
    {
        public S_Attack(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("S_Attack: 攻撃シークエンス開始。サブステートを構築します。");
            
            // 攻撃アニメーション再生
            if (owner.m_Animator != null && owner.m_EnemyData != null)
            {
                if (!string.IsNullOrEmpty(owner.m_EnemyData.m_AttackAnimName))
                {
                    owner.m_Animator.Play(owner.m_EnemyData.m_AttackAnimName);
                }
            }

            // サブステートの登録
            // Phase 1: Start (予備動作)
            AddSubState(new S_Attack_Start(owner));
            // Phase 2: Execution (攻撃判定)
            AddSubState(new S_Attack_Exec(owner));
            // Phase 3: End (硬直・終了)
            AddSubState(new S_Attack_End(owner));

            // 最初のサブステート(Start)へ遷移
            ChangeSubState(0);
        }

        public override void Stay()
        {
            // ここでは特に何もしない（サブステートの更新はStateMachine側で自動で行われるが、
            // カスタムロジックが必要なら書く）
            
            // 例: 強制キャンセル条件など
        }

        public override void Exit()
        {
            Debug.Log("S_Attack: 攻撃シークエンス終了。");
        }
    }

    // ==========================================
    // サブステート: 予備動作
    // ==========================================
    public class S_Attack_Start : State<AITester>
    {
        private float m_Timer = 0f;
        private float m_Duration = 0.5f; // 0.5秒ためる

        public S_Attack_Start(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("  -> SubState [Start]: 予備動作...");
            m_Timer = 0f;
        }

        public override void Stay()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer >= m_Duration)
            {
                // 次のサブステート(Exec)へ
                // 親ステート経由で遷移できるが、ここでは親を知っている前提でインデックス1へ
                GetParentState()?.ChangeSubState(1);
            }
        }

        public override void Exit() { }
    }

    // ==========================================
    // サブステート: 攻撃実行
    // ==========================================
    public class S_Attack_Exec : State<AITester>
    {
        private float m_Timer = 0f;
        private float m_Duration = 0.2f; // 一瞬

        public S_Attack_Exec(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("  -> SubState [Exec]: とりゃあ！！(攻撃判定発生)");
            m_Timer = 0f;
            // ここで攻撃判定を出す処理など
        }

        public override void Stay()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer >= m_Duration)
            {
                // 次のサブステート(End)へ
                GetParentState()?.ChangeSubState(2);
            }
        }

        public override void Exit() { }
    }

    // ==========================================
    // サブステート: 硬直・終了
    // ==========================================
    public class S_Attack_End : State<AITester>
    {
        private float m_Timer = 0f;
        // アニメーション指定がない場合のフォールバック時間
        private float m_FallbackDuration = 1.0f;

        public S_Attack_End(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("  -> SubState [End]: 残心...(硬直)");
            m_Timer = 0f;
        }

        public override void Stay()
        {
            bool isFinish = false;

            // アニメーターがあればアニメーションの終了を判定
            if (owner.m_Animator != null && owner.m_EnemyData != null && !string.IsNullOrEmpty(owner.m_EnemyData.m_AttackAnimName))
            {
                AnimatorStateInfo stateInfo = owner.m_Animator.GetCurrentAnimatorStateInfo(0);
                
                // 指定のアニメーションが再生されているかチェック
                if (stateInfo.IsName(owner.m_EnemyData.m_AttackAnimName))
                {
                    // 終了判定 (1.0以上で再生終了)
                    if (stateInfo.normalizedTime >= 1.0f)
                    {
                        isFinish = true;
                    }
                }
                else
                {
                    // 攻撃アニメーション以外が再生されている場合
                    // (遷移都合で既に変わっている、あるいは再生失敗など)
                    // タイマーで保険をかける
                    m_Timer += Time.deltaTime;
                    if (m_Timer >= m_FallbackDuration)
                    {
                        isFinish = true;
                    }
                }
            }
            else
            {
                // アニメーターがない場合はタイマー処理
                m_Timer += Time.deltaTime;
                if (m_Timer >= m_FallbackDuration)
                {
                    isFinish = true;
                }
            }

            if (isFinish)
            {
                // 全工程終了。
                Debug.Log("  -> 攻撃完了（アニメーション終了）。");

                // 後退設定がある、かつ後退時間が0より大きい場合は後退へ、そうでなければIdleへ
                if (owner.m_EnemyData != null && owner.m_EnemyData.m_RetreatDuration > 0f)
                {
                     // 後退へ遷移
                     owner.ChangeState(AIState_Type.Retreat);
                }
                else
                {
                    // そのまま待機へ
                    owner.m_IsSearching = true; // 索敵フラグON
                    owner.ChangeState(AIState_Type.Idle);
                }
            }
        }

        public override void Exit() { }
    }
}
