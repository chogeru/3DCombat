using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StateMachineAI
{
    /// <summary>
    /// ステートを持つオブジェクトの基底
    /// abstract class によって、継承が成立する
    /// 
    /// 【拡張機能】
    /// - ステート履歴スタック: RevertToPreviousState()で前のステートに戻る
    /// - 遷移イベント通知: SubscribeStateChanged()で監視
    /// - グローバルステート: SetGlobalState()で常に実行されるステートを設定
    /// </summary>
    public abstract class StatefulObjectBase<T, TEnum> : MonoBehaviour
        where T : class where TEnum : System.IConvertible
    {
        /// <summary>
        ///登録されるステートのリストデータ
        ///ここで登録されていない場合は、ステート遷移が出来ない
        /// <summary>
        public List<State<T>> stateList = new List<State<T>>();

        /// <summary>
        ///ステートマシーンの登録
        /// <summary>
        protected StateMachine<T> stateMachine;

        /// <summary>
        ///ステートの切り替え
        ///ステートを遷移させる為の関数
        ///対象となるステート名(enum型)に対応している。
        /// <summary>
        public virtual void ChangeState(TEnum state)
        {
            ///ステートマシーン内がnullの場合
            if (stateMachine == null)
            {
                ///無いから戻れ、慈悲はない。イヤャャャャヤ!!
                ///つまり、遷移したくても遷移できないので弾く
                return;
            }
            ///該当するステートをステートマシーンのステートとして登録する
            ///つまり、ステート切り替え実行される
            stateMachine.ChangeState(stateList[state.ToInt32(null)]);
        }

        /// <summary>
        ///まぁ、使っていないけど…
        ///現在のステートが、新しいステートと同じかどうかをチェックする
        /// <summary>
        public virtual bool IsCurrentState(TEnum state)
        {
            ///ステートマシーン内がnullの場合
            if (stateMachine == null)
            {
                ///無いから戻れ、慈悲はない。イヤャャャャヤ!!
                ///つまり、遷移したくても遷移できないので弾く
                return false;
            }
            ///現在のステートマシンで稼働しているステートと、指定したステートが同じかどうかをBool値で返す
            return stateMachine.CurrentState == stateList[state.ToInt32(null)];
        }

        /// <summary>
        /// ステートマシンのアップデート(毎回実行)を行う
        /// </summary>
        protected virtual void Update()
        {
            ///ステートマシーン内がnullではない
            if (stateMachine != null)
            {
                ///ステートマシーンを実行する
                ///つまり、現在のステートにあるUpdate()を実行させる
                stateMachine.Update();
            }
        }

        // ========== 拡張機能ラッパーメソッド ==========

        /// <summary>
        /// 前のステートに戻る
        /// </summary>
        /// <returns>戻ることができたらtrue</returns>
        public virtual bool RevertToPreviousState()
        {
            if (stateMachine == null)
                return false;
            return stateMachine.RevertToPreviousState();
        }

        /// <summary>
        /// グローバルステートを設定
        /// </summary>
        /// <param name="globalState">グローバルステート</param>
        public virtual void SetGlobalState(State<T> globalState)
        {
            if (stateMachine != null)
                stateMachine.SetGlobalState(globalState);
        }

        /// <summary>
        /// グローバルステートを取得
        /// </summary>
        /// <returns>グローバルステート</returns>
        public virtual State<T> GetGlobalState()
        {
            return stateMachine?.GetGlobalState();
        }

        /// <summary>
        /// ステート変更イベントを購読
        /// </summary>
        /// <param name="handler">イベントハンドラ (前のステート, 新しいステート)</param>
        public void SubscribeStateChanged(System.Action<State<T>, State<T>> handler)
        {
            if (stateMachine != null)
                stateMachine.OnStateChanged += handler;
        }

        /// <summary>
        /// ステート変更イベントの購読を解除
        /// </summary>
        /// <param name="handler">解除するイベントハンドラ</param>
        public void UnsubscribeStateChanged(System.Action<State<T>, State<T>> handler)
        {
            if (stateMachine != null)
                stateMachine.OnStateChanged -= handler;
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void ClearStateHistory()
        {
            if (stateMachine != null)
                stateMachine.ClearHistory();
        }

        /// <summary>
        /// 履歴の最大数を設定
        /// </summary>
        /// <param name="count">最大数</param>
        public void SetMaxHistoryCount(int count)
        {
            if (stateMachine != null)
                stateMachine.SetMaxHistoryCount(count);
        }

        /// <summary>
        /// 履歴の数を取得
        /// </summary>
        /// <returns>履歴の数</returns>
        public int GetHistoryCount()
        {
            return stateMachine?.HistoryCount ?? 0;
        }
    }
}
