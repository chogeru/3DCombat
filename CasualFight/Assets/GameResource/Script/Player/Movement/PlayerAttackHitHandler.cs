using Cysharp.Threading.Tasks;
using StateMachineAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// プレイヤーの攻撃判定処理(攻撃判定はアニメーションイベントで1から記入していく)
/// 「0」を「ダメージ判定なし（エフェクトのみ）」や「システム予約」として設計にする。
/// 「1」から使い始めることで、「0 ＝ 何も起きない」という安全策を講じている
/// </summary>
public class PlayerAttackHitHandler : MonoBehaviour
{
    [Header("判定させるレイヤー"), SerializeField]
    LayerMask m_LayerMaskEnemy;

    [Header("コンボ順に(段数ごと)")]
    [Header("判定の大きさ(半径)"), SerializeField]
    float[] m_Radii = { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2.5f };
    [Header("判定の大きさ(奥行)"), SerializeField]
    float[] m_Distance = { 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.5f };

    [Header("与えるダメージ"), SerializeField]
    int[] m_Damages = { 10, 10, 10, 10, 10, 30 };

    [Header("ヒットしたときに表示させるエフェクト"), SerializeField]
    GameObject m_HitEffectPrefab;

    //ヒットストップの複数ヒットしたときの重複対策フラグ
    bool m_IsHitStopping = false;

    /// <summary>
    /// アニメーションイベントで呼ばれる当たり判定処理
    /// </summary>
    /// <param name="hit"></param>
    void OnAttackHitCheck(int step)
    {
        //コンボ取得
        int index = Mathf.Clamp(step - 1, 0, m_Radii.Length - 1);

        //奥行の位置を決め、中心とする(足元にEffectが出るので１メートル足す)
        Vector3 hitCenter = transform.position + Vector3.up * 1.0f + transform.forward * m_Distance[index];

        //指定した中心点から指定した半径（Radii）の見えない球体を一瞬だけ発生
        Collider[] hitenemys = Physics.OverlapSphere(hitCenter, m_Radii[index], m_LayerMaskEnemy);

        foreach (var enemy in hitenemys)
        {
            if (enemy.TryGetComponent<AITester>(out var hitEnemy))
            {
                //敵のコライダーを利用し、判定の中心（hitCenter）に一番近い表面の点を探す
                Vector3 preciseHitPoint = enemy.ClosestPoint(hitCenter);

                //その点を渡す
                ApplyHit(hitEnemy, index, preciseHitPoint);
            }
        }
    }

    /// <summary>
    /// ヒットしたときの敵側の処理
    /// </summary>
    /// <param name="target"></param>
    /// <param name="index"></param>
    void ApplyHit(AITester target, int index, Vector3 preciseHitPoint)
    {
        //安全対策でダメージがおかしくないように
        int damageIndex = Mathf.Clamp(index, 0, m_Damages.Length - 1);

        //ダメージ処理
        target.TakeDamage(m_Damages[damageIndex]);

        //エフェクトを生成
        ShowHitEffect(target, preciseHitPoint);

        //ヒットストップ処理
        HitStopAsync(0.06f).Forget();

        Debug.Log($"{target.name} にヒット！");
    }

    /// <summary>
    /// 指定時間ヒットストップ
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    async UniTaskVoid HitStopAsync(float time)
    {
        //すでにヒットストップ中ならスキップ
        if (m_IsHitStopping)
            return;

        //フラグON
        m_IsHitStopping = true;

        //ゲーム時間スロー
        Time.timeScale = 0.05f;

        //現実世界で測る
        await UniTask.Delay(TimeSpan.FromSeconds(time), ignoreTimeScale: true);

        //元に戻す
        Time.timeScale = 1.0f;

        //フラグ解除
        m_IsHitStopping = false;
    }

    /// <summary>
    /// エフェクトを生成する処理
    /// </summary>
    /// <param name="target"></param>
    void ShowHitEffect(AITester target, Vector3 preciseHitPoint)
    {
        //胸などの「中心点」を基準に引き算する
        Vector3 center = target.m_HitPosition.position;

        //ヒットした点から敵の中心を引き算し方向を確定、そして元のヒットした衝突地点に合成
        Vector3 effectPos = preciseHitPoint + (preciseHitPoint - center).normalized * 0.05f;

        //回転値なしのヒットポジションし、エフェクトを生成
        GameObject hitEffect = Instantiate(m_HitEffectPrefab, effectPos, Quaternion.identity);

        Destroy(hitEffect, 0.5f);
    }

    /// <summary>
    /// エディタのSceneビューで判定範囲を赤枠で見えるようにする
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // 1段目の設定をリアルタイムに表示
        Vector3 previewPos = transform.position + transform.forward * m_Distance[0];
        Gizmos.DrawWireSphere(previewPos, m_Radii[0]);
    }
}
