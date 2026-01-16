using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineAI
{
    /// <summary>
    /// ステートの受け皿
    /// ステート(状態)の基礎骨子で、これを基にステートが運営される
    /// ステート内にある、Enter、Execute、Exitは、実装されたステートでオーバーライド(上書き)されて実行される。
    /// Enter   ステートに移行した際に最初に起動する関数。
    ///         Unityでいうなれば、Start()と同じものと考えればよい。
    /// Stay    ステートが継続している間常に実行される関数。
    ///         Unityでいうなれば、Update()と同じものと考えればよい。
    /// Exit    ステートが終了する際に実行される関数。
    ///         まぁ、C++でいうデストラクタと同じ
    /// 
    /// 【拡張機能】
    /// - サブステート: ステートの階層化を実現
    /// </summary>
    public class State<T>
    {
        /// このステートを利用するインスタンス
        public T owner;

        /// <summary>
        /// 親ステート（サブステートの場合のみ使用）
        /// </summary>
        protected State<T> m_ParentState = null;

        /// <summary>
        /// サブステート用のステートマシン
        /// </summary>
        protected StateMachine<T> m_SubStateMachine = null;

        /// <summary>
        /// サブステートのリスト
        /// </summary>
        protected List<State<T>> m_SubStates = new List<State<T>>();

        /// コンストラクタで、ステート登録された場合、登録先のAIをオーナーとして認定する。
        /// 認定されたオーナー(owner)を使って、様々な行動を処理させる事になる。
        public State(T owner)
        {
            /// コンストラクタで獲得したオーナー(owner)対象を代入し確定させる。
            this.owner = owner;
        }
    
        /// このステートに遷移する時に一度だけ呼ばれる
        /// UnityでのStart()関数と同じもの
        public virtual void Enter()
        {
        }

        /// このステートである間、毎フレーム呼ばれる
        /// UnityでのUpdate()関数と同じもの
        public virtual void Stay()
        {
        }

        /// このステートから他のステートに遷移するときに一度だけ呼ばれる
        /// C++でのディストラクタと同じもの
        public virtual void Exit()
        {
        }

        // ========== サブステート関連メソッド ==========

        /// <summary>
        /// サブステートを追加
        /// </summary>
        /// <param name="subState">追加するサブステート</param>
        public void AddSubState(State<T> subState)
        {
            subState.m_ParentState = this;
            m_SubStates.Add(subState);

            // サブステートマシンを初期化
            if (m_SubStateMachine == null)
                m_SubStateMachine = new StateMachine<T>();
        }

        /// <summary>
        /// サブステートに遷移（インデックス指定）
        /// </summary>
        /// <param name="index">サブステートのインデックス</param>
        public void ChangeSubState(int index)
        {
            if (m_SubStateMachine != null && index >= 0 && index < m_SubStates.Count)
                m_SubStateMachine.ChangeState(m_SubStates[index]);
        }

        /// <summary>
        /// サブステートに遷移（ステート直接指定）
        /// </summary>
        /// <param name="subState">遷移先のサブステート</param>
        public void ChangeSubState(State<T> subState)
        {
            if (m_SubStateMachine != null && m_SubStates.Contains(subState))
                m_SubStateMachine.ChangeState(subState);
        }

        /// <summary>
        /// サブステートを持っているか
        /// </summary>
        /// <returns>サブステートがあればtrue</returns>
        public bool HasSubStates()
        {
            return m_SubStates.Count > 0;
        }

        /// <summary>
        /// サブステートを更新
        /// </summary>
        public void UpdateSubStates()
        {
            if (m_SubStateMachine != null)
                m_SubStateMachine.Update();
        }

        /// <summary>
        /// 親ステートを取得
        /// </summary>
        /// <returns>親ステート（なければnull）</returns>
        public State<T> GetParentState()
        {
            return m_ParentState;
        }

        /// <summary>
        /// 現在のサブステートを取得
        /// </summary>
        /// <returns>現在のサブステート</returns>
        public State<T> GetCurrentSubState()
        {
            return m_SubStateMachine?.CurrentState;
        }

        /// <summary>
        /// サブステートのリストを取得
        /// </summary>
        /// <returns>サブステートのリスト</returns>
        public List<State<T>> GetSubStates()
        {
            return m_SubStates;
        }

        /// <summary>
        /// サブステートの数を取得
        /// </summary>
        /// <returns>サブステートの数</returns>
        public int GetSubStateCount()
        {
            return m_SubStates.Count;
        }
    }
}
