using Cysharp.Threading.Tasks;
using StateMachineAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの攻撃判定管理
/// </summary>
public class PlayerAttackHitHandler : MonoBehaviour
{
    [Header("判定対象レイヤー"), SerializeField]
    LayerMask m_LayerMaskEnemy;

    [Header("コンボ設定")]
    [Header("判定半径"), SerializeField]
    float[] m_Radii = { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2.5f };
    [Header("判定距離"), SerializeField]
    float[] m_Distance = { 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.5f };

    [Header("ダメージ"), SerializeField]
    int[] m_Damages = { 10, 10, 10, 10, 10, 30 };

    [Header("テレポート攻撃設定 (1撃目: 背後)")]
    [SerializeField] float m_TeleportAttackRadius1 = 2.0f; 
    [SerializeField] float m_TeleportAttackDistance1 = 1.0f;
    [SerializeField] int m_TeleportAttackDamage1 = 20;

    [Header("テレポート攻撃設定 (2撃目: 正面)")]
    [SerializeField] float m_TeleportAttackRadius2 = 2.0f;
    [SerializeField] float m_TeleportAttackDistance2 = 1.0f;
    [SerializeField] int m_TeleportAttackDamage2 = 20;

    [Header("ヒットエフェクト"), SerializeField]
    GameObject m_HitEffectPrefab;

    // ヒットストップ中かどうか
    bool m_IsHitStopping = false;

    /// <summary>
    /// アニメーションイベントで呼ばれる判定
    /// </summary>
    /// <param name="hit"></param>
    void OnAttackHitCheck(int step)
    {
        //コンボ取得
        int index = Mathf.Clamp(step - 1, 0, m_Radii.Length - 1);

        //球の中心位置（Effectが出る位置）
        Vector3 hitCenter = transform.position + Vector3.up * 1.0f + transform.forward * m_Distance[index];

        //球判定（Radii）の範囲内の敵を取得
        Collider[] hitenemys = Physics.OverlapSphere(hitCenter, m_Radii[index], m_LayerMaskEnemy);

        foreach (var enemy in hitenemys)
        {
            if (enemy.TryGetComponent<AITester>(out var hitEnemy))
            {
                // 敵のコライダーを利用、最も近い表面の点を探す
                Vector3 preciseHitPoint = enemy.ClosestPoint(hitCenter);

                // ヒット適用
                ApplyHit(hitEnemy, index, preciseHitPoint);
            }
        }
    }

    /// <summary>
    /// 【アニメーションイベント】テレポート攻撃（1撃目）
    /// </summary>
    public void OnTeleportAttackHitFirst()
    {
        ExecuteTeleportHit(m_TeleportAttackRadius1, m_TeleportAttackDistance1, m_TeleportAttackDamage1, "1撃目");
    }

    /// <summary>
    /// 【アニメーションイベント】テレポート攻撃（2撃目）
    /// </summary>
    public void OnTeleportAttackHitSecond()
    {
        ExecuteTeleportHit(m_TeleportAttackRadius2, m_TeleportAttackDistance2, m_TeleportAttackDamage2, "2撃目");
    }

    /// <summary>
    /// テレポート攻撃のヒット処理共通部分
    /// </summary>
    private void ExecuteTeleportHit(float radius, float distance, int damage, string label)
    {
        // 中心点と半径を設定
        Vector3 hitCenter = transform.position + Vector3.up * 1.0f + transform.forward * distance;

        // 当たり判定
        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, radius, m_LayerMaskEnemy);

        foreach (var enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<AITester>(out var hitEnemy))
            {
                // 専用ダメージ値を適用
                hitEnemy.TakeDamage(damage);

                // エフェクト生成
                Vector3 preciseHitPoint = enemy.ClosestPoint(hitCenter);
                ShowHitEffect(hitEnemy, preciseHitPoint);

                // ヒットストップ
                HitStopAsync(0.06f).Forget();

                Debug.Log($"テレポート攻撃({label})命中: {hitEnemy.name}");
            }
        }
    }

    /// <summary>
    /// ヒット時の処理
    /// </summary>
    /// <param name="target"></param>
    /// <param name="index"></param>
    void ApplyHit(AITester target, int index, Vector3 preciseHitPoint)
    {
        //要素外なら丸める
        int damageIndex = Mathf.Clamp(index, 0, m_Damages.Length - 1);

        //ダメージ
        target.TakeDamage(m_Damages[damageIndex]);

        //エフェクト生成
        ShowHitEffect(target, preciseHitPoint);

        //ヒットストップ
        HitStopAsync(0.06f).Forget();

        Debug.Log($"{target.name} にヒット！");
    }

    /// <summary>
    /// 簡易ヒットストップ
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    async UniTaskVoid HitStopAsync(float time)
    {
        //既にヒットストップならスキップ
        if (m_IsHitStopping)
            return;

        //フラグON
        m_IsHitStopping = true;

        //時間停止
        Time.timeScale = 0.05f;

        //待機
        await UniTask.Delay(TimeSpan.FromSeconds(time), ignoreTimeScale: true);

        //戻す
        Time.timeScale = 1.0f;

        //フラグOFF
        m_IsHitStopping = false;
    }

    /// <summary>
    /// エフェクト生成
    /// </summary>
    /// <param name="target"></param>
    void ShowHitEffect(AITester target, Vector3 preciseHitPoint)
    {
        // ターゲットの中心
        Vector3 center = target.m_HitPosition.position;

        // ヒット点と中心の少し間に出す
        Vector3 effectPos = preciseHitPoint + (preciseHitPoint - center).normalized * 0.05f;

        // 生成
        GameObject hitEffect = Instantiate(m_HitEffectPrefab, effectPos, Quaternion.identity);

        Destroy(hitEffect, 0.5f);
    }

    /// <summary>
    /// エディタでの範囲可視化
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // 1段目の設定、タイムに表示
        Vector3 previewPos = transform.position + transform.forward * m_Distance[0];
        Gizmos.DrawWireSphere(previewPos, m_Radii[0]);
    }
}
