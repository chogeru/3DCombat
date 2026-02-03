using UnityEngine;
using System.Collections.Generic;
using StateMachineAI;

/// <summary>
/// 指定された中ボス（AITester.m_IsBoss == true）が倒されたら、
/// 指定されたオブジェクト群（障害物など）を削除するクラス
/// </summary>
public class BossObjectDestroyer : MonoBehaviour
{
    [Header("削除対象のオブジェクトリスト")]
    [Tooltip("中ボス撃破時に削除・非表示にするオブジェクト")]
    [SerializeField] List<GameObject> m_Targets;

    [Header("設定")]
    [SerializeField, Tooltip("ボスが見つかるまで検索を続ける間隔（秒）")]
    float m_SearchInterval = 1.0f;

    // ボスの参照
    AITester m_Boss;
    float m_Timer = 0f;

    void Start()
    {
        FindBoss();
    }

    void Update()
    {
        // ボスが未取得なら一定間隔で再検索
        if (m_Boss == null)
        {
            m_Timer += Time.deltaTime;
            if (m_Timer >= m_SearchInterval)
            {
                m_Timer = 0f;
                FindBoss();
            }
            return;
        }

        // ボスのHPが0以下（または死亡フラグ）なら削除実行
        if (m_Boss.m_IsDead || m_Boss.m_EnemyHP <= 0)
        {
            DestroyTargets();
        }
    }

    void FindBoss()
    {
        // シーン内の全てのAITesterを検索
        // 注意: FindObjectsOfTypeは重いので頻繁に呼ばないこと
        AITester[] enemies = FindObjectsOfType<AITester>();
        foreach (var enemy in enemies)
        {
            if (enemy.m_IsBoss)
            {
                m_Boss = enemy;
                Debug.Log($"[{gameObject.name}] ボス({enemy.name})を発見しました。監視を開始します。");
                break;
            }
        }
    }

    void DestroyTargets()
    {
        if (m_Targets != null)
        {
            foreach (var target in m_Targets)
            {
                if (target != null)
                {
                    // 削除ではなくSetActive(false)の方が安全かもしれないが、要望通りDestroyする
                    // 必要に応じて書き換えてください
                    Destroy(target);
                }
            }
        }

        Debug.Log($"[{gameObject.name}] ボス撃破を確認。対象オブジェクトを削除しました。");
        
        // 役割を終えたので自分自身も削除
        Destroy(gameObject);
    }
}
