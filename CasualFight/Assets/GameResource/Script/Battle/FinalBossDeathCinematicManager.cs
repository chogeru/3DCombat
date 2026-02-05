using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;
using StateMachineAI;

/// <summary>
/// ラスボス撃破時の演出管理マネージャー
/// 改修版：待機ロジックを廃止し、ボスのDissolveControllerに委譲
/// さらに改修：自身は削除せず、演出用オブジェクトの消滅を監視する
/// </summary>
public class FinalBossDeathCinematicManager : MonoBehaviour
{
    [Header("監視対象のラスボス")]
    [SerializeField] AITester m_BossEnemy;

    [Header("演出用カメラ")]
    [SerializeField] CinemachineVirtualCamera m_TargetCamera;

    [Header("演出用アニメーター")]
    [SerializeField] Animator m_CinematicAnimator;

    [Header("Barkトリガー名")]
    [SerializeField] string m_BarkTriggerName = "Bark";

    [Header("演出後に有効化するオブジェクト（監視対象）")]
    [SerializeField] GameObject m_EnableTargetObject;

    private void Start()
    {
        // 既に設定されている場合は即購読
        if (m_BossEnemy != null)
        {
            m_BossEnemy.OnDeathEvent += OnBossDead;
        }
        else
        {
            // 設定されていない場合は自動検索を開始
            SearchBossLoop().Forget();
        }

        // 初期状態の確認
        if (m_TargetCamera != null)
        {
            m_TargetCamera.Priority = 0;
            m_TargetCamera.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 1秒ごとにラスボス（m_IsFinalBoss=true）を探すループ
    /// </summary>
    async UniTaskVoid SearchBossLoop()
    {
        Debug.Log("FinalBossDeathCinematicManager: ラスボスの自動検索を開始します...");

        while (m_BossEnemy == null)
        {
            // 1秒待機
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.0f));

            // 自身の破棄チェック
            if (this == null) return;

            // ラスボス探索
            var allEnemies = FindObjectsOfType<AITester>();
            foreach (var enemy in allEnemies)
            {
                // IsFinalBossフラグが立っていて、かつまだ生きている敵を対象とする
                if (enemy != null && enemy.m_IsFinalBoss && !enemy.m_IsDead)
                {
                    m_BossEnemy = enemy;
                    m_BossEnemy.OnDeathEvent += OnBossDead;
                    Debug.Log($"FinalBossDeathCinematicManager: ラスボス({enemy.name})を発見し、追跡を開始しました。");
                    return; // ループ終了
                }
            }
        }
    }

    private void OnDestroy()
    {
        // イベント解除
        if (m_BossEnemy != null)
        {
            m_BossEnemy.OnDeathEvent -= OnBossDead;
        }
    }

    // 演出済みフラグ
    bool m_IsPlayed = false;

    /// <summary>
    /// ボス死亡時に呼ばれる演出処理
    /// </summary>
    void OnBossDead()
    {
        if (m_IsPlayed) return;
        m_IsPlayed = true;

        Debug.Log("FinalBossDeathCinematicManager: ラスボス撃破、演出を開始します。");

        // 即座に演出開始
        StartCinematicSequence().Forget();
    }

    async UniTaskVoid StartCinematicSequence()
    {
        // UI非表示のためにステートをイベントに変更
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ChangeState(GameStateManager.GameState.Event);
        }

        // 全ての敵をフリーズ
        SetAllEnemiesFreeze(true);
        // プレイヤーの操作ロック
        SetPlayerLock(true);

        // 1. カメラを有効化し、優先度を999に変更
        if (m_TargetCamera != null)
        {
            m_TargetCamera.gameObject.SetActive(true);
            // カメラの優先度を上げて切り替え
            m_TargetCamera.Priority = 999;
        }

        // 2. アニメーターのトリガーBarkを1回だけ呼び出す
        if (m_CinematicAnimator != null)
        {
            m_CinematicAnimator.SetTrigger(m_BarkTriggerName);
        }

        // 3. 指定した演出用オブジェクトの有効化
        if (m_EnableTargetObject != null)
        {
            m_EnableTargetObject.SetActive(true);
        }

        // 4. ボスのアニメーション再生とDissolve開始
        // AITester（戦闘用ボス）は即削除されている可能性があるため、
        // 演出用オブジェクト（m_EnableTargetObject）に対してアニメーション指示を送る
        if (m_EnableTargetObject != null)
        {
            var dissolveController = m_EnableTargetObject.GetComponent<FinalBossDissolveController>();
            if (dissolveController != null)
            {
                dissolveController.PlayDeathAnimation();
            }
            else
            {
                // まだAITesterが生きている、あるいはAITester側で演出する場合のバックアップ
                if (m_BossEnemy != null)
                {
                    var enemyController = m_BossEnemy.GetComponent<FinalBossDissolveController>();
                    if (enemyController != null)
                    {
                        enemyController.PlayDeathAnimation();
                    }
                }
            }
        }
        else if (m_BossEnemy != null)
        {
             // 演出用オブジェクトが無い場合は旧仕様通りボス自身を操作
            var dissolveController = m_BossEnemy.GetComponent<FinalBossDissolveController>();
            if (dissolveController != null)
            {
                dissolveController.PlayDeathAnimation();
            }
        }

        // 5. 演出用オブジェクトが消滅するまで監視する
        // m_EnableTargetObject が演出用ボスそのものである場合、これが消える（Destroyまたは非アクティブ）のを待つ。
        Debug.Log($"FinalBossDeathCinematicManager: 監視対象({m_EnableTargetObject?.name})の消滅を待ちます...");

        if (m_EnableTargetObject != null)
        {
            // オブジェクトが破棄される(nullになる)まで待機
            // ※Activeがfalseになるのを待つか、Destroyでnullになるのを待つか。
            // DissolveControllerはDestroy(gameObject)しているので、nullチェックでOK。
            await UniTask.WaitWhile(() => m_EnableTargetObject != null, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        else
        {
            // オブジェクトが無い場合は適当な時間で終わる（あるいは即終了）
            Debug.LogWarning("FinalBossDeathCinematicManager: 監視対象が設定されていません。安全のため2秒待機します。");
            await UniTask.Delay(System.TimeSpan.FromSeconds(2.0f));
        }

        Debug.Log("FinalBossDeathCinematicManager: 監視対象の消滅を確認しました。終了処理を実行します。");
        
        // 6. 終了処理

        // カメラを戻す
        if (m_TargetCamera != null)
        {
            m_TargetCamera.Priority = 0;
            m_TargetCamera.gameObject.SetActive(false);
        }

        // 全ての敵のフリーズ解除
        SetAllEnemiesFreeze(false);
        // プレイヤーの操作ロック解除
        SetPlayerLock(false);

        // UI表示を戻すためにステートを探索に戻す
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ChangeState(GameStateManager.GameState.Exploration);
        }

        // ※Manager自身は削除しない
        Debug.Log("FinalBossDeathCinematicManager: 演出終了。");
    }

    /// <summary>
    /// 全ての敵(AITester)のフリーズ状態を設定する
    /// </summary>
    void SetAllEnemiesFreeze(bool isFreeze)
    {
        var enemies = FindObjectsOfType<AITester>();
        foreach (var enemy in enemies)
        {
            // 生きている敵のみ対象
            if (enemy != null && !enemy.m_IsDead)
            {
                enemy.SetFreeze(isFreeze);
            }
        }
    }

    /// <summary>
    /// プレイヤーのイベントロック状態を設定する
    /// </summary>
    void SetPlayerLock(bool isLocked)
    {
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetEventLock(isLocked);
        }
    }
}
