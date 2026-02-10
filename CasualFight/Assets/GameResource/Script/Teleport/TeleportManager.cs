using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// テレポート管理オブジェクト
/// </summary>
public class TeleportManager : MonoBehaviour
{
    // インスタンス(他参照用)
    public static TeleportManager TPInstance { get; private set; }

    [Header("全てのテレポート"), SerializeField]
    List<TeleportPoint> m_AllPoints = new List<TeleportPoint>();

    [Header("プレイヤーの参照"), SerializeField]
    GameObject m_Player;

    [Header("UI設定")]
    [SerializeField] GameObject m_TeleportUIRoot; // UI全体の親
    [SerializeField] Image m_TeleportPointImage; // プレビュー画像表示用Image
    [SerializeField] Button m_TeleportOKButton;  // 決定ボタン
    [SerializeField] List<Text> m_PointNameTexts; // 各地点のボタン等にあるテキストのリスト
    [SerializeField] Text m_NotYetOpenText; // 未開放時に表示するテキスト

    int m_CurrentSelectedIndex = -1; // 現在選択中の地点 (-1は未選択)

    private void Awake()
    {
        TPInstance = this;
    }

    private void Start()
    {
        // 初期状態：UI非表示
        if (m_TeleportUIRoot != null)
        {
            m_TeleportUIRoot.SetActive(false);
        }

        // 初期状態：画像非表示、選択なし
        if (m_TeleportPointImage != null)
        {
            // 透明度0で非表示
            Color c = m_TeleportPointImage.color;
            c.a = 0f;
            m_TeleportPointImage.color = c;
        }

        // 未開放テキストも非表示
        if (m_NotYetOpenText != null)
        {
            m_NotYetOpenText.gameObject.SetActive(false);
        }

        // 決定ボタンイベント登録
        if (m_TeleportOKButton != null)
        {
            m_TeleportOKButton.onClick.AddListener(OnClickTeleportOKButton);
        }
    }


    // UIが開いているかどうか（外部参照用）
    public bool IsUIOpen => m_TeleportUIRoot != null && m_TeleportUIRoot.activeSelf;

    private void Update()
    {
        // デバッグ用: TabキーでUI表示切り替え
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Settings画面が開いていたら反応しない
            if (SettingsManager.Instance != null && SettingsManager.Instance.IsMenuOpen)
            {
                return;
            }

            if (m_TeleportUIRoot != null)
            {
                bool isActive = m_TeleportUIRoot.activeSelf;
                bool nextActiveState = !isActive;
                m_TeleportUIRoot.SetActive(nextActiveState);

                if (nextActiveState)
                {
                    // UIを開いたので時間を止める
                    Time.timeScale = 0f;
                    
                    // 字幕を一時非表示
                    if (GameSubtitleManager.Instance != null) GameSubtitleManager.Instance.Pause();
                    
                    // カーソルを表示・ロック解除
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    // テキスト更新（未開放なら???）
                    RefreshPointTexts();
                }
                else
                {
                    // UIを閉じたので時間を再開する
                    Time.timeScale = 1f;

                    // 字幕を復帰
                    if (GameSubtitleManager.Instance != null) GameSubtitleManager.Instance.Resume();

                    // カーソルを非表示・ロック
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }

    /// <summary>
    /// 外部からUIを強制的に閉じる
    /// </summary>
    public void CloseUI()
    {
        if (m_TeleportUIRoot != null && m_TeleportUIRoot.activeSelf)
        {
            m_TeleportUIRoot.SetActive(false);

            // UIを閉じたので時間を再開する
            Time.timeScale = 1f;

            // 字幕を復帰
            if (GameSubtitleManager.Instance != null) GameSubtitleManager.Instance.Resume();

            // カーソルを非表示・ロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// UI上のテキスト表示更新（未開放なら???）
    /// </summary>
    void RefreshPointTexts()
    {
        if (m_PointNameTexts == null) return;

        int count = Mathf.Min(m_AllPoints.Count, m_PointNameTexts.Count);

        for (int i = 0; i < count; i++)
        {
            if (m_AllPoints[i] == null || m_PointNameTexts[i] == null) continue;

            if (m_AllPoints[i].IsUnlocked)
            {
                m_PointNameTexts[i].text = m_AllPoints[i].PointName;
            }
            else
            {
                m_PointNameTexts[i].text = "???";
            }
        }
    }

    /// <summary>
    /// UIのボタンで地点を選択したときの処理
    /// </summary>
    /// <param name="index"></param>
    public void OnSelectTeleportPoint(int index)
    {
        if (index < 0 || index >= m_AllPoints.Count) return;

        TeleportPoint target = m_AllPoints[index];

        // 選択状態更新
        m_CurrentSelectedIndex = index;

        if (target != null && !target.IsUnlocked)
        {
            // === 未開放の場合 ===
            Debug.Log("未開放の地点を選択しました");

            // 1. 画像を非表示
            if (m_TeleportPointImage != null)
            {
                Color c = m_TeleportPointImage.color;
                c.a = 0f;
                m_TeleportPointImage.color = c;
            }

            // 2. 「未開放」テキストを表示
            if (m_NotYetOpenText != null)
            {
                m_NotYetOpenText.gameObject.SetActive(true);
            }
        }
        else
        {
            // === 開放済みの場合 ===
            
            // 1. 画像を表示
            if (m_TeleportPointImage != null && target.AreaSprite != null)
            {
                m_TeleportPointImage.sprite = target.AreaSprite;
                
                // 透明度を1に戻して表示
                Color c = m_TeleportPointImage.color;
                c.a = 1f;
                m_TeleportPointImage.color = c;
            }

            // 2. 「未開放」テキストを非表示
            if (m_NotYetOpenText != null)
            {
                m_NotYetOpenText.gameObject.SetActive(false);
            }
            
            Debug.Log($"地点選択: {target.PointName}");
        }
    }

    /// <summary>
    /// 右下の「テレポート」ボタンを押したときの処理
    /// </summary>
    public void OnClickTeleportOKButton()
    {
        if (m_CurrentSelectedIndex < 0 || m_CurrentSelectedIndex >= m_AllPoints.Count)
        {
            Debug.Log("地点が選択されていません");
            return;
        }

        TeleportPoint target = m_AllPoints[m_CurrentSelectedIndex];
        
        // 未開放なら移動しない
        if (target != null && !target.IsUnlocked)
        {
            Debug.Log("未開放のため移動できません");
            return;
        }

        if (target != null)
        {
            // プレイヤーのCharacterControllerを取得
            CharacterController cc = m_Player.GetComponent<CharacterController>();

            // 移動のために一時的に無効化
            if (cc != null) cc.enabled = false;

            // Y軸だけはプレイヤーの現在地を使用
            Vector3 finalPos = target.TeleportPosition;
            finalPos.y = m_Player.transform.position.y;
            m_Player.transform.position = finalPos;

            // 有効化に戻す
            if (cc != null) cc.enabled = true;

            Debug.Log($"移動しました: {target.PointName}");
            
            // UIを閉じる
            if (m_TeleportUIRoot != null)
            {
                m_TeleportUIRoot.SetActive(false);
            }

            // 時間を再開する
            Time.timeScale = 1f;

            // 字幕を復帰
            if (GameSubtitleManager.Instance != null) GameSubtitleManager.Instance.Resume();

            // カーソルを非表示・ロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// 指定座標から一番近い開放済みテレポート地点の座標を返す
    /// 見つからない場合は null を返す
    /// </summary>
    public Vector3? GetNearestUnlockedPosition(Vector3 currentPos)
    {
        //最初は空っぽにしておいて、見つかったら上書きする。最後まで空っぽなら、見つからなかったということ
        TeleportPoint nearestPoint = null;

        // 「仮の最小距離」として、あえて最初に最大値を入れておく
        float minDistanceSqr = float.MaxValue;

        foreach (var point in m_AllPoints)
        {
            if (point == null) continue;

            // 開放済みかチェック
            if (point.IsUnlocked)
            {
                float distSqr = (point.TeleportPosition - currentPos).sqrMagnitude;
                // 距離が短ければ更新
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    nearestPoint = point;
                }
            }
        }

        if (nearestPoint != null)
        {
            return nearestPoint.TeleportPosition;
        }

        return null; // 開放済みポイントなし
    }
}
