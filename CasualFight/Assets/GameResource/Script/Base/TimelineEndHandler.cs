using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 最初のTimeline終了後呼ばれる
/// </summary>
public class TimelineEndHandler : MonoBehaviour
{
    [Header("プレイヤーオブジェクト"),SerializeField]
    PlayableDirector m_PlayableDirector;

    [Header("シネマシーン"), SerializeField]
    GameObject m_Cinema;

    [Header("プレイヤーの機能（コンポーネント）")]
    [SerializeField] PlayerController m_PC;

    [SerializeField]
    ActionController m_AC;

    [SerializeField]
    PlayerGravity m_PG;

    [SerializeField]
    StandbyCount m_SC;

    [SerializeField]
    CharacterController m_CC;


    private void Start()
    {
        //初期時OFF
        m_PC.enabled = false;
        m_AC.enabled = false;
        m_PG.enabled = false;
        m_SC.enabled = false;
        m_CC.enabled = false;
    }

    private void OnEnable()
    {
        Debug.Log("2. イベント登録（+=）を行いました。");
        //イベント終了時登録
        m_PlayableDirector.stopped += OnTimelineStopped;
    }

    private void OnDisable()
    {
        //オブジェクトが破棄される時にイベント登録を解除(メモリリークの防止)
        m_PlayableDirector.stopped-=OnTimelineStopped;
    }

    /// <summary>
    /// イベントが発生した時に呼び出される
    /// </summary>
    /// <param name="playabledirector"></param>
    void OnTimelineStopped(PlayableDirector playabledirector)
    {
        Debug.Log("Timelineの停止を検知しました！");
        //再生が終わったDirectorが指定のものであるか確認
        if (m_PlayableDirector==playabledirector)
        {
            Debug.Log("指定されたDirectorと一致したので、オブジェクトを切り替えます");
            //ゲームスタートできるようにON
            m_Cinema.SetActive(false);
            m_PC.enabled = true;
            m_AC.enabled = true;
            m_PG.enabled = true;
            m_SC.enabled = true;
            m_CC.enabled = true;
        }
    }
}
