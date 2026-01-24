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

    [Header("ブリンク"), SerializeField]
    AudioClip m_BlinkSound;

    //再生中の種類
    int m_CurrentState = -1;

    /// <summary>
    /// プレイヤーの移動サウンド処理
    /// </summary>
    /// <param name="no">[0.停止 1.歩き 2.走り]</param>
    public void PlayerSoundMove(int no)
    {
        //再生中の番号と同じならスキップ
        if(m_CurrentState==no)
            return;

        m_AudioSource.loop = true;

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

    /// <summary>
    /// ブリンク音再生
    /// </summary>
    public void PlayBlinkSound()
    {
        if (m_BlinkSound != null && m_AudioSource != null)
        {
            m_AudioSource.PlayOneShot(m_BlinkSound);
        }
    }
}
