using UnityEngine;
using UnityEngine.UI; // 標準のTextを使用
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

/// <summary>
/// ゲーム内の字幕表示を管理するクラス。
/// シングルトンとして機能し、どこからでもアクセス可能です。
/// </summary>
public class GameSubtitleManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameSubtitleManager Instance { get; private set; }

    [Header("UI References")]
    // フェードイン・アウト用のCanvasGroup
    [SerializeField] private CanvasGroup m_CanvasGroup;
    // 話者名を表示するテキスト
    [SerializeField] private Text m_NameText;
    // メッセージ本文を表示するテキスト
    [SerializeField] private Text m_MessageText;

    [Header("Settings")]
    // 1秒間に表示する文字数（Characters Per Second）
    [SerializeField] private int m_Cps = 20;

    // 非同期処理のキャンセル用トークンソース
    private CancellationTokenSource m_Cts;

    private void Awake()
    {
        // シングルトンの初期化
        if (Instance == null)
        {
            Instance = this;
            // 初期状態では透明にしておく
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0;
            
            // TextのRichText設定を強制的にONにする（色タグなどを使用するため）
            if (m_MessageText != null) m_MessageText.supportRichText = true;
        }
        else
        {
            // 既にインスタンスが存在する場合は破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 字幕を表示する非同期メソッド。
    /// リッチテキストタグ（<color=red>など）に対応しています。
    /// </summary>
    /// <param name="name">話者の名前</param>
    /// <param name="message">表示するメッセージ本文</param>
    /// <param name="duration">表示完了後の待機時間（秒）</param>
    public async UniTask ShowSubtitleAsync(string name, string message, float duration = 3.0f)
    {
        // 前回の表示処理が実行中ならキャンセル
        m_Cts?.Cancel();
        m_Cts = new CancellationTokenSource();
        var token = m_Cts.Token;

        try
        {
            // UIの初期設定
            if (m_NameText != null) m_NameText.text = name;
            if (m_MessageText != null) m_MessageText.text = "";
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 1;

            // タイプライター演出の実行（文字送り）
            await TypewriterEffect(message, token);

            // 指定時間待機（読み終わるまでの時間）
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            // 終了時にフェードアウト
            await FadeOutAsync(0.5f, token);
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合（次の字幕が表示された時など）は即座に非表示にするなどの処理
            // ここではシンプルにアルファを0にする
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0;
        }
    }

    /// <summary>
    /// タイプライター風に文字を1文字ずつ表示する演出。
    /// リッチテキストタグ（<>で囲まれた部分）は一括で表示し、終了タグを補完してリッチテキストとして認識させます。
    /// </summary>
    private async UniTask TypewriterEffect(string fullText, CancellationToken token)
    {
        if (m_MessageText == null) return;

        m_MessageText.text = "";
        int currentIndex = 0;
        int intervalMs = Mathf.RoundToInt(1000f / m_Cps);

        while (currentIndex < fullText.Length)
        {
            // キャンセルチェック
            if (token.IsCancellationRequested) return;

            // 1. タグのチェックループ
            // もし「<」があったら、タグの終わり「>」まで一気にスキップしてインデックスを進める
            if (fullText[currentIndex] == '<')
            {
                int tagEndIndex = fullText.IndexOf('>', currentIndex);
                if (tagEndIndex != -1)
                {
                    // タグの終わりまで進める
                    currentIndex = tagEndIndex + 1;
                    // タグだけでは待機せず、次の文字へ即座に進む
                    continue;
                }
            }

            // 2. タグ以外の文字なら1文字進める
            currentIndex++;

            // 3. 現在の文字数分を切り出す
            string currentDisplay = fullText.Substring(0, currentIndex);

            // 4. 【重要】未完のタグを補完する
            // 常に最後に "</color>" などを強制的に付与することで、Unityに「これはリッチテキストだ」と認識させる
            m_MessageText.text = EnsureTagsClosed(currentDisplay);

            await UniTask.Delay(intervalMs, cancellationToken: token);
        }
    }

    /// <summary>
    /// Legacy Text用に、開始タグが開いたままの文字列に対して終了タグを補完するメソッド
    /// </summary>
    private string EnsureTagsClosed(string currentText)
    {
        // もし現在の文字に "<color=" が含まれていて、かつ直近で閉じられていない場合
        // （簡易的なチェックとしてLastIndexOfを使用）
        int lastOpenTag = currentText.LastIndexOf("<color=");
        int lastCloseTag = currentText.LastIndexOf("</color>");

        if (lastOpenTag > lastCloseTag)
        {
            return currentText + "</color>";
        }
        
        return currentText;
    }

    // 一時停止前のアルファ値を保存する変数
    private float m_PrePauseAlpha = 0f;

    /// <summary>
    /// 字幕を一時的に非表示にする（ポーズ画面用）
    /// 時間停止中に画面から消すために使用します。
    /// </summary>
    public void Pause()
    {
        if (m_CanvasGroup != null)
        {
            // 現在のアルファ値を保存
            m_PrePauseAlpha = m_CanvasGroup.alpha;
            // 完全に透明にする
            m_CanvasGroup.alpha = 0;
        }
    }

    /// <summary>
    /// 字幕の表示を再開する
    /// ポーズ画面から戻った時に呼び出します。
    /// </summary>
    public void Resume()
    {
        if (m_CanvasGroup != null)
        {
            // 保存しておいたアルファ値に戻す
            m_CanvasGroup.alpha = m_PrePauseAlpha;
        }
    }

    /// <summary>
    /// CanvasGroupのアルファ値を徐々に下げてフェードアウトさせる処理。
    /// </summary>
    private async UniTask FadeOutAsync(float duration, CancellationToken token)
    {
        if (m_CanvasGroup == null) return;

        float startAlpha = m_CanvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 線形補間でアルファ値を計算
            m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsed / duration);
            // 1フレーム待機
            await UniTask.Yield(token);
        }
        // 最終的に完全に透明にする
        m_CanvasGroup.alpha = 0;
    }
    
    private void OnDestroy()
    {
        // オブジェクト破棄時に非同期処理をキャンセル
        m_Cts?.Cancel();
        m_Cts?.Dispose();
    }
}
