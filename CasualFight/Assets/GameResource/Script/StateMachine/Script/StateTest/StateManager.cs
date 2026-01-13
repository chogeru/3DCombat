using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("敵のタイプの変更")]
    public int m_TypeNo = 0;
    public GameObject m_Body;
    public Transform m_Player;



    [System.Serializable]

    //キャラ詳細データ
    public struct AINames
    {
        [Header("敵の名前(ニックネーム)")]
        public string m_TypeName;
        public List<string> m_AIName;
    }

    /// <summary>
    /// それぞれのキャラのリスト
    /// </summary>
    public List<AINames> m_Ainame;

    public void Start()
    {
        SetUp();
    }

    /// <summary>
    /// AITester_StateMachineのコンポーネントやクラスなど一括装着
    /// </summary>
    public void SetUp()
    {
        //オブジェクト生成
        GameObject m_Chara = Instantiate(m_Body, transform.position, transform.rotation);
        //キャラクターオブジェクトからステートマシン取得
        AITester m_StateM = m_Chara.GetComponent<AITester>();
        m_StateM.m_Player = m_Player;

        //指定したタイプのリストがない場合通知する
        if (m_Ainame[m_TypeNo].m_AIName.Count == 0)
        {
            Debug.Log("リストはありますが中身が空です");
        }
        else
        {
            //m_TypeNoに指定したタイプのステートをdummyにして入れる
            foreach (string dummy in m_Ainame[m_TypeNo].m_AIName)
            {
                m_StateM.AddStateByName(dummy);
            }
        }



        m_StateM.AISetUp();
    }

    public void ChangePrefab()
    {

    }
}
