using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// UI Textを右から左（または左から右）に一文字ずつフェードインしながら表示するエフェクト
/// </summary>
public class TextFillEffect : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("全体の表示にかかる時間（秒）")]
    [SerializeField] float m_Duration = 1.5f;

    [Tooltip("各文字のフェードイン時間（秒）")]
    [SerializeField] float m_CharFadeDuration = 0.3f;

    [Tooltip("右から左に表示するか（falseなら左から右）")]
    [SerializeField] bool m_RightToLeft = true;

    [Tooltip("開始時に自動で再生するか")]
    [SerializeField] bool m_PlayOnEnable = true;

    // 内部変数
    Text m_Text;
    string m_OriginalText;
    Color m_OriginalColor;

    void Awake()
    {
        m_Text = GetComponent<Text>();
        if (m_Text != null)
        {
            m_OriginalText = m_Text.text;
            m_OriginalColor = m_Text.color;
        }
    }

    void OnEnable()
    {
        if (m_PlayOnEnable && m_Text != null)
        {
            PlayFillEffect().Forget();
        }
    }

    /// <summary>
    /// フィルエフェクトを再生
    /// </summary>
    public async UniTaskVoid PlayFillEffect()
    {
        if (m_Text == null || string.IsNullOrEmpty(m_OriginalText)) return;

        int charCount = m_OriginalText.Length;
        if (charCount == 0) return;

        // 最初は透明で非表示
        m_Text.text = "";
        Color color = m_OriginalColor;
        color.a = 0f;
        m_Text.color = color;

        // 各文字の表示開始タイミングを計算
        float delayPerChar = (m_Duration - m_CharFadeDuration) / Mathf.Max(1, charCount - 1);
        if (charCount == 1) delayPerChar = 0f;

        // 各文字のアルファ値を管理
        float[] charAlphas = new float[charCount];
        float[] charStartTimes = new float[charCount];

        // 表示順序を決定（右から左 or 左から右）
        for (int i = 0; i < charCount; i++)
        {
            int index = m_RightToLeft ? (charCount - 1 - i) : i;
            charStartTimes[index] = i * delayPerChar;
            charAlphas[index] = 0f;
        }

        float elapsed = 0f;
        float totalDuration = m_Duration + m_CharFadeDuration;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // 各文字のアルファ値を計算
            for (int i = 0; i < charCount; i++)
            {
                float charElapsed = elapsed - charStartTimes[i];
                if (charElapsed >= 0f)
                {
                    charAlphas[i] = Mathf.Clamp01(charElapsed / m_CharFadeDuration);
                }
            }

            // 全体の平均アルファでテキストを表示
            // ※UI Text (Legacy)は文字ごとのアルファ設定ができないため、
            //   全体のアルファとテキストの表示文字数で疑似的に表現
            float avgAlpha = 0f;
            int visibleCount = 0;
            for (int i = 0; i < charCount; i++)
            {
                if (charAlphas[i] > 0f)
                {
                    avgAlpha += charAlphas[i];
                    visibleCount++;
                }
            }

            if (visibleCount > 0)
            {
                avgAlpha /= visibleCount;
                
                // 表示する文字列を構築
                string visibleText = "";
                for (int i = 0; i < charCount; i++)
                {
                    if (charAlphas[i] > 0.01f)
                    {
                        visibleText += m_OriginalText[i];
                    }
                    else
                    {
                        // まだ表示されない文字はスペースで置き換え（位置を保持）
                        visibleText += " ";
                    }
                }
                m_Text.text = visibleText;

                // 全体のアルファを設定
                color.a = Mathf.Min(avgAlpha * 1.5f, m_OriginalColor.a);
                m_Text.color = color;
            }

            await UniTask.Yield();
        }

        // 最終状態を確定
        m_Text.text = m_OriginalText;
        m_Text.color = m_OriginalColor;
    }

    /// <summary>
    /// エフェクトをリセット（非表示状態に戻す）
    /// </summary>
    public void ResetEffect()
    {
        if (m_Text != null)
        {
            m_Text.text = "";
            Color color = m_OriginalColor;
            color.a = 0f;
            m_Text.color = color;
        }
    }

    /// <summary>
    /// 外部から再生を呼び出す
    /// </summary>
    public void Play()
    {
        PlayFillEffect().Forget();
    }

    /// <summary>
    /// 即座に表示完了状態にする
    /// </summary>
    public void ShowImmediately()
    {
        if (m_Text != null)
        {
            m_Text.text = m_OriginalText;
            m_Text.color = m_OriginalColor;
        }
    }
}
