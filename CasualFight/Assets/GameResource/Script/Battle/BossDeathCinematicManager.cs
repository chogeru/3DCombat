using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;
using StateMachineAI;

/// <summary>
/// 中ボス撃破時の演出管理マネージャー
/// </summary>
public class BossDeathCinematicManager : MonoBehaviour
{
    [Header("監視対象の中ボス")]
    [SerializeField] AITester m_BossEnemy;

    [Header("演出用カメラ")]
    [SerializeField] CinemachineVirtualCamera m_TargetCamera;

    [Header("演出用アニメーター")]
    [SerializeField] Animator m_CinematicAnimator;

    [Header("Barkトリガー名")]
    [SerializeField] string m_BarkTriggerName = "Bark";

    [Header("演出後に有効化するオブジェクト")]
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

        // 初期状態の確認（カメラ等は無効化しておくべきだが、Inspector設定に任せる）
        if (m_TargetCamera != null)
        {
            m_TargetCamera.Priority = 0;
            m_TargetCamera.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 1秒ごとに中ボス（m_IsBoss=true）を探すループ
    /// </summary>
    async UniTaskVoid SearchBossLoop()
    {
        Debug.Log("BossDeathCinematicManager: 中ボスの自動検索を開始します...");

        while (m_BossEnemy == null)
        {
            // 1秒待機
            await UniTask.Delay(System.TimeSpan.FromSeconds(1.0f));

            // 自身の破棄チェック
            if (this == null) return;

            // 中ボス探索
            var allEnemies = FindObjectsOfType<AITester>();
            foreach (var enemy in allEnemies)
            {
                // IsBossフラグが立っていて、かつまだ生きている敵を対象とする
                if (enemy != null && enemy.m_IsBoss && !enemy.m_IsDead)
                {
                    m_BossEnemy = enemy;
                    m_BossEnemy.OnDeathEvent += OnBossDead;
                    Debug.Log($"BossDeathCinematicManager: 中ボス({enemy.name})を発見し、追跡を開始しました。");
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

        Debug.Log("BossDeathCinematicManager: 中ボス撃破、演出を開始します。");
        PlayCinematicSequence().Forget();
    }

    async UniTaskVoid PlayCinematicSequence()
    {
        // 1. カメラを有効化し、優先度を999に変更
        if (m_TargetCamera != null)
        {
            m_TargetCamera.gameObject.SetActive(true);
            m_TargetCamera.Priority = 999;

            // ノイズ（シェイク）コンポーネントの取得
            var noise = m_TargetCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                // シェイク開始（数値は仮置き、Inspectorで設定できるようにするとベスト）
                noise.m_AmplitudeGain = 2.0f; // 揺れの大きさ
                noise.m_FrequencyGain = 2.0f; // 揺れの速さ
                Debug.Log("BossDeathCinematicManager: カメラシェイク開始");
            }
            else
            {
                Debug.LogWarning("BossDeathCinematicManager: VirtualCameraにNoise(Perlin)が設定されていません。シェイクできません。");
            }
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
            // シェイク停止
            var noise = m_TargetCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                noise.m_AmplitudeGain = 0f;
                noise.m_FrequencyGain = 0f;
            }

            m_TargetCamera.Priority = 0;
            m_TargetCamera.gameObject.SetActive(false);
        }

        // 4. その後に指定したオブジェクトのSetActiveをtrueにしたい
        if (m_EnableTargetObject != null)
        {
            m_EnableTargetObject.SetActive(true);
            Debug.Log($"BossDeathCinematicManager: {m_EnableTargetObject.name} を有効化しました。");
        }
    }
}
