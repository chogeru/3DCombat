using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くでなきい。
    /// </summary>
    /// 
    public enum AIState_Type
    {
        Idle,//待機
        Attack,//攻撃
        Search,//索敵
        Tracking,//追跡
        Hit,//被弾
        Die,//死亡
    }


    public class AITester 
        : StatefulObjectBase<AITester, AIState_Type>
    {
        //自分のアニメーター
        public Animator m_Animator { get; private set; }

        //自分のRigidbody 
        public Rigidbody m_Rigidbody { get; private set; }

        [Header("敵の固有設定データ")]
        public EnemyData m_EnemyData;

        //PlayerのTransform
        public Transform m_Player { get; set; }

        /// <summary>
        /// クラス名を元にステートを生成して追加する
        /// </summary>
        /// <param name="ClassName">生成するクラスの名前</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // 現在のアセンブリからクラスを取得
                //Type StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");
                Type StateType = Assembly.GetExecutingAssembly().GetType($"{ClassName}");

                // クラスが見つからなかった場合の対処
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} クラスが見つかりませんでした。");
                    return true;
                }

                // 型が State<AITester> かどうかをチェック
                if (!typeof(State<AITester>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(AITester) });
                

                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<AITester> StateInstance = 
                    Constructor.Invoke(new object[] { this }) as State<AITester>;

                if (StateInstance != null)
                {
                    // ステートリストに追加
                    stateList.Add(StateInstance);
                    Debug.Log($"{ClassName} をステートリストに追加しました。");
                    return true;
                }
                else
                {
                    Debug.LogError($"{ClassName} のインスタンス生成に失敗しました。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"エラーが発生しました。: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// アニメーションイベントから呼び出される攻撃判定用関数
        /// </summary>
        public void AnimEvent_AttackHit()
        {
            // 現在のステートが State_Attack なら判定処理を実行
            // State_Attack クラスにキャストしてメソッドを呼ぶ
            if (stateMachine != null && stateMachine.CurrentState is State_Attack attackState)
            {
                attackState.OnCheckHit();
            }
        }

        /// <summary>
        /// 起動時にセットアップする一覧
        /// </summary>
        public void AISetUp()
        {

            Debug.Log($"{nameof(AISetUp)}起動", this);
            //ステートマシACーンを自身として設定
            stateMachine = new StateMachine<AITester>();

            //初期起動時は、「???」に移行させる
            ChangeState(AIState_Type.Idle);
        }
    }
}
