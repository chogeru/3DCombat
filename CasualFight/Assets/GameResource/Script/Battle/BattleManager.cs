using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵認識システム
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager m_BattleInstance;

    [Header("アクティブ敵リスト")]
    public List<Transform> m_ActiveEnemies = new List<Transform>();

    /// <summary>
    /// 戦闘状態かどうか
    /// </summary>
    public bool m_IsCombat => m_ActiveEnemies.Count > 0;

    private void Awake()
    {
        m_BattleInstance = this;
    }

    /// <summary>
    /// プレイヤーを発見した敵を追加
    /// </summary>
    public void EnemyFoundPlayer(Transform enemyTransform)
    {
        m_ActiveEnemies.Add(enemyTransform);
    }

    /// <summary>
    /// 敵がプレイヤーを見失った時に削除
    /// </summary>
    public void EnemyLostPlayer(Transform enemyTransform)
    {
        m_ActiveEnemies.Remove(enemyTransform);
    }
}
