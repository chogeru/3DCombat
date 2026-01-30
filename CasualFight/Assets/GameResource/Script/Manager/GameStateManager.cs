using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の状態（ステート）を管理するマネージャークラス。
/// シングルトンパターンで実装され、どこからでもアクセス可能。
/// 状態に応じてUIの表示/非表示などを切り替える機能を持つ。
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameStateManager Instance { get; private set; }

    /// <summary>
    /// ゲームのステート定義
    /// </summary>
    public enum GameState
    {
        Exploration, // 探索（通常状態）
        Combat,      // 戦闘中
        Event,       // イベント演出中（必殺技など）
        Dialogue     // 会話イベント中
    }

    [Header("現在のステート")]
    [SerializeField] GameState m_CurrentState = GameState.Exploration;
    
    /// <summary>
    /// 現在のゲームステート（読み取り専用）
    /// </summary>
    public GameState CurrentState => m_CurrentState;

    [Header("UI設定: イベント中(Event/Dialogue)に非表示にするオブジェクト")]
    [SerializeField] List<GameObject> m_ObjectsToHideOnEvent = new List<GameObject>();

    /// <summary>
    /// 初期化処理。シングルトンの設定を行う。
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーン遷移しても破壊されないようにする（必要であればコメントアウトを解除）
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定したステートに変更し、UIの状態を更新する。
    /// </summary>
    /// <param name="newState">変更先の新しいステート</param>
    public void ChangeState(GameState newState)
    {
        Debug.Log($"GameState変更: {m_CurrentState} -> {newState}");
        m_CurrentState = newState;
        ApplyUIState();
    }

    /// <summary>
    /// 現在のステートに基づいてUIの表示/非表示を適用する。
    /// </summary>
    private void ApplyUIState()
    {
        // イベント中または会話中は、指定されたUIを非表示にする
        bool isEventMode = (m_CurrentState == GameState.Event || m_CurrentState == GameState.Dialogue);

        foreach (var obj in m_ObjectsToHideOnEvent)
        {
            if (obj != null)
            {
                // イベントモードなら非表示(false)、それ以外なら表示(true)
                obj.SetActive(!isEventMode);
            }
        }
    }
}
