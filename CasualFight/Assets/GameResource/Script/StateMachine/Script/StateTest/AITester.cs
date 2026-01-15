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

        //自分のHP
        public int m_EnemyHP;

        //ヒット時にエフェクトを出す場所
        public Transform m_HitPosition;

        //死亡判定フラグ
        public bool m_IsDead = false;

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
        /// ダメージ処理
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(int damage)
        {
            if(m_IsDead)
                return;

            //減算処理
            m_EnemyHP = Mathf.Clamp(m_EnemyHP - damage, 0, m_EnemyData.m_MaxHp);

            //HPが0になると
            if(m_EnemyHP==0)
            {
                ChangeState(AIState_Type.Die);
            }
            else
            {
                //0じゃなければ
                ChangeState(AIState_Type.Hit);
            }
        }

        /// <summary>
        /// 起動時にセットアップする一覧
        /// </summary>
        public void AISetUp()
        {
            Debug.Log($"{nameof(AISetUp)}起動", this);

            // コンポーネントの取得
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();

            // EnemyDataのnullチェック
            if (m_EnemyData == null)
            {
                Debug.LogError("m_EnemyData が設定されていません！", this);
                return;
            }

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<AITester>();

            //HP代入
            m_EnemyHP = m_EnemyData.m_MaxHp;

            //初期起動時は、Idleに移行させる
            ChangeState(AIState_Type.Idle);
        }
    }
}
