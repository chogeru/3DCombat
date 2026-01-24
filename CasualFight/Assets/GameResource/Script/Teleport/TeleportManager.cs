using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        TPInstance = this;
    }

    /// <summary>
    /// テレポート要請
    /// </summary>
    /// <param name="index"></param>
    public void RequestTeleport(int index)
    {
        if (index < 0 || index >= m_AllPoints.Count) return;

        TeleportPoint target = m_AllPoints[index];

        // 指定したオブジェクト解放済みならスキップ(未開放ならリターン)
        if (target != null && !target.IsUnlocked)
        {
            Debug.Log("未開放");
            return;
        }

        if (target != null)
        {
            m_Player.transform.position = target.TeleportPosition;
            Debug.Log("移動しました");
        }
    }

    /// <summary>
    /// 指定座標から一番近い開放済みテレポート地点の座標を返す
    /// 見つからない場合は null を返す
    /// </summary>
    public Vector3? GetNearestUnlockedPosition(Vector3 currentPos)
    {
        TeleportPoint nearestPoint = null;
        float minDistanceSqr = float.MaxValue;

        foreach (var point in m_AllPoints)
        {
            if (point == null) continue;

            // 開放済みかチェック
            if (point.IsUnlocked)
            {
                float distSqr = (point.TeleportPosition - currentPos).sqrMagnitude;
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
