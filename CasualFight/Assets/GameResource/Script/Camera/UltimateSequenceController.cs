using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 必殺技演出のカメラ制御など
/// </summary>
public class UltimateSequenceController : MonoBehaviour
{
    [Header("構え用カメラ")]
    [SerializeField]
    CinemachineVirtualCamera m_KamaeCamera;

    [Header("プレイヤー")]
    [SerializeField]
    Animator m_Player;

    [Header("ズーム設定")]
    [SerializeField]
    float m_StartFov = 60f;

    [SerializeField]
    float m_EndFov = 15f;

    [Header("WeaponSwitch"), SerializeField]
    WeaponSwitch m_WeaponSwitch;

    [Header("Right Hand Sword (Normal)"), SerializeField]
    GameObject m_RightHandSword;

    [Header("Left Hand Sword (Ultimate)"), SerializeField]
    GameObject m_LeftHandSword;

    /// <summary>
    /// アニメーションイベントなどから呼び出す想定
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid PlayUltimateSequenceAsync()
    {
        // とりあえず実行
        try
        {
            await SyncZoomToAnimationAsync();
        }
        // キャンセル時
        catch(OperationCanceledException)
        {
            Debug.Log("キャンセルされました");
            return;
        }
    }

    [Header("Priority Settings")]
    [SerializeField] int m_HighPriority = 20;
    [SerializeField] int m_LowPriority = 0;

    /// <summary>
    /// アニメーションに合わせてズームしつつ、刀の表示切り替えと納刀タイマー制御を行う
    /// </summary>
    /// <returns></returns>
    private async UniTask SyncZoomToAnimationAsync()
    {

        // 演出開始：納刀タイマー一時停止、刀切り替え、カメラ優先度変更
        if (m_WeaponSwitch != null)
        {
            m_WeaponSwitch.SetSheathePaused(true);
        }
        
        if (m_RightHandSword != null) m_RightHandSword.SetActive(false);
        if (m_LeftHandSword != null) m_LeftHandSword.SetActive(true);

        if (m_KamaeCamera != null)
        {
            m_KamaeCamera.m_Priority = m_HighPriority;
        }

        // アニメーション情報取得
        AnimatorStateInfo stateInfo = m_Player.GetCurrentAnimatorStateInfo(0);

        // アニメーションの長さ取得
        float animationLength = stateInfo.length;

        // タイマー作成
        float elapsed = 0f;

        // アニメーションの長さ分ループ
        while (elapsed < animationLength)
        {
            // 時間の加算
            elapsed += Time.deltaTime;

            // アニメーションの進行度を計算
            float t = elapsed / animationLength;

            // Fovを滑らかに変更
            if (m_KamaeCamera != null)
            {
                m_KamaeCamera.m_Lens.FieldOfView = Mathf.Lerp(m_StartFov, m_EndFov, t);
            }

            // 1フレーム待機
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // 演出停止（アニメーション終了後）：刀戻し、納刀タイマー再開・リセット、カメラ優先度戻す
        
        if (m_LeftHandSword != null) m_LeftHandSword.SetActive(false);
        if (m_RightHandSword != null) m_RightHandSword.SetActive(true);

        if (m_KamaeCamera != null)
        {
            m_KamaeCamera.m_Priority = m_LowPriority;
        }
        
        if (m_WeaponSwitch != null)
        {
            m_WeaponSwitch.SetSheathePaused(false);
            // 機能を再スタート（再表示扱いにしてタイマーリセット）
            m_WeaponSwitch.DrawWeapon(); 
        }
    }
}
