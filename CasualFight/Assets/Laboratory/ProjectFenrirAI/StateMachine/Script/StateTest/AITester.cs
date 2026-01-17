using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    public enum AIState_Type
    {
        Idle,       //待機
        Attack,     //攻撃
        Tracking,   //追跡
        Hit,        //被弾
        Die,        //死亡
        Retreat,    //後退
    }

    public class AITester : StatefulObjectBase<AITester, AIState_Type>
    {
        // 自分のアニメーター
        public Animator m_Animator { get; private set; }

        // 自分のRigidbody
        public Rigidbody m_Rigidbody { get; private set; }

        [Header("敵の固有設定データ")]
        public EnemyData m_EnemyData;

        // PlayerのTransform
        public Transform m_Player;

        // 自分のHP
        public int m_EnemyHP;

        // ヒット時にエフェクトを出す場所
        public Transform m_HitPosition;

        // 死亡判定フラグ
        public bool m_IsDead = false;

        // 索敵中かどうかのフラグ
        public bool m_IsSearching = true;

        // テスト用カウンタ
        public int m_Counter = 0;

        /// <summary>
        /// コンポーネントの初期化（StateManagerから呼ばれる）
        /// </summary>
        public void Initialize()
        {
            Debug.Log($"{nameof(Initialize)}起動", this);

            // コンポーネントの取得
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();

            // EnemyDataのnullチェック
            if (m_EnemyData != null)
            {
                m_EnemyHP = m_EnemyData.m_MaxHp;
            }

            // ステートマシンの初期化
            stateMachine = new StateMachine<AITester>();
            stateList.Clear();
        }

        /// <summary>
        /// グローバルステートの設定（StateManagerから呼ばれる）
        /// </summary>
        /// <param name="globalState">設定するグローバルステート</param>
        public void SetupGlobalState(State<AITester> globalState)
        {
            Debug.Log($"{nameof(SetupGlobalState)}起動: {globalState.GetType().Name}", this);
            SetGlobalState(globalState);
        }

        /// <summary>
        /// クラス名を元にグローバルステートを設定する
        /// </summary>
        /// <param name="className">グローバルステートのクラス名</param>
        /// <returns>成功したらtrue</returns>
        public bool SetupGlobalStateByName(string className)
        {
            try
            {
                // 名前空間を含めて検索
                Type stateType = Type.GetType($"StateMachineAI.{className}");
                if (stateType == null)
                {
                    stateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{className}");
                }
                if (stateType == null)
                {
                    stateType = Assembly.GetExecutingAssembly().GetType(className);
                }

                if (stateType == null)
                {
                    Debug.LogError($"{className} クラスが見つかりませんでした。");
                    return false;
                }

                if (!typeof(State<AITester>).IsAssignableFrom(stateType))
                {
                    Debug.LogError($"{className} は State<AITester> 型ではありません。");
                    return false;
                }

                ConstructorInfo constructor = stateType.GetConstructor(new[] { typeof(AITester) });
                if (constructor == null)
                {
                    Debug.LogError($"{className} のコンストラクタが見つかりませんでした。");
                    return false;
                }

                State<AITester> stateInstance = constructor.Invoke(new object[] { this }) as State<AITester>;
                if (stateInstance != null)
                {
                    SetGlobalState(stateInstance);
                    Debug.Log($"{className} をグローバルステートに設定しました。");
                    return true;
                }
                else
                {
                    Debug.LogError($"{className} のインスタンス生成に失敗しました。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"エラーが発生しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ステートマシンの開始（StateManagerから呼ばれる）
        /// </summary>
        /// <param name="initialState">初期ステート（デフォルト: Idle）</param>
        public void StartStateMachine(AIState_Type initialState = AIState_Type.Idle)
        {
            Debug.Log($"{nameof(StartStateMachine)}起動: 初期ステート={initialState}", this);

            // ステート変更イベントの購読 (ログ出力)
            SubscribeStateChanged((prev, next) =>
            {
                string prevName = prev != null ? prev.GetType().Name : "null";
                string nextName = next != null ? next.GetType().Name : "null";
                Debug.Log($"[StateChanged] {prevName} -> {nextName}");
            });

            // 初期ステートへ遷移
            ChangeState(initialState);
        }


        /// <summary>
        /// ダメージ処理
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(int damage)
        {
            if (m_IsDead)
                return;

            if (m_EnemyData != null)
            {
                // 減算処理
                m_EnemyHP = Mathf.Clamp(m_EnemyHP - damage, 0, m_EnemyData.m_MaxHp);

                // HPが0になると
                if (m_EnemyHP == 0)
                {
                    ChangeState(AIState_Type.Die);
                }
                else
                {
                    // 0じゃなければ
                    ChangeState(AIState_Type.Hit);
                }
            }
        }

        /// <summary>
        /// クラス名を元にステートを生成して追加する
        /// </summary>
        /// <param name="ClassName">生成するクラスの名前</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // 名前空間を含めて検索するか、そのまま検索するか
                // StateTestのスクリプトは StateMachineAI 名前空間内にある
                Type StateType = Type.GetType($"StateMachineAI.{ClassName}");
                if (StateType == null)
                {
                    // 見つからない場合、現在の実行アセンブリから探す
                    StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");
                }
                if (StateType == null)
                {
                     StateType = Assembly.GetExecutingAssembly().GetType(ClassName);
                }


                // クラスが見つからなかった場合の対処
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} クラスが見つかりませんでした。");
                    return true;
                }

                // 型が State<AITester> かどうかをチェック
                if (!typeof(State<AITester>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<AITester> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                ConstructorInfo Constructor = StateType.GetConstructor(new[] { typeof(AITester) });

                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<AITester> StateInstance = Constructor.Invoke(new object[] { this }) as State<AITester>;

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
    }
}
