using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの移動サウンド[0.停止 1.歩き 2.走り]
/// </summary>
public class PlayerMoveSound : MonoBehaviour
{
    [Header("プレイヤーのオーディオソース"),SerializeField]
    AudioSource m_AudioSource;

    [Header("歩き"),SerializeField]
    AudioClip m_WalkSound;

    [Header("走り"), SerializeField]
    AudioClip m_DashSound;

    //再生中の種類
    int m_CurrentState = -1;

    public void PlayerSoundMove(int no)
    {
        m_AudioSource.loop = true;
     
        //再生中の番号と同じならスキップ
        if(m_CurrentState==no)
            return;

        //音の切り替え
        m_CurrentState = no;

        //0番停止
        if(m_CurrentState==0)
        {
            m_AudioSource.Stop();
        }
        else
        {
            //1なら歩き2ならは走りサウンド再生
            m_AudioSource.clip=(no == 1) ? m_WalkSound : m_DashSound;
            m_AudioSource.Play();
        }
    }
}
