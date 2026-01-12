using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// Playerのヒット処理
/// </summary>
public class PlayerHitController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] PlayerController m_PC;
    [SerializeField] Animator m_Animator;

    [Header("設定")]
    [SerializeField] float m_StunDuration = 0.5f;

    // 硬直中フラグ
    public bool IsStunned { get; private set; } = false;

    // ダメージ処理のメインメソッド
    public void OnDamage(int damage, Vector3 attackerPos, bool isHeavy)
    {
        // 状態チェック（スーパーアーマー判定）
        // 「現在ダッシュ中」かつ「攻撃が『軽いヒット』」であるか判定
        // PlayerControllerのm_IsDashがtrueならダッシュ中
        if (m_PC != null && m_PC.m_IsDash && !isHeavy)
        {
             // 処理を中断（スーパーアーマー状態）
             // アニメーション再生・硬直を行わない
             return;
        }

        // 方向計算（相対ベクトル算出）
        // 敵の位置(attackerPos) - 自分の位置 = 自分から見た敵へのベクトル
        Vector3 direction = (attackerPos - transform.position).normalized;
        
        // 内積計算
        float hitX = Vector3.Dot(transform.right, direction);
        float hitY = Vector3.Dot(transform.forward, direction);

        // Animator反映
        if (m_Animator != null)
        {
            m_Animator.SetFloat("HitX", hitX);
            m_Animator.SetFloat("HitY", hitY);

            // アニメーション再生
            if (isHeavy)
            {
                m_Animator.CrossFade("HeavyBlendTree", 0.1f);
            }
            else
            {
                m_Animator.CrossFade("LightBlendTree", 0.1f);
            }
        }

        // ヒットストップ開始
        StartHitStop().Forget();
    }

    // ヒットストップ処理
    private async UniTaskVoid StartHitStop()
    {
        // 動作停止開始
        
        IsStunned = true;

        // 待機
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_StunDuration));

        // 動作再開
        IsStunned = false;
    }
}
