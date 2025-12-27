using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 戦闘確認システム
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager m_BattleInstance;

    [Header("自分を狙っている敵の数"), SerializeField]
    int m_EngagedEnemies = 0;

    /// <summary>
    /// 見つかっているかどうかの判定
    /// </summary>
    public bool m_IsCombat => m_EngagedEnemies > 0;

    private void Awake()
    {
        m_BattleInstance = this;
    }

    /// <summary>
    /// プレイヤーを見つけたら加算
    /// </summary>
    public void EnemyFoundPlayer()
    {
        m_EngagedEnemies++;
    }

    /// <summary>
    /// 敵が死んだ、あるいは見失ったら減算
    /// </summary>
    public void EnemyLostPlayer()
    {
        m_EngagedEnemies = Mathf.Max(m_EngagedEnemies - 1, 0);
    }
}
