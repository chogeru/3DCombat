using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    [Header("基本ステータス")]
    [Header(" 敵の名前")]
    public string m_EnemyName;
    [Header("最大HP")]
    public int m_MaxHp;
    [Header("移動速度")]
    public float m_MoveSpeed;

    [Space]

    [Header("戦闘設定")]
    [Header("攻撃が届く距離")]
    public float m_AttackRange;
    [Header("プレイヤーを見つける距離")]
    public float m_SearchRange;

    [Space]

    [Header("待機アニメ名")]
    public string m_IdleAnimName;
    [Header("攻撃アニメ名")]
    public string m_AttackAnimName;
    [Header("死亡アニメ名")]
    public string m_DieAnimName;
}
