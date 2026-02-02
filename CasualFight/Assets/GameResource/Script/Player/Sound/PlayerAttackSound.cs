using UnityEngine;

/// <summary>
/// プレイヤーの攻撃サウンドを管理するクラス。
/// アニメーションイベントから呼び出されて効果音を再生する。
/// </summary>
public class PlayerAttackSound : MonoBehaviour
{
    [Header("オーディオソース")]
    [SerializeField] AudioSource m_AudioSource;

    [Header("攻撃音")]
    [SerializeField, Tooltip("斬撃音")]
    AudioClip m_SlashSound;

    /// <summary>
    /// 【アニメーションイベント用】
    /// 斬撃音を再生する
    /// </summary>
    public void PlaySlashSound()
    {
        if (m_SlashSound != null && m_AudioSource != null)
        {
            m_AudioSource.PlayOneShot(m_SlashSound);
        }
    }
}
