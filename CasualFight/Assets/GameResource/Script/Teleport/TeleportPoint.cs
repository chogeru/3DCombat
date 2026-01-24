using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーがテレポートを開放させる処理
/// </summary>
public class TeleportPoint : MonoBehaviour
{
    [Header("地点名"), SerializeField]
    string m_PointName = "新エリア";

    [Header("プレビュー画像"), SerializeField] 
    Sprite m_AreaSprite;
    
    [Header("着地地点（空のGameObjectを指定）"), SerializeField]
    Transform m_TeleportTarget;

    [Header("解放時に表示するオブジェクト"), SerializeField]
    GameObject m_ActiveEffect;

    //解放されたかどうかの判定処理
    bool m_IsUnlocked = false;

    //マネージャーからの参照用
    public string PointName => m_PointName;
    public bool IsUnlocked => m_IsUnlocked;
    //テレポート地点が設定されたいない場合は保険として判定用の座標を送る
    public Vector3 TeleportPosition => m_TeleportTarget != null ? m_TeleportTarget.position : transform.position;

    private void Start()
    {
        //最初は非表示
        m_ActiveEffect.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //すでに開放済みならスキップ
        if (m_IsUnlocked)
            return;

        //ヒットしたタグの名前がPlayerなら
        if (other.CompareTag("Player"))
        {
            Unlock();
        }
    }

    /// <summary>
    /// ポイント解放処理
    /// </summary>
    void Unlock()
    {
        //フラグOn(解放)
        m_IsUnlocked = true;

        //オブジェクトの表示
        m_ActiveEffect.SetActive(true);

        Debug.Log($"{m_PointName}が解放されました");
    }
}
