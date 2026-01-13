using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StateMachineAI
{
    /// <summary>
    /// ステートを持つオブジェクトの基底
    /// abstract class によって、継承が成立する
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
    }
}
