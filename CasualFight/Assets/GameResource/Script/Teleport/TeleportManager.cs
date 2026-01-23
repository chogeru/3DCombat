using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// テレポートを管理するオブジェクト
/// </summary>
public class TeleportManager : MonoBehaviour
{
    //インスタンス化(書き換えは自身のみ)
    public static TeleportManager TPInstance { get; private set; }

    [Header("全てのテレポート"), SerializeField]
    List<TeleportPoint> m_AllPoints = new List<TeleportPoint>();

    [Header("プレイヤーの参照"), SerializeField]
    GameObject m_Player;

    private void Awake()
    {
        TPInstance = this;
    }

    /// <summary>
    /// テレポート処理
    /// </summary>
    /// <param name="index"></param>
    public void RequestTeleport(int index)
    {
        TeleportPoint target=m_AllPoints[index];

        //指定したオブジェクトが未開放ならスキップ
        if (!target.IsUnlocked)
        {
            Debug.Log("未開放");
            return;
        }

        m_Player.transform.position=target.TeleportPosition;
        Debug.Log("転送します");
    }
}
