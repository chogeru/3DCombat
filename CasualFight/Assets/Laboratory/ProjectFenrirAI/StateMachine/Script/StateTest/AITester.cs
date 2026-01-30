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
        Return,     //初期位置へ帰還
    }

    public class AITester : StatefulObjectBase<AITester, AIState_Type>, IDamageable
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

        // フリーズ状態フラグ
        bool m_IsFrozen = false;

        // 索敵中かどうかのフラグ
        public bool m_IsSearching = true;

        // テスト用カウンタ
        public int m_Counter = 0;

        // 初期位置情報
        public Vector3 m_SpawnPosition { get; private set; }
        public Quaternion m_SpawnRotation { get; private set; }

        // DissolveController
        public EnemyDissolveController m_DissolveController { get; private set; }

        /// <summary>
        /// コンポーネントの初期化（StateManagerから呼ばれる）
        /// </summary>
        public void Initialize()
        {
            Debug.Log($"{nameof(Initialize)}起動", this);

            // コンポーネントの取得
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_DissolveController = GetComponent<EnemyDissolveController>();

            // EnemyDataのnullチェック
            if (m_EnemyData != null)
            {
                m_EnemyHP = m_EnemyData.m_MaxHp;
            }

            // 初期位置・回転を保存
            m_SpawnPosition = transform.position;
            m_SpawnRotation = transform.rotation;

            // ステートマシンの初期化
            stateMachine = new StateMachine<AITester>();
            stateList.Clear();

            // S_Return は固定で追加（Inspector設定に依存しないため）
            stateList.Add(new S_Return(this));
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

        // ... (省略)

        protected override void Update()
        {
            // フリーズ中は更新しない（アニメーションも止まるが、念のため論理更新も止める）
            if (m_IsFrozen) return;

            base.Update();
        }

        // IDamageableの実装
        public void SetFreeze(bool isFrozen)
        {
            m_IsFrozen = isFrozen;

            // アニメーションの停止/再開
            if (m_Animator != null)
            {
                m_Animator.speed = isFrozen ? 0 : 1;
            }

            // 必要であればRigidbodyの停止処理などもここに追加
            if (m_Rigidbody != null && isFrozen)
            {
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// ステート切り替えのオーバーライド
        /// indexでの指定ではなく、型で検索して遷移させる（リストの順序に依存しないようにする）
        /// </summary>
        public override void ChangeState(AIState_Type state)
        {
            if (stateMachine == null) return;

            State<AITester> targetState = null;

            // Enum に対応するクラスをリストから検索
            switch (state)
            {
                case AIState_Type.Idle: targetState = stateList.Find(s => s is S_Idle); break;
                case AIState_Type.Attack: targetState = stateList.Find(s => s is S_Attack); break;
                case AIState_Type.Tracking: targetState = stateList.Find(s => s is S_Tracking); break;
                case AIState_Type.Hit: targetState = stateList.Find(s => s is S_Hit); break;
                case AIState_Type.Die: targetState = stateList.Find(s => s is S_Die); break;
                case AIState_Type.Retreat: targetState = stateList.Find(s => s is S_Retreat); break;
                case AIState_Type.Return: targetState = stateList.Find(s => s is S_Return); break;
            }

            if (targetState != null)
            {
                stateMachine.ChangeState(targetState);
            }
            else
            {
                Debug.LogWarning($"AITester: {state} ステートが見つかりませんでした。Inspectorの設定を確認してください。");
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
        /// アニメーションイベントから呼ばれる攻撃判定処理
        /// Animation Eventの設定: Function = CheckHit, Int = 0 (Light) or 1 (Heavy)
        /// </summary>
        public void CheckHit(int severityValue)
        {
            if (m_EnemyData == null) return;

            // int -> Enum 変換
            HitSeverity severity = (HitSeverity)severityValue;

            // 自身を中心に判定を行う
            Vector3 center = transform.position + transform.forward * (m_EnemyData.m_AttackRange * 0.5f);
            float radius = m_EnemyData.m_AttackRange * 0.5f;

            // レイヤーマスクを使用するように変更
            LayerMask targetLayer = m_EnemyData.m_TargetLayer;
            // もしLayerMaskが何も設定されていなければ（0なら）、SafetyとしてPlayerタグ判定用コードを残すか、
            // あるいはDefaultを含むすべてを対象にするか等は考慮必要だが、今回は設定済み前提で進める。
            // ただし、LayerMaskが0(Nothing)だと何もヒットしないため、未設定時は従来のOverlapSphere(全レイヤー対象)にする手もあるが、
            // ここでは実装計画通り LayerMask を引数に渡す。
            
            Collider[] hitColliders;
            if (targetLayer.value != 0)
            {
                hitColliders = Physics.OverlapSphere(center, radius, targetLayer);
            }
            else
            {
                // LayerMask未設定時は従来どおり全レイヤー取得してタグ判定させる（互換性維持）
                hitColliders = Physics.OverlapSphere(center, radius);
            }

            foreach (var hitCollider in hitColliders)
            {
                // 自分自身は無視
                if (hitCollider.gameObject == gameObject) continue;

                // レイヤー指定がある場合はタグ判定不要だが、LayerMaskが未設定の場合のバックアップとしてタグ判定も残す
                bool isTarget = false;
                if (targetLayer.value != 0)
                {
                    // レイヤーマスクでフィルタリング済みなので対象とみなす
                    isTarget = true;
                }
                else if (hitCollider.CompareTag("Player"))
                {
                    isTarget = true;
                }

                if (isTarget)
                {
                    // ヒットエフェクト生成処理
                    if (m_EnemyData.m_HitEffectPrefab != null)
                    {
                         // 接触点（近似）を計算
                        Vector3 preciseHitPoint = hitCollider.ClosestPoint(transform.position + transform.forward + Vector3.up);
                        Vector3 effectPos = preciseHitPoint;

                        // PlayerControllerを持っていてHitPositionがあれば使う
                        // HitController経由でPlayerController取れるか確認、あるいは直接取る
                        if (hitCollider.TryGetComponent<PlayerController>(out var player) && player.m_HitPosition != null)
                        {
                             effectPos.y = player.m_HitPosition.position.y;
                        }
                        // 親や兄弟にPlayerControllerがある場合も考慮したいが、ひとまず上記で実装

                        GameObject effect = Instantiate(m_EnemyData.m_HitEffectPrefab, effectPos, Quaternion.identity);
                        Destroy(effect, 0.5f);
                    }

                    // ダメージ処理
                    var playerHit = hitCollider.GetComponent<PlayerHitController>();
                    if (playerHit != null)
                    {
                        // ダメージ値、攻撃者の位置、Severity
                        playerHit.OnDamage(m_EnemyData.m_AttackDamage, transform.position, severity);
                        Debug.Log($"プレイヤーに攻撃 Hit! Severity: {severity}");
                    }
                }
            }
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
