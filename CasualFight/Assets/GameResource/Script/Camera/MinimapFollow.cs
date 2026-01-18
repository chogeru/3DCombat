using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミニマップのカメラの追尾処理
/// </summary>
public class MinimapFollow : MonoBehaviour
{
    [Header("追尾対象"), SerializeField]
    Transform m_Player;

    [Header("カメラの高さ"),SerializeField]
    float m_Height = 20.0f;


    private void LateUpdate()
    {
        //プレイヤーの座標取得
        Vector3 pos = m_Player.position;
        
        //高さの変更
        pos.y = pos.y+m_Height;
        
        //カメラに座標適応
        transform.position = pos;
    }
}
