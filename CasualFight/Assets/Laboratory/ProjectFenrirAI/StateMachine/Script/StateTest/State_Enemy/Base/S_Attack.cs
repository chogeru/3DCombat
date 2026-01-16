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
        private float m_Duration = 1.0f; // 1秒硬直

        public S_Attack_End(AITester owner) : base(owner) { }

        public override void Enter()
        {
            Debug.Log("  -> SubState [End]: 残心...(硬直)");
            m_Timer = 0f;
        }

        public override void Stay()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer >= m_Duration)
            {
                // 全工程終了。索敵フラグをONにしてIdleへ
                Debug.Log("  -> 攻撃完了。索敵フラグをONに戻します。");
                owner.m_IsSearching = true; // 索敵フラグON
                owner.ChangeState(AIState_Type.Idle);
            }
        }

        public override void Exit() { }
    }
}
