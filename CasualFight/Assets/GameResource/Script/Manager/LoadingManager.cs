using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//UniTaskをインポート
using Cysharp.Threading.Tasks;
using System;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("UI References"), Header("フェード用"), SerializeField]
    private CanvasGroup m_LoadingCanvasGroup;
    [Header("進捗バー"), SerializeField]
    private Slider m_ProgressBar;

    [Header("Settings"), Header("フェード時間"), SerializeField]
    private float m_FadeDuration = 0.5f;
    [Header("最低表示時間（一瞬で終わるのを防ぐ）"), SerializeField]
    private float m_MinLoadingTime = 1.0f;

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

    ///<summary>
    ///シーンを非同期でロードする
    ///</summary>
    public async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        //ロード画面を表示し、フェードイン
        m_LoadingCanvasGroup.gameObject.SetActive(true);
        await FadeAsync(1.0f);

        //非同期ロード開始
        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        //読み込み完了しても勝手に切り替えない
        asyncOp.allowSceneActivation = false;

        float startTime = Time.time;

        //ロード進捗を監視
        while (asyncOp.progress < 0.9f)
        {
            if (m_ProgressBar != null)
            {
                //progress(0~0.9)を0~1に補正してバーに反映
                m_ProgressBar.value = asyncOp.progress / 0.9f;
            }
            //1フレーム待機
            await UniTask.Yield();
        }

        //最低表示時間を確保（演出のため）
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < m_MinLoadingTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(m_MinLoadingTime - elapsedTime));
        }

        //シーン切り替えを許可
        if (m_ProgressBar != null) m_ProgressBar.value = 1.0f;
        asyncOp.allowSceneActivation = true;

        //シーンが完全に切り替わるまで待機
        await UniTask.WaitUntil(() => asyncOp.isDone);

        //フェードアウトしてロード画面を隠す
        await FadeAsync(0.0f);
        m_LoadingCanvasGroup.gameObject.SetActive(false);
    }

    ///<summary>
    ///CanvasGroupのAlphaを操作するシンプルなフェード
    ///</summary>
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
