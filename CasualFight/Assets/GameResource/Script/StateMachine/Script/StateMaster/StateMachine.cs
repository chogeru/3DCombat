namespace StateMachineAI
{
    /// <summary>
    /// ステートマシン
    /// ステートの本来のプログラムで、ステートに与えられた稼働関数は
    /// ここで実行指示を受けている。
    /// ステートの稼働関数を増やす場合、ここも手を入れなければならないが
    /// 基本、ChangeState、Enter、Execute、Exitで事足りる。
    /// </summary>
    public class StateMachine<T>
    {
        /// <summary>
        /// ステート
        /// </summary>
        private State<T> currentState;

        /// <summary>
        /// コンストラクタ
        /// 現在のステートをnullにして無効化する
        /// </summary>
        public StateMachine()
        {
            ///現在のステートをnullにして初期化する
            currentState = null;
        }

        /// <summary>
        /// 現在のステートを呼び出す
        /// </summary>
        public State<T> CurrentState
        {
            //返し値を同ステートで返す
            get { return currentState; }
        }

        /// <summary>
        /// 該当するステートに変更する
        /// </summary>
        /// <param name="state">　遷移する先のステート</param>
        public void ChangeState(State<T> state)
        {
            // 現在のステートが存在している
            if (currentState != null)
            {
                // ステートの遷移の為、現在のステートの終了処理を実行
                // 現在のステートのExit関数を呼び出す
                currentState.Exit();
            }
            // 現在のステートを新しいステートに変更する
            currentState = state;
            // 現在のステートのEnter関数を呼び出す
            // つまり、ステートのコンストラクタを起動させる
            currentState.Enter();
        }

        /// <summary>
        /// 毎フレーム実行されるいつものUpdate()
        /// 現在のステートの毎フレーム実行を実行
        /// <summary>
        public void Update()
        {
            // 現在のステートが存在している
            if (currentState != null)
            {
                // 現在のステートを実行する
                // ただそれだけ
                currentState.Stay();
            }
        }
    }
}


