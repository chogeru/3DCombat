using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("敵のタイプの変数")]
    public int m_TypeNo = 0;
    public Transform m_Player;

    [System.Serializable]
    /// <summary>
    /// AIタイプごとの設定データ
    /// </summary>
    public struct AINames
    {
        [Header("敵の名前(ニックネーム)")]
        public string m_TypeName;
        
        [Header("出現させるプレハブ")]
        public GameObject m_Prefab;
        
        [Header("グローバルステートのクラス名")]
        public string m_GlobalStateName;
        
        [Header("使用するステートのリスト")]
        public List<string> m_AIName;
    }

    /// <summary>
    /// それぞれのリスト
    /// </summary>
    public List<AINames> m_Ainame;

    public void Start()
    {
        SetUp();
    }

    /// <summary>
    /// AITester_StateMachineのコンポーネントクラスなどを初期化
    /// </summary>
    public void SetUp()
    {
        // 選択されたAIタイプのデータを取得
        AINames aiData = m_Ainame[m_TypeNo];
        
        // プレハブのnullチェック
        if (aiData.m_Prefab == null)
        {
            Debug.LogError($"タイプ[{m_TypeNo}]: {aiData.m_TypeName} のプレハブが設定されていません！");
            return;
        }
        
        // オブジェクト生成
        GameObject chara = Instantiate(aiData.m_Prefab, transform.position, transform.rotation);
        
        // キャラクターオブジェクトからステートマシン取得
        AITester stateM = chara.GetComponent<AITester>();
        if (stateM == null)
        {
            Debug.LogError($"プレハブにAITesterコンポーネントがありません！");
            return;
        }
        stateM.m_Player = m_Player;
        
        // 1. コンポーネント初期化
        stateM.Initialize();
        
        // 2. グローバルステート設定（AIタイプから取得）
        if (!string.IsNullOrEmpty(aiData.m_GlobalStateName))
        {
            stateM.SetupGlobalStateByName(aiData.m_GlobalStateName);
        }
        
        // 3. ステートの登録（リストから動的に追加）
        if (aiData.m_AIName == null || aiData.m_AIName.Count == 0)
        {
            Debug.Log("ステートリストは空っぽです");
        }
        else
        {
            foreach (string stateName in aiData.m_AIName)
            {
                stateM.AddStateByName(stateName);
            }
        }
        
        // 4. ステートマシン開始
        stateM.StartStateMachine(AIState_Type.Idle);
    }

    public void ChangePrefab()
    {

    }
}
