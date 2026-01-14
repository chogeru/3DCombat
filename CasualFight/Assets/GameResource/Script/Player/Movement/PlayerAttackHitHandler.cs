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

    [Header("コンボ順に")]
    [Header("判定の大きさ(半径)"), SerializeField]
    float[] m_Radii = { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2.5f };
    [Header("判定の大きさ(奥行)"), SerializeField]
    float[] m_Distance = { 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.5f };

    [Header("与えるダメージ"),SerializeField]
    int[] m_Damages = { 10, 10, 10, 10, 10, 30 }; // 段数ごとのダメージ

    /// <summary>
    /// アニメーションイベントで呼ばれる当たり判定処理
    /// </summary>
    /// <param name="hit"></param>
    void OnAttackHitCheck(int step)
    {
        //コンボ取得
        int index = Mathf.Clamp(step - 1, 0, m_Radii.Length - 1);

        //奥行の位置を決め、中心とする
        Vector3 hitCenter = transform.position + transform.forward * m_Distance[index];

        //指定した中心点から指定した半径（Radii）の見えない球体を一瞬だけ発生
        Collider[] hitenemys = Physics.OverlapSphere(hitCenter, m_Radii[index], m_LayerMaskEnemy);

        foreach (var enemy in hitenemys)
        {
            if (enemy.TryGetComponent<AITester>(out var hitEnemy))
            {
                ApplyHit(hitEnemy, index);
            }
        }
    }

    /// <summary>
    /// ヒットしたときの敵側の処理
    /// </summary>
    /// <param name="target"></param>
    /// <param name="index"></param>
    void ApplyHit(AITester target,int index)
    {
        //安全対策でダメージがおかしくないように
        int damageIndex = Mathf.Clamp(index, 0, m_Damages.Length - 1);

        //ダメージ処理
        target.TakeDamage(m_Damages[damageIndex]);

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
        //ゲーム時間スロー
        Time.timeScale = 0.05f;

        //現実世界で測る
        await UniTask.Delay(TimeSpan.FromSeconds(time), ignoreTimeScale: true);

        //元に戻す
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// エディタのSceneビューで判定範囲を赤枠で見えるようにする
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // 1段目の範囲をプレビュー
        Vector3 previewPos = transform.position + transform.forward * 1.2f;
        Gizmos.DrawWireSphere(previewPos, 1.5f);
    }
}
