using Cinemachine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
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

    [Header("プレイヤー"), SerializeField]
    Animator m_Player;

    [Header("ズーム設定")]
    [SerializeField]
    float m_StartFov = 60f;

    [SerializeField]
    float m_EndFov = 15f;

    [Header("移動先カメラ"), SerializeField]
    CinemachineVirtualCamera m_SwingCamera;

    [Header("カメラの配置設定")]
    [Header("終着点の何m前に置くか"),SerializeField]
    float m_FrontOffset = 6.0f;

    [Header("カメラの高さ"),SerializeField]
    float m_CameraHight = 1.2f;

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

    /// <summary>
    /// アニメーションイベント（移動アニメの1フレーム目）から呼ぶ
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid OnAttackStartEvent()
    {
        try
        {
            //現在再生中のアニメーション名を取得
            AnimatorStateInfo stateInfo = m_Player.GetCurrentAnimatorStateInfo(0);

            //移動先を取得
            Vector3 endPosition = GetRootMotionDestination(stateInfo.fullPathHash);
            
            //カメラを親子関係から切り離す
            if(m_SwingCamera.transform.parent!=null)
            {
                m_SwingCamera.transform.SetParent(null);
            }

            //プレイヤーの正面方向取得
            Vector3 forwardDir=m_Player.transform.forward;
        
            //プレイヤーの向いている方向に指定した距離を掛け算し、プレイヤーの終着点を足す（カメラの設置座標）
            Vector3 cameraPos=endPosition+forwardDir*m_FrontOffset;

            //高さの適応
            cameraPos.y = endPosition.y+m_CameraHight;

            //実際にカメラを設置する
            m_SwingCamera.transform.position = cameraPos;

            //キャラクターが突っ込んでくる地点（地面から1m上＝腰付近）を見つめる
            m_SwingCamera.transform.LookAt(endPosition + Vector3.up * 1.0f);

            //前回のカメラ（構え用）の優先度を0にする
            if(m_KamaeCamera != null)
            {
                m_KamaeCamera.m_Priority = 0;
            }

            //こちらのカメラ（スイング用）の有効化（優先度を20にする）
            if(m_SwingCamera != null)
            {
                m_SwingCamera.m_Priority = 20;
            }

            //アニメーションの長さ（秒）を取得
            float animationDuration = stateInfo.length;

            //アニメーション終了まで待機
            await UniTask.Delay(TimeSpan.FromSeconds(animationDuration), cancellationToken: this.GetCancellationTokenOnDestroy());

            //アニメーションが終わると同時に優先度を0に戻す
            if(m_SwingCamera != null)
            {
                m_SwingCamera.m_Priority = 0;
            }
        }
        catch (OperationCanceledException) 
        {
            Debug.Log("キャンセルされました");
            return;
        }
    }

    /// <summary>
    /// ハッシュ値から移動先を計算する補助関数
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    Vector3 GetRootMotionDestination(int stateHash)
    {
        //アニメーターから現在のクリップを取得
        var clips = m_Player.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            //名前が一致したら
            if(m_Player.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash || clip.name.Contains("SP_Attack"))
            {
                //clip.averageSpeed (Vector3) は、1秒間あたりのルートモーション移動量
                //clip.length (float) は、アニメーションの全長（秒）
                //クリップが持つローカルの移動量を取得
                Vector3 localDelta = clip.averageSpeed * clip.length;

                //プレイヤーが現在の向きに合わせてワールド座標に変換
                Vector3 worldDelta = m_Player.transform.rotation * localDelta;

                //現在地点と移動量を加算
                return m_Player.transform.position + worldDelta;
            }
        }

        return m_Player.transform.position;
    }

    /// <summary>
    /// アニメーションイベントから呼び出し。構えカメラの優先度を20にする
    /// </summary>
    public void OnKamaeStartEvent()
    {
        if (m_KamaeCamera != null)
        {
            // 構えカメラの優先度を20に設定
            m_KamaeCamera.m_Priority = 20;
        }
    }
}
