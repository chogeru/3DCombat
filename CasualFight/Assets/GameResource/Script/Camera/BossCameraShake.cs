using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCameraShake : MonoBehaviour
{
    [Header("設定"), SerializeField]
    CinemachineVirtualCamera m_VirtualCamera;

    [Header("揺れの強さ"), SerializeField]
    float m_ShakeIntensity = 2.0f;

    [Header("揺れの速さ"), SerializeField]
    float m_ShakeFrequency = 2.0f;

    [SerializeField]
    CinemachineBasicMultiChannelPerlin m_Noise;

    private void Start()
    {

        if(m_VirtualCamera==null)
        {
            m_VirtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        //Noiseコンポーネント取得
        if(m_VirtualCamera!=null)
        {
            m_Noise=m_VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    ///<summary>
    ///アニメーションイベントで呼ぶ(揺れ開始)
    ///</summary>
    public void StartShake()
    {
        m_Noise.m_AmplitudeGain = m_ShakeIntensity;
        m_Noise.m_FrequencyGain = m_ShakeFrequency;
    }

    ///<summary>
    ///アニメーションイベントで呼ぶ(揺れ停止)
    ///</summary>
    public void EndShake()
    {
        m_Noise.m_AmplitudeGain = 0f;
        m_Noise.m_FrequencyGain = 0f;
    }
}
