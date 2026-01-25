using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーテレポート場所管理クラス
/// </summary>
public class TeleportPoint : MonoBehaviour
{
    [Header("地点名"), SerializeField]
    string m_PointName = "新エリア";

    [Header("プレビュー画像"), SerializeField] 
    Sprite m_AreaSprite;
    
    [Header("地点座標(GameObject推奨)"), SerializeField]
    Transform m_TeleportTarget;

    [Header("解放時エフェクト"), SerializeField]
    GameObject m_ActiveEffect;

    //一度通ったかの判定
    bool m_IsUnlocked = false;

    //マネージャーの参照用
    public string PointName => m_PointName;
    public bool IsUnlocked => m_IsUnlocked;
    public Sprite AreaSprite => m_AreaSprite; // 画像参照用

    //テレポート地点設定されてない場合は保護として自分自身の座標を送る
    public Vector3 TeleportPosition => m_TeleportTarget != null ? m_TeleportTarget.position : transform.position;

    private void Start()
    {
        //最初は非表示
        if(m_ActiveEffect != null)
            m_ActiveEffect.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //既に解放済みならスキップ
        if (m_IsUnlocked)
            return;

        //ヒットタグの対象がPlayerなら
        if (other.CompareTag("Player"))
        {
            Unlock();
        }
    }

    /// <summary>
    /// ポイント解放
    /// </summary>
    void Unlock()
    {
        //フラグOn(解放)
        m_IsUnlocked = true;

        //エフェクトの表示
        if(m_ActiveEffect != null)
            m_ActiveEffect.SetActive(true);

        Debug.Log($"{m_PointName}解放");
    }
}
