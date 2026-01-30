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
    float m_StartFov = 70f;

    [SerializeField]
    float m_EndFov = 15f;

    [Header("移動先カメラ"), SerializeField]
    CinemachineVirtualCamera m_SwingCamera;

    [Header("カメラの配置設定")]
    [Header("終着点の何m前に置くか"),SerializeField]
    float m_FrontOffset = 0.5f;

    [Header("カメラの高さ"),SerializeField]
    float m_CameraHight = 0.2f;

    [Header("注視点の高さ"), SerializeField] 
    float m_LookAtHeight = 0.8f;

    [Header("注視点を右にずらす量"),SerializeField] 
    float m_LookAtSideOffset = 1.0f; 

    [Header("横へのズレ（プラスでキャラが左に寄る）")]
    [SerializeField] float m_SideOffset = 1.5f;

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

    [Header("Main Camera Brain"), SerializeField]
    CinemachineBrain m_MainBrain;

    // 元のブレンド設定保存用
    CinemachineBlendDefinition m_OriginalBlend;

    [Header("視点操作用カメラ(FreeLook)"), SerializeField]
    CinemachineFreeLook m_ControlCamera;

    // 視点操作カメラの元の優先度保存用
    int m_OriginalControlPriority = 10;

    void Start()
    {
        if (m_ControlCamera != null)
        {
            m_OriginalControlPriority = m_ControlCamera.m_Priority;
        }

        // 1回目の開始時から2回目と同じ状態（構えカメラ有効＆低優先度）にしておく
        if (m_KamaeCamera != null)
        {
            m_KamaeCamera.gameObject.SetActive(true);
            m_KamaeCamera.m_Priority = m_LowPriority;
        }

        if (m_SwingCamera != null)
        {
            m_SwingCamera.m_Priority = 0;
        }
    }

    /// <summary>
    /// アニメーションに合わせてズームしつつ、刀の表示切り替えと納刀タイマー制御を行う
    /// </summary>
    /// <returns></returns>
    private async UniTask SyncZoomToAnimationAsync()
    {
        // ブレンド設定をCutに変更
        if (m_MainBrain != null)
        {
            // 現在の設定を保存
            m_OriginalBlend = m_MainBrain.m_DefaultBlend;
            // カット（一瞬）に変更
            m_MainBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0);
        }

        //カメラの優先度の変更
        if (m_KamaeCamera != null)
        {
            m_KamaeCamera.gameObject.SetActive(true);
            m_KamaeCamera.m_Priority = m_HighPriority;
        }


        // 視点操作用カメラをOFFにする
        if (m_ControlCamera != null)
        {
            m_ControlCamera.m_Priority = 0;
            m_ControlCamera.enabled = false;
        }

        // 演出開始：納刀タイマー一時停止、刀切り替え、カメラ優先度変更
        if (m_WeaponSwitch != null)
        {
            m_WeaponSwitch.SetSheathePaused(true);
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

            m_SwingCamera.Follow = null;
            m_SwingCamera.LookAt = null;

            //プレイヤーの正面方向取得
            Vector3 forwardDir=m_Player.transform.forward;

            //プレイヤーの右方向取得
            Vector3 rightDir = m_Player.transform.right;

            // m_SideOffset で右に寄せることで、キャラを相対的に左へ配置する準備をします
            Vector3 cameraPos = endPosition + (forwardDir * m_FrontOffset) + (rightDir * m_SideOffset);

            // 0.2m などの低さにする
            cameraPos.y = endPosition.y + m_CameraHight;

            //実際にカメラを設置する
            m_SwingCamera.transform.position = cameraPos;

            //キャラクターが突っ込んでくる地点を見つめる
            Vector3 lookTarget = endPosition + (Vector3.up * m_LookAtHeight) + (rightDir * m_LookAtSideOffset);

            m_SwingCamera.transform.LookAt(lookTarget);

            //前回のカメラ（構え用）の優先度を0にする
            if (m_KamaeCamera != null)
            {
                m_KamaeCamera.m_Priority = 0;
            }

            //こちらのカメラ（スイング用）の有効化（優先度を40にする）
            if(m_SwingCamera != null)
            {
                m_SwingCamera.m_Priority = 40;
            }

            // Swingスタート: 右手ON、左手OFF
            if (m_RightHandSword != null) m_RightHandSword.SetActive(true);
            if (m_LeftHandSword != null) m_LeftHandSword.SetActive(false);

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
            m_KamaeCamera.gameObject.SetActive(true);
            m_KamaeCamera.m_Priority = 20;
        }

        // 視点操作用カメラを無効化
        if (m_ControlCamera != null)
        {
             m_ControlCamera.m_Priority = 0;
             m_ControlCamera.enabled = false;
        }

        // 構えスタート: 右手OFF、左手ON
        if (m_RightHandSword != null) m_RightHandSword.SetActive(false);
        if (m_LeftHandSword != null) m_LeftHandSword.SetActive(true);
    }

    /// <summary>
    /// アニメーションイベントから呼び出し。Swing終了時の処理（カメラ戻しなど）
    /// </summary>
    public void OnSwingEndEvent()
    {
        // 1. スイングカメラ、構えカメラの優先度を戻す
        if (m_SwingCamera != null) m_SwingCamera.m_Priority = 0;
        if (m_KamaeCamera != null) m_KamaeCamera.m_Priority = m_LowPriority;

        // 2. 武器表示を戻す（左手ON、右手OFF）
        if (m_LeftHandSword != null) m_LeftHandSword.SetActive(false);
        if (m_RightHandSword != null) m_RightHandSword.SetActive(true);

        // 3. WeaponSwitch再開
        if (m_WeaponSwitch != null)
        {
            m_WeaponSwitch.SetSheathePaused(false);
            m_WeaponSwitch.DrawWeapon(); 
        }

        // 4. ブレンド設定を元に戻す
        if (m_MainBrain != null)
        {
            m_MainBrain.m_DefaultBlend = m_OriginalBlend;
        }

        // 5. 視点操作用カメラをONに戻す
        if (m_ControlCamera != null)
        {
            m_ControlCamera.enabled = true;
            m_ControlCamera.m_Priority = m_OriginalControlPriority;
        }
    }
}
