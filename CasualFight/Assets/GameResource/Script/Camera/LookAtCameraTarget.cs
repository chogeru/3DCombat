using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラの向きでキャラクターの向きを行うための処理
/// </summary>
public class LookAtCameraTarget : MonoBehaviour
{
    [Header("Animation Riggingのターゲット"),SerializeField]
    Transform m_LookTarget;

    [Header("メインカメラ"), SerializeField]
    Transform m_CameraTf;

    [Header("どれくらい先を見るか"), SerializeField]
    float m_Distance = 15f;

    [Header("高さの補正"), SerializeField]
    float m_HightOffset = 0f;

    private void Update()
    {
        if (m_LookTarget == null || m_CameraTf == null) 
            return;

        //カメラ座標の取得
        Vector3 cameraPos = m_CameraTf.position;

        //カメラが見ている視界のど真ん中に配置
        Vector3 targetPos = cameraPos + m_CameraTf.forward * m_Distance;

        //高さ調整
        targetPos.y+= m_HightOffset;

        //ターゲットを移動
        m_LookTarget.position = targetPos;
    }
}
