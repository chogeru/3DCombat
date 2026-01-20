using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 最初のTimeline終了後呼ばれる
/// </summary>
public class TimelineEndHandler : MonoBehaviour
{
    [Header("プレイヤーオブジェクト"), SerializeField]
    PlayableDirector m_PlayableDirector;

    [Header("シネマシーン"), SerializeField]
    GameObject m_Cinema;

    [Header("プレイヤーのUI"), SerializeField]
    GameObject m_Canvas;

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

    [SerializeField]
    AbilityAttackSystem m_AA;

    [SerializeField]
    SettingsManager m_SM;

    private void Start()
    {
        //初期時OFF
        m_Canvas.SetActive(false);
        m_PC.enabled = false;
        m_AC.enabled = false;
        m_PG.enabled = false;
        m_SC.enabled = false;
        m_CC.enabled = false;
        m_AA.enabled = false;
        m_SM.enabled = false;
    }

    private void OnEnable()
    {
        //イベント終了時登録
        m_PlayableDirector.stopped += OnTimelineStopped;
    }

    private void OnDisable()
    {
        //オブジェクトが破棄される時にイベント登録を解除(メモリリークの防止)
        m_PlayableDirector.stopped -= OnTimelineStopped;
    }

    /// <summary>
    /// イベントが発生した時に呼び出される
    /// </summary>
    /// <param name="playabledirector"></param>
    void OnTimelineStopped(PlayableDirector playabledirector)
    {
        //再生が終わったDirectorが指定のものであるか確認
        if (m_PlayableDirector == playabledirector)
        {
            //ゲームスタートできるようにON
            m_Cinema.SetActive(false);
            m_Canvas.SetActive(true);
            m_PC.enabled = true;
            m_AC.enabled = true;
            m_PG.enabled = true;
            m_SC.enabled = true;
            m_CC.enabled = true;
            m_AA.enabled = true;
            m_SM.enabled = true;
        }
    }
}
