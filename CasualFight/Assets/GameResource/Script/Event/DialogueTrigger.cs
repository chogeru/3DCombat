using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// プレイヤーがコライダーに接触した際に字幕を表示するトリガーコンポーネント。
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Content")]
    [SerializeField] private string m_SpeakerName = "自分";
    
    // 複数のメッセージ（ページ）を登録できるように配列に変更
    [SerializeField, TextArea] 
    private string[] m_Messages = new string[] { "ここは<color=cyan>スタミナ</color>を使いそうだぞ。" };
    
    [SerializeField] private float m_DisplayDuration = 3.0f;

    [Header("Settings")]
    [SerializeField, Tooltip("一度きりしか喋らないか")]
    private bool m_TriggerOnce = true;
    
    private bool m_HasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが触れたか判定
        if (other.CompareTag("Player"))
        {
            ExecuteDialogueSequence().Forget();
        }
    }

    /// <summary>
    /// 設定されたメッセージを順番に表示します。
    /// </summary>
    private async UniTaskVoid ExecuteDialogueSequence()
    {
        // すでに実行済みなら何もしない（一度きり設定の場合）
        if (m_TriggerOnce && m_HasTriggered) return;

        m_HasTriggered = true;

        // 字幕マネージャーが存在し、メッセージが設定されている場合のみ実行
        if (GameSubtitleManager.Instance != null && m_Messages != null)
        {
            foreach (var message in m_Messages)
            {
                // 字幕を表示し、待機（表示時間 + フェードアウトなど）が完了するまで待つ
                // これにより、自然と「1ページ目が終わったら2ページ目」という動作になる
                await GameSubtitleManager.Instance.ShowSubtitleAsync(m_SpeakerName, message, m_DisplayDuration);
            }
        }
    }
}
