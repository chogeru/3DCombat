using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーションイベントで呼び出す斬撃エフェクト
/// </summary>
public class WeaponEffectController : MonoBehaviour
{
    [Header("斬撃エフェクト"), SerializeField]
    ParticleSystem m_Slash;

    /// <summary>
    /// アニメーションイベントで呼び出す
    /// </summary>
    public void PlaySlashEffect()
    {
        m_Slash.Stop();
        m_Slash.Play();
    }
}