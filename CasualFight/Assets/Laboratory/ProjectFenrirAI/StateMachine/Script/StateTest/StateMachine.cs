using System.Collections.Generic;

namespace StateMachineAI
{
    /// <summary>
    /// ステートマシン
    /// ステートの本来のプログラムで、ステートに与えられた稼働関数は
    /// ここで実行指示を受けている。
    /// 
    /// 【拡張機能】
    /// - ステート履歴スタック: 前のステートに戻る機能
    /// - 遷移イベント通知: ステート変化を外部から監視
    /// - グローバルステート: 全ステート共通で実行される処理
    /// </summary>
    public class StateMachine<T>
    {
        /// <summary>
        /// 現在のステート
        /// </summary>
        private State<T> m_CurrentState;

        /// <summary>
        /// ステート履歴スタック
        /// </summary>
        private Stack<State<T>> m_StateHistory = new Stack<State<T>>();

        /// <summary>
        /// 履歴の最大保持数
        /// </summary>
        private int m_MaxHistoryCount = 10;

        /// <summary>
        /// グローバルステート（常に実行）
        /// </summary>
        private State<T> m_GlobalState = null;

        /// <summary>
        /// ステート変更時のイベント
        /// 引数: (前のステート, 新しいステート)
        /// </summary>
        public event System.Action<State<T>, State<T>> OnStateChanged;

        /// <summary>
        /// コンストラクタ
        /// 現在のステートをnullにして無効化する
        /// </summary>
        public StateMachine()
        {
            m_CurrentState = null;
        }

        /// <summary>
        /// 現在のステートを呼び出す
        /// </summary>
        public State<T> CurrentState
        {
            get { return m_CurrentState; }
        }

        /// <summary>
        /// 履歴の数を取得
        /// </summary>
        public int HistoryCount
        {
            get { return m_StateHistory.Count; }
        }

        /// <summary>
        /// 該当するステートに変更する
        /// </summary>
        /// <param name="state">遷移する先のステート</param>
        /// <param name="saveHistory">履歴に保存するかどうか（デフォルト: true）</param>
        public void ChangeState(State<T> state, bool saveHistory = true)
        {
            State<T> previousState = m_CurrentState;

            // 現在のステートが存在している
            if (m_CurrentState != null)
            {
                // 履歴に保存
                if (saveHistory)
                {
                    m_StateHistory.Push(m_CurrentState);
                    // 最大数を超えたら古いものを削除
                    TrimHistory();
                }
                // ステートの遷移の為、現在のステートの終了処理を実行
                m_CurrentState.Exit();
            }

            // 現在のステートを新しいステートに変更する
            m_CurrentState = state;
            // 現在のステートのEnter関数を呼び出す
            m_CurrentState.Enter();

            // イベント通知
            OnStateChanged?.Invoke(previousState, m_CurrentState);
        }

        /// <summary>
        /// 前のステートに戻る
        /// </summary>
        /// <returns>戻ることができたらtrue</returns>
        public bool RevertToPreviousState()
        {
            if (m_StateHistory.Count == 0)
                return false;

            State<T> previousState = m_StateHistory.Pop();
            ChangeState(previousState, false); // 履歴に保存しない
            return true;
        }

        /// <summary>
        /// グローバルステートを設定
        /// </summary>
        /// <param name="state">グローバルステート</param>
        public void SetGlobalState(State<T> state)
        {
            // 既存のグローバルステートがあれば終了処理
            if (m_GlobalState != null)
                m_GlobalState.Exit();

            m_GlobalState = state;
            if (m_GlobalState != null)
                m_GlobalState.Enter();
        }

        /// <summary>
        /// グローバルステートを取得
        /// </summary>
        public State<T> GetGlobalState()
        {
            return m_GlobalState;
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void ClearHistory()
        {
            m_StateHistory.Clear();
        }

        /// <summary>
        /// 履歴の最大数を設定
        /// </summary>
        /// <param name="count">最大数</param>
        public void SetMaxHistoryCount(int count)
        {
            m_MaxHistoryCount = count;
            TrimHistory();
        }

        /// <summary>
        /// 履歴を最大数に合わせて削除
        /// </summary>
        private void TrimHistory()
        {
            if (m_StateHistory.Count <= m_MaxHistoryCount)
                return;

            // スタックを一時的に反転して古いものを削除
            var tempList = new List<State<T>>();
            while (m_StateHistory.Count > 0)
                tempList.Add(m_StateHistory.Pop());

            // 新しいものだけをスタックに戻す
            m_StateHistory.Clear();
            int startIndex = tempList.Count - m_MaxHistoryCount;
            for (int i = tempList.Count - 1; i >= startIndex; i--)
                m_StateHistory.Push(tempList[i]);
        }

        /// <summary>
        /// 毎フレーム実行されるいつものUpdate()
        /// グローバルステート→現在のステート→サブステートの順に実行
        /// </summary>
        public void Update()
        {
            // グローバルステートを先に実行
            if (m_GlobalState != null)
                m_GlobalState.Stay();

            // 現在のステートが存在している
            if (m_CurrentState != null)
            {
                // 現在のステートを実行する
                m_CurrentState.Stay();

                // サブステートがあれば更新
                if (m_CurrentState.HasSubStates())
                    m_CurrentState.UpdateSubStates();
            }
        }
    }
}
