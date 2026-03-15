using UnityEngine;

/// <summary>
/// 特定のグループ内で初めて要素が解放された時に、ゲームを一時停止してUIを表示する汎用クラス。
/// 既存の GameStateManager と連携し、表示中はミニマップなどの各種UIを自動で非表示にします。
/// </summary>
public class UnlockManager : MonoBehaviour
{
    [Header("初回解放時に表示するUIパネル等のオブジェクト")]
    [SerializeField] private GameObject m_FirstUnlockUI;

    [Header("この解放グループを識別するためのセーブキー")]
    [Tooltip("全対象オブジェクトで共通のメモの名前（キー）にするのが最大のポイントです")]
    [SerializeField] private string m_UnlockGroupKey = "HasUnlocked_DefaultGroup";

    [Header("デバッグ用: ゲーム起動時にこのキーのセーブデータをリセットするか")]
    [SerializeField] private bool m_ResetOnStart = false;

    // 停止中フラグ
    private bool m_IsDisplaying = false;

    // 停止前の状態を記憶
    private float m_PreviousTimeScale = 1f;
    private GameStateManager.GameState m_PreviousGameState;

    private void Start()
    {
        if (m_ResetOnStart)
        {
            ResetSaveData();
        }
    }

    private void Update()
    {
        // UI表示中（停止中）のみ入力を監視
        if (m_IsDisplaying)
        {
            // 右クリック または ESCキー が押されたら閉じる
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseUnlockUI();
            }
        }
    }

    /// <summary>
    /// オブジェクトが解放された時に呼ばれる（UIボタンや他スクリプトから呼び出し）
    /// </summary>
    public void OnObjectUnlocked()
    {
        int hasUnlocked = PlayerPrefs.GetInt(m_UnlockGroupKey, 0);

        if (hasUnlocked == 0)
        {
            // 初回解放時の処理
            Debug.Log($"[{m_UnlockGroupKey}] 初回解放演出を開始。ゲームを停止し、他UIを隠します。");
            
            PlayerPrefs.SetInt(m_UnlockGroupKey, 1);
            PlayerPrefs.Save();

            // GameStateManager を利用して他のUI（ミニマップ等）を非表示にする
            if (GameStateManager.Instance != null)
            {
                m_PreviousGameState = GameStateManager.Instance.CurrentState;
                // Eventステートにすることで、m_ObjectsToHideOnEvent に登録されたUIが消える
                GameStateManager.Instance.ChangeState(GameStateManager.GameState.Event);
            }

            // TimeScaleを0にして、各種Update（移動、会話、アニメーションなど）を停止する
            m_PreviousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // このマネージャー専用のUIを表示
            if (m_FirstUnlockUI != null)
            {
                m_FirstUnlockUI.SetActive(true);
            }

            m_IsDisplaying = true;
        }
    }

    /// <summary>
    /// 解放UIを閉じてゲームを再開する
    /// </summary>
    private void CloseUnlockUI()
    {
        // 専用UIを非表示
        if (m_FirstUnlockUI != null)
        {
            m_FirstUnlockUI.SetActive(false);
        }

        // TimeScaleを元に戻し、ゲームを再開
        Time.timeScale = m_PreviousTimeScale;

        // GameStateManager を元のステートに戻し、隠していたUIを復活させる
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ChangeState(m_PreviousGameState);
        }

        m_IsDisplaying = false;
        Debug.Log($"[{m_UnlockGroupKey}] 解放UIを閉じ、ゲームを再開しました。");
    }

    /// <summary>
    /// このマネージャーで管理しているセーブデータをリセットする
    /// </summary>
    public void ResetSaveData()
    {
        if (PlayerPrefs.HasKey(m_UnlockGroupKey))
        {
            PlayerPrefs.DeleteKey(m_UnlockGroupKey);
            PlayerPrefs.Save();
            Debug.Log($"[{m_UnlockGroupKey}] セーブデータをリセットしました。");
        }
    }
}
