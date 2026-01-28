using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// 武器の背中と手の入れ替え処理
/// </summary>
public class WeaponSwitch : MonoBehaviour
{
    [Header("手元の武器オブジェクト"), SerializeField]
    GameObject m_HandWeapon;

    [Header("背中の武器オブジェクト"), SerializeField]
    GameObject m_BackWeapon;

    [Header("納刀までの時間"), SerializeField]
    float m_AutoSheatheDuration = 10f;

    [Header("プレイヤーコントローラー"), SerializeField]
    PlayerController m_PC;

    [Header("アニメーター"), SerializeField]
    Animator m_Animator;

    CancellationTokenSource m_Cts;

    // 武器の状態をスクリプトで管理するフラグ
    bool m_IsWeaponActive = false;

    /// <summary>
    /// 武器を抜いているかどうか
    /// </summary>
    public bool IsWeaponDrawn => m_IsWeaponActive;

    /// <summary>
    /// 攻撃時に武器を表示（攻撃フラグも立てる）
    /// </summary>
    public void ShowWeapon()
    {
        // 攻撃フラグON (既存動作維持)
        m_PC.m_IsAttack = true;
        
        // 武器表示処理共通化
        DrawWeaponInternal();
    }

    /// <summary>
    /// 被弾時などに武器を表示（攻撃フラグは立てない）
    /// </summary>
    public void DrawWeapon()
    {
        // 攻撃フラグは変更しない
        DrawWeaponInternal();
    }

    private void DrawWeaponInternal()
    {
        // 状態をActiveにする
        m_IsWeaponActive = true;

        // 既存のタイマーをキャンセル
        m_Cts?.Cancel();
        m_Cts?.Dispose();
        m_Cts = new CancellationTokenSource();

        // 手の武器をON
        m_HandWeapon.SetActive(true);

        // 背中の武器をOFF
        m_BackWeapon.SetActive(false);

        // 納刀タイマー開始
        HideWeaponTimer(m_Cts.Token).Forget();
    }

    // 納刀タイマー一時停止フラグ
    bool m_IsSheathePaused = false;

    /// <summary>
    /// 納刀タイマーの一時停止を設定
    /// </summary>
    public void SetSheathePaused(bool isPaused)
    {
        m_IsSheathePaused = isPaused;
    }

    async UniTask HideWeaponTimer(CancellationToken token)
    {
        try
        {
            float timer = 0f;
            while (timer < m_AutoSheatheDuration)
            {
                // 一時停止中はタイマーを進めない
                if (m_IsSheathePaused)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
                    continue;
                }

                // 移動入力がある場合
                if (m_PC.m_MoveInput.sqrMagnitude > 0.01f)
                {
                    // タイマーをリセット
                    timer = 0f;
                }
                else
                {
                    // 加算
                    timer += Time.deltaTime;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }

            // 納刀状態へ移行（InEquippedを0にし、アニメーション同士の重複を防ぐ）
            m_IsWeaponActive = false;

            // 納刀アニメーション Play
            m_Animator.Play("Idle_to_Idle_Combat", 0, 0f);
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は何もしない
        }
    }

    /// <summary>
    /// アニメーターイベントから呼ばれる想定：実際に背中に戻す処理
    /// </summary>
    public void PositionChangeWeapon()
    {
        // 状態を非Activeにする
        m_IsWeaponActive = false;

        // 手の武器をOFF
        m_HandWeapon?.SetActive(false);

        // 背中の武器をON
        m_BackWeapon?.SetActive(true);
    }

    private void OnDestroy()
    {
        m_Cts?.Cancel();
        m_Cts?.Dispose();
    }
}
