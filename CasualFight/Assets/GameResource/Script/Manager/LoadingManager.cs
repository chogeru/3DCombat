using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks; // UniTaskをインポート
using System;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup m_LoadingCanvasGroup; // フェード用
    [SerializeField] private Slider m_ProgressBar;            // 進捗バー

    [Header("Settings")]
    [SerializeField] private float m_FadeDuration = 0.5f;    // フェード時間
    [SerializeField] private float m_MinLoadingTime = 1.0f;  // 最低表示時間（一瞬で終わるのを防ぐ）

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            m_LoadingCanvasGroup.alpha = 0;
            m_LoadingCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンを非同期でロードする
    /// </summary>
    public async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        // 1. ロード画面を表示し、フェードイン
        m_LoadingCanvasGroup.gameObject.SetActive(true);
        await FadeAsync(1.0f);

        // 2. 非同期ロード開始
        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false; // 読み込み完了しても勝手に切り替えない

        float startTime = Time.time;

        // 3. ロード進捗を監視
        while (asyncOp.progress < 0.9f)
        {
            if (m_ProgressBar != null)
            {
                // progress(0~0.9)を0~1に補正してバーに反映
                m_ProgressBar.value = asyncOp.progress / 0.9f;
            }
            await UniTask.Yield(); // 1フレーム待機
        }

        // 4. 最低表示時間を確保（演出のため）
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < m_MinLoadingTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(m_MinLoadingTime - elapsedTime));
        }

        // 5. シーン切り替えを許可
        if (m_ProgressBar != null) m_ProgressBar.value = 1.0f;
        asyncOp.allowSceneActivation = true;

        // シーンが完全に切り替わるまで待機
        await UniTask.WaitUntil(() => asyncOp.isDone);

        // 6. フェードアウトしてロード画面を隠す
        await FadeAsync(0.0f);
        m_LoadingCanvasGroup.gameObject.SetActive(false);
    }

    /// <summary>
    /// CanvasGroupのAlphaを操作するシンプルなフェード
    /// </summary>
    private async UniTask FadeAsync(float targetAlpha)
    {
        float startAlpha = m_LoadingCanvasGroup.alpha;
        float timer = 0;

        while (timer < m_FadeDuration)
        {
            timer += Time.deltaTime;
            m_LoadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / m_FadeDuration);
            await UniTask.Yield();
        }
        m_LoadingCanvasGroup.alpha = targetAlpha;
    }
}
