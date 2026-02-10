using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class OperationGuideManager : MonoBehaviour
{
    public static OperationGuideManager Instance { get; private set; }

    [Header("UI References")]
    // フェードイン・アウト用のCanvasGroup（シーン上のオブジェクト）
    [SerializeField] private CanvasGroup m_CanvasGroup;
    // ガイドテキスト（シーン上のオブジェクト）
    [SerializeField] private Text m_MessageText;

    [Header("Animation Settings")]
    [SerializeField] private float m_FadeDuration = 0.5f;
    [SerializeField] private float m_DisplayTime = 3.0f;
    [SerializeField] private float m_IntervalTime = 0.5f; // 次のガイドまでの間隔

    [Header("Guide Messages")]
    [TextArea]
    [SerializeField] private List<string> m_GuideMessages = new List<string>()
    {
        "WSADで移動する",
        "Spaceを押してジャンプする",
        "左Shift / マウス右クリックで回避する\n長押しでダッシュ状態になる",
        "左クリックで攻撃する"
    };

    // 初回表示済みかどうかのキー
    private const string KEY_HAS_SHOWN_GUIDE = "HasShownGuide";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // 最初は非表示にしておく
        if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0f;
    }

    // Startメソッドは使用しない（TitleCameraControllerから呼び出される）

    /// <summary>
    /// 初回起動時のガイド再生を試みる（外部から呼び出し）
    /// </summary>
    public void PlayFirstLaunchGuide()
    {
        Debug.Log($"PlayFirstLaunchGuide called. HasKey: {PlayerPrefs.HasKey(KEY_HAS_SHOWN_GUIDE)}");

        // 初回起動チェック
        // "HasShownGuide" という鍵を持っているか確認する
        if (!PlayerPrefs.HasKey(KEY_HAS_SHOWN_GUIDE))
        {
            Debug.Log("First launch detected. Starting guide.");
            // 鍵を持っていない = 初めての起動

            // 1. ガイドを表示する処理を開始（非同期なのでForgetで投げっぱなしにする）
            ShowGuideSequenceAsync(m_GuideMessages).Forget();

            // 2. 「もう表示したよ」という証（鍵）を保存する
            // SetIntで "HasShownGuide" という名前の引出しに 1 を入れる
            PlayerPrefs.SetInt(KEY_HAS_SHOWN_GUIDE, 1);
            
            // 3. 確実にディスクに書き込む
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("Guide already shown. Skipping.");
        }
    }

    // 一時停止前のアルファ値を保存する変数
    private float m_PrePauseAlpha = 0f;

    /// <summary>
    /// ガイドを一時的に非表示にする（ポーズ画面用）
    /// </summary>
    public void Pause()
    {
        if (m_CanvasGroup != null)
        {
            m_PrePauseAlpha = m_CanvasGroup.alpha;
            m_CanvasGroup.alpha = 0;
        }
    }

    /// <summary>
    /// ガイドの表示を再開する
    /// </summary>
    public void Resume()
    {
        if (m_CanvasGroup != null)
        {
            m_CanvasGroup.alpha = m_PrePauseAlpha;
        }
    }

    /// <summary>
    /// テキストガイドを順番に表示する
    /// </summary>
    public async UniTask ShowGuideSequenceAsync(IEnumerable<string> messages)
    {
        foreach (var msg in messages)
        {
            await ShowSingleGuideAsync(msg);
            // 次の通知が出るまでの間隔
            await UniTask.Delay(TimeSpan.FromSeconds(m_IntervalTime));
        }
    }

    private async UniTask ShowSingleGuideAsync(string message)
    {
        if (m_CanvasGroup == null || m_MessageText == null) return;

        // テキストのセット
        m_MessageText.text = message;

        // 1. フェードイン
        await FadeAsync(m_CanvasGroup, 1f);

        // 2. 表示維持
        await UniTask.Delay(TimeSpan.FromSeconds(m_DisplayTime));

        // 3. フェードアウト
        await FadeAsync(m_CanvasGroup, 0f);
    }

    private async UniTask FadeAsync(CanvasGroup cg, float targetAlpha)
    {
        if (cg == null) return;
        float startAlpha = cg.alpha;
        float time = 0;

        while (time < m_FadeDuration)
        {
            if (cg == null) break;
            time += Time.deltaTime;
            // Linear interpolation (Lerp) で滑らかに数値を変化させる
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / m_FadeDuration);
            await UniTask.Yield();
        }
        if (cg != null) cg.alpha = targetAlpha;
    }

    // デバッグ用：強制リセット
    // Unityエディタ上でコンポーネントを右クリックして実行できる
    [ContextMenu("Reset Guide Flag")]
    public void ResetGuideFlag()
    {
        PlayerPrefs.DeleteKey(KEY_HAS_SHOWN_GUIDE);
        Debug.Log("ガイド表示フラグをリセットしました。次回の起動時に表示されます。");
    }

    // デバッグ用：強制再生
    [ContextMenu("Play Guide Sequence")]
    public void PlayGuideSequence()
    {
        ShowGuideSequenceAsync(m_GuideMessages).Forget();
    }
}
