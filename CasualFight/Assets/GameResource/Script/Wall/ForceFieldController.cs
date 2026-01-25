using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 壁（シェーダー）を表示させるかどうかの判定処理
/// </summary>
public class ForceFieldController : MonoBehaviour
{
    [Header("追跡する対象（プレイヤー）"), SerializeField]
    Transform m_PlayerTransform;

    [Header("表示される半径"), SerializeField]
    float m_VisibleRadius = 5.0f;

    Material m_Material;

    //シェーダー内の「Reference」名と一致させる
    static readonly int m_PlayerPosID = Shader.PropertyToID("_PlayerPos");
    static readonly int m_RadiusID = Shader.PropertyToID("_VisibleRadius");

    void Start()
    {
        //オブジェクトのRendererからマテリアルを取得
        m_Material = GetComponent<Renderer>().material;

        //プレイヤーが未設定なら自動で"Player"タグから探す
        if (m_PlayerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) m_PlayerTransform = player.transform;
        }

        //初期の半径をシェーダーに送る
        m_Material.SetFloat(m_RadiusID, m_VisibleRadius);
    }

    void Update()
    {
        if (m_PlayerTransform != null && m_Material != null)
        {
            // プレイヤーの現在地をリアルタイムでシェーダーへ送る
            m_Material.SetVector(m_PlayerPosID, m_PlayerTransform.position);
        }
    }
}
