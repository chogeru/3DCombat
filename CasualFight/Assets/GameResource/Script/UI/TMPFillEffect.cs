using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

/// <summary>
/// TextMeshProを右から左（または左から右）に一文字ずつフェードインしながら表示するエフェクト
/// 文字ごとに個別のアルファ値を設定可能
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TMPFillEffect : MonoBehaviour
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
    TMP_Text m_Text;
    bool m_IsPlaying = false;

    void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
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
        if (m_Text == null || m_IsPlaying) return;

        m_IsPlaying = true;

        // テキスト情報を更新
        m_Text.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_Text.textInfo;
        int charCount = textInfo.characterCount;

        if (charCount == 0)
        {
            m_IsPlaying = false;
            return;
        }

        // 最初は全文字を透明にする
        SetAllCharsAlpha(0f);

        // 各文字の表示開始タイミングを計算
        float delayPerChar = charCount > 1 
            ? (m_Duration - m_CharFadeDuration) / (charCount - 1) 
            : 0f;

        float[] charStartTimes = new float[charCount];
        
        // 表示順序を決定（右から左 or 左から右）
        for (int i = 0; i < charCount; i++)
        {
            int index = m_RightToLeft ? (charCount - 1 - i) : i;
            charStartTimes[index] = i * delayPerChar;
        }

        float elapsed = 0f;
        float totalDuration = m_Duration + m_CharFadeDuration;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // メッシュ情報を取得
            m_Text.ForceMeshUpdate();
            textInfo = m_Text.textInfo;

            // 各文字のアルファ値を計算・適用
            for (int i = 0; i < charCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                // 表示されない文字（スペース等）はスキップ
                if (!charInfo.isVisible) continue;

                float charElapsed = elapsed - charStartTimes[i];
                float alpha = charElapsed >= 0f 
                    ? Mathf.Clamp01(charElapsed / m_CharFadeDuration) 
                    : 0f;

                // 文字の頂点カラーを更新
                SetCharAlpha(i, alpha, textInfo);
            }

            // メッシュを更新
            m_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            await UniTask.Yield();
        }

        // 最終状態を確定（全文字を完全に表示）
        SetAllCharsAlpha(1f);
        
        m_IsPlaying = false;
    }

    /// <summary>
    /// 特定の文字のアルファ値を設定
    /// </summary>
    void SetCharAlpha(int charIndex, float alpha, TMP_TextInfo textInfo)
    {
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
        byte alphaByte = (byte)(alpha * 255);

        // 文字の4つの頂点のアルファを設定
        vertexColors[vertexIndex + 0].a = alphaByte;
        vertexColors[vertexIndex + 1].a = alphaByte;
        vertexColors[vertexIndex + 2].a = alphaByte;
        vertexColors[vertexIndex + 3].a = alphaByte;
    }

    /// <summary>
    /// 全文字のアルファ値を設定
    /// </summary>
    void SetAllCharsAlpha(float alpha)
    {
        if (m_Text == null) return;

        m_Text.ForceMeshUpdate();
        TMP_TextInfo textInfo = m_Text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            SetCharAlpha(i, alpha, textInfo);
        }

        m_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    /// <summary>
    /// エフェクトをリセット（非表示状態に戻す）
    /// </summary>
    public void ResetEffect()
    {
        SetAllCharsAlpha(0f);
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
        SetAllCharsAlpha(1f);
    }
}
