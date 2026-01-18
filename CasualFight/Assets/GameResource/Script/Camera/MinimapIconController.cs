using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミニマップの回転処理
/// </summary>
public class MinimapIconController : MonoBehaviour
{
    [Header("プレイヤーの座標"), SerializeField] 
    Transform m_PlayerTransform;
    [Header("回転ONOFFフラグ"), SerializeField] 
    bool m_IsPlayerIcon = true;

    void LateUpdate()
    {
        if (m_PlayerTransform == null) return;

        // アイコンを真上（ミニマップカメラの方）に向ける
        // Xを90度に固定し、Yにプレイヤーの向きを代入する
        if (m_IsPlayerIcon)
        {
            // プレイヤーの向きに合わせてカメラも回転（前方固定モード）
            transform.rotation = Quaternion.Euler(90f, m_PlayerTransform.eulerAngles.y, 0f);
        }
        else
        {
            // 常に北を上にする（北固定モード）
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
