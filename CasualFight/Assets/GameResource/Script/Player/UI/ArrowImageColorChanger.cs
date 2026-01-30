using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵に向く矢印のハイライト変更処理
/// </summary>
public class ArrowImageColorChanger : MonoBehaviour
{
    [Header("設定項目")]
    [Header("色を変える対象のImage"),SerializeField]
    Image m_TargetImage;      
    [Header("開始色"),SerializeField]
    private Color m_ColorA = Color.white;  
    [Header("終了色"), SerializeField]
    private Color m_ColorB = new Color(1f, 1f, 1f, 0.5f); 
    [Header("色が変わる速度"), SerializeField]
    private float m_ChangeSpeed = 2f;  

    private void Start()
    {
        //もしImageが未設定なら自分自身のImageを取得
        if (m_TargetImage == null)
        {
            m_TargetImage = GetComponent<Image>();
        }
    }

    private void Update()
    {
        // イベント中は処理しない（負荷軽減＆エラー防止）
        if (GameStateManager.Instance != null)
        {
            var state = GameStateManager.Instance.CurrentState;
            if (state == GameStateManager.GameState.Event || state == GameStateManager.GameState.Dialogue) return;
        }

        if (m_TargetImage == null) return;

        //サイン波を使って0.0〜1.0の間を滑らかに行き来させる
        float lerpFactor = (Mathf.Sin(Time.time * m_ChangeSpeed) + 1f) / 2f;

        //指定した2色の間で色を補完する
        m_TargetImage.color = Color.Lerp(m_ColorA, m_ColorB, lerpFactor);
    }
}
