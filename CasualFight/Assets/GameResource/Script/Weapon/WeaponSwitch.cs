using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class WeaponSwitch : MonoBehaviour
{
    [Header("手の刀のオブジェクト"), SerializeField]
    GameObject m_HandWeapon;

    [Header("背中の刀のオブジェクト"), SerializeField]
    GameObject m_BackWeapon;

    [Header("武器が消えるまでの時間"), SerializeField]
    float m_WeaponTimer = 10f;

    [Header("プレイヤーオブジェクト"), SerializeField]
    PlayerController m_PC;

    CancellationTokenSource m_Cts;

    /// <summary>
    /// 攻撃時に呼ばれる武器表示の切り替え
    /// </summary>
    public void ShowWeapon()
    {
        //攻撃時攻撃フラグON
        m_PC.m_IsAttack = true;

        //すでに動いているタイマーがあれば停止
        m_Cts?.Cancel();
        m_Cts?.Dispose();
        m_Cts = new CancellationTokenSource();

        //手のオブジェクトON
        m_HandWeapon.SetActive(true);

        //背中のオブジェクトOFF
        m_BackWeapon.SetActive(false);

        //タイマーを開始
        HideWeaponTimer(m_Cts.Token).Forget();
    }

    async UniTask HideWeaponTimer(CancellationToken token)
    {
        try
        {
            //戦闘中なら刀をしまう処理をスキップする
            if (BattleManager.m_BattleInstance.m_IsCombat)
            {
                return;
            }

            //指定した時間待機
            await UniTask.Delay(TimeSpan.FromSeconds(m_WeaponTimer), cancellationToken: token);

            //手のオブジェクトOFF
            m_HandWeapon?.SetActive(false);

            //背中のオブジェクトON
            m_BackWeapon?.SetActive(true);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("攻撃されたのでタイマー中断");
        }
    }

    private void OnDestroy()
    {
        //オブジェクトが壊れた時にタイマーを確実に止める
        m_Cts?.Cancel();
        m_Cts?.Dispose();
    }

    private void Update()
    {
        //移動入力があるかチェック
        if (m_PC.m_MoveInput.sqrMagnitude > 0.01f)
        {
            //戦闘中なら刀をしまう処理をスキップする
            if (BattleManager.m_BattleInstance.m_IsCombat)
            {
                return;
            }

            //すでに動いているタイマーがあれば停止
            m_Cts?.Cancel();
            m_Cts?.Dispose();
            m_Cts = null;

            //手のオブジェクトOFF
            m_HandWeapon?.SetActive(false);

            //背中のオブジェクトON
            m_BackWeapon?.SetActive(true);
        }
    }
}
