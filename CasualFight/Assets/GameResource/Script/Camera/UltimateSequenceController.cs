using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 必殺技のカメラズーム
/// </summary>
public class UltimateSequenceController : MonoBehaviour
{
    [Header(""), SerializeField]
    CinemachineVirtualCamera m_KamaeCamera;

    [Header(""), SerializeField]
    Animator m_Player;

    [Header("ズーム設定")]
    [SerializeField]
    float m_StartFov = 60f;

    [SerializeField]
    float m_EndFov = 15f;

    public async UniTaskVoid PlayUltimateSequenceAsync()
    {
        try
        {

        }
        catch(OperationCanceledException)
        {
            Debug.Log("今無理");
            return;
        }
    }

    private async UniTask SyncZoomToAnimationAsync()
    {

    }
}
