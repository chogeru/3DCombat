using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵方向UI表示機能
/// </summary>
public class EnemyIndicator : MonoBehaviour
{
    [Header("矢印Prefab"), SerializeField]
    GameObject m_ArrowPrefab;

    [Header("中心基準点となる親オブジェクト"), SerializeField]
    RectTransform m_CenterScreen;

    [Header("プレイヤーのTransform"), SerializeField]
    Transform m_Player;

    [Header("UIを配置する半径"), SerializeField]
    float m_Radius = 350;

    [Tooltip("敵とUIのペア辞書")]
    Dictionary<Transform, RectTransform> m_Indicators = new Dictionary<Transform, RectTransform>();

    private void Start()
    {
        // 設定値のバリデーション（デバッグ用）
        if (m_Radius <= 0)
        {
            Debug.LogError($"[EnemyIndicator] Radius is set to {m_Radius}! The arrow will stick to the center. Please set a positive value in the Inspector.");
        }

        if (m_CenterScreen != null)
        {
            if (m_CenterScreen.GetComponent<UnityEngine.UI.LayoutGroup>() != null)
            {
                Debug.LogError($"[EnemyIndicator] LayoutGroup detected on '{m_CenterScreen.name}'! This will override the arrow's position and force it to the center/layout position. Please remove the LayoutGroup.");
            }
        }
    }

    private void Update()
    {
        UpdateIndicators();
    }

    /// <summary>
    /// 敵がカメラ外にいる場合、方向矢印を計算
    /// </summary>
    void UpdateIndicators()
    {
        ManagePool();

        // カメラの前方向（0度）を基準として定義
        Vector3 camForward = Camera.main.transform.forward;
        // 水平方向のみ考慮
        camForward.y = 0f;

        foreach (var pair in m_Indicators)
        {
            // 敵の座標（角度計算に使用）
            Transform enemy = pair.Key;
            // 矢印UIの座標
            RectTransform arrow = pair.Value;

            // 3D空間の敵座標をスクリーン座標（ピクセル）に変換
            Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.position);

            // 敵が画面外にいるかどうか判定
            bool isOffScreen = screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;

            // 画面外なら表示
            if (isOffScreen)
            {
                arrow.gameObject.SetActive(true);

                // カメラから敵への方向を計算
                Vector3 direction = enemy.position - m_Player.position;
                // 水平方向のみ
                direction.y = 0;

                // 相対角度を計算
                float angle = Vector3.SignedAngle(camForward, direction, Vector3.up);

                // 度数をラジアンに変換
                float rad = angle * Mathf.Deg2Rad;

                // 中心からの円周上の座標を計算
                arrow.anchoredPosition = new Vector2(Mathf.Sin(rad) * m_Radius, Mathf.Cos(rad) * m_Radius);

                // 矢印を敵の方向に回転
                arrow.localRotation = Quaternion.Euler(0, 0, -angle);
            }
            else
            {
                // 画面内の敵は非表示
                arrow.gameObject.SetActive(false);
            }
        }
    }

    void ManagePool()
    {
        // リストにあるがUIがない敵を追加
        foreach (var enemy in BattleManager.m_BattleInstance.m_ActiveEnemies)
        {
            // 敵が未登録なら追加
            if (!m_Indicators.ContainsKey(enemy))
            {
                // 生成
                GameObject newArrow = Instantiate(m_ArrowPrefab, m_CenterScreen);

                // 敵を登録
                m_Indicators.Add(enemy, newArrow.GetComponent<RectTransform>());
            }
        }

        // 削除対象を一時リストに保存
        List<Transform> toRemove = new List<Transform>();

        foreach (var key in m_Indicators.Keys)
        {
            // 辞書の敵がリストにいない（死亡、倒した等）
            if (!BattleManager.m_BattleInstance.m_ActiveEnemies.Contains(key))
            {
                // 削除リストに追加
                toRemove.Add(key);
            }
        }

        foreach (var key in toRemove)
        {
            // 削除リストにある敵のUIを破棄
            Destroy(m_Indicators[key].gameObject);

            // ペアリストから削除
            m_Indicators.Remove(key);
        }
    }
}
