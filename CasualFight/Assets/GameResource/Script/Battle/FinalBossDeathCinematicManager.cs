using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;
using StateMachineAI;

/// <summary>
/// ラスボス撃破時の演出管理マネージャー
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

    [Header("演出後に有効化するオブジェクト")]
    [SerializeField] GameObject m_EnableTargetObject;

    [Header("演出後に削除するオブジェクト")]
    [SerializeField] GameObject m_DestroyTargetObject;

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

        // 初期状態の確認（カメラ等は無効化しておくべきだが、Inspector設定に任せる）
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
        PlayCinematicSequence().Forget();
    }

    async UniTaskVoid PlayCinematicSequence()
    {
        // ボス(AITester)が消滅するまで待機
        // UnityのObjectはDestroyされるとnull扱いになるため、これを利用して待機します
        while (m_BossEnemy != null)
        {
            // 1フレーム待機して再チェック
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        Debug.Log("FinalBossDeathCinematicManager: ラスボスの消滅を確認しました。演出を開始します。");

        // 全ての敵をフリーズ
        SetAllEnemiesFreeze(true);
        // プレイヤーの操作ロック
        SetPlayerLock(true);

        // 1. カメラを有効化し、優先度を999に変更
        if (m_TargetCamera != null)
        {
            m_TargetCamera.gameObject.SetActive(true);
            m_TargetCamera.Priority = 999;
        }

        // 2. アニメーターのトリガーBarkを1回だけ呼び出す
        if (m_CinematicAnimator != null)
        {
            m_CinematicAnimator.SetTrigger(m_BarkTriggerName);
            
            // アニメーションの遷移と終了を待機する
            // 遷移待ち
            await UniTask.DelayFrame(1); 
            
            // 現在のステート情報を取得して長さを得る (Barkステートに遷移している前提)
            AnimatorStateInfo stateInfo = m_CinematicAnimator.GetCurrentAnimatorStateInfo(0);
            
            // もし遷移中なら、遷移先の情報を取る
            if (m_CinematicAnimator.IsInTransition(0))
            {
                stateInfo = m_CinematicAnimator.GetNextAnimatorStateInfo(0);
            }

            // stateInfo.length 分だけ待機 (長さが取得できない場合は安全策で2秒)
            float waitTime = stateInfo.length > 0 ? stateInfo.length : 2.0f;
            Debug.Log($"演出アニメーション待機: {waitTime}秒");
            
            await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
        }
        else
        {
            // アニメーターが無い場合は適当な待機時間
            await UniTask.Delay(System.TimeSpan.FromSeconds(2.0f));
        }

        // 3. アニメーション終了時バーチャルカメラを優先度0にした後、falseにする
        if (m_TargetCamera != null)
        {
            m_TargetCamera.Priority = 0;
            m_TargetCamera.gameObject.SetActive(false);
        }

        // 全ての敵のフリーズ解除
        SetAllEnemiesFreeze(false);
        // プレイヤーの操作ロック解除
        SetPlayerLock(false);

        // 4. その後に指定したオブジェクトのSetActiveをtrueにしたい
        if (m_EnableTargetObject != null)
        {
            m_EnableTargetObject.SetActive(true);
            Debug.Log($"FinalBossDeathCinematicManager: {m_EnableTargetObject.name} を有効化しました。");
        }

        Debug.Log("FinalBossDeathCinematicManager: 全ての処理が完了したので指定オブジェクトを削除します。");

        // 指定されたオブジェクトを削除
        if (m_DestroyTargetObject != null)
        {
            Destroy(m_DestroyTargetObject);
        }

        Debug.Log("FinalBossDeathCinematicManager: 自身を削除します。");
        Destroy(gameObject);
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
