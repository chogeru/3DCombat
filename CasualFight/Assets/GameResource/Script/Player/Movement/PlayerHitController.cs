using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// ヒットの強弱判定
/// </summary>
public enum HitSeverity
{
    Light = 0,
    Heavy = 1
}

/// <summary>
/// Playerのヒット処理
/// </summary>
public class PlayerHitController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] PlayerController m_PC;
    [SerializeField] Animator m_Animator;
    [SerializeField] ComboSystem m_CS;

    [Header("設定")]
    [SerializeField] float m_StunDuration = 0.5f;

    // 硬直中フラグ
    public bool IsStunned { get; private set; } = false;

    // ダメージ処理のメインメソッド
    public void OnDamage(int damage, Vector3 attackerPos, HitSeverity severity)
    {
        // 状態チェック（スーパーアーマー判定）
        
        // プレイヤーが無敵状態なら、ヒット処理（アニメーション、硬直、ダメージ通知）を全てスキップする
        if (m_PC != null && m_PC.IsInvincible)
        {
            return;
        }

        // 攻撃中 (m_IsAttack == true) なら、ヒットアニメーションと硬直をスキップして終了する
        if (m_PC != null && m_PC.m_IsAttack)
        {
            // ダメージだけ与える
            m_PC.TakeDamage(damage);
            return;
        }

        // 「現在ダッシュ中」かつ「Lightヒット」であるか判定
        // PlayerControllerのm_IsDashがtrueならダッシュ中
        if (m_PC != null && m_PC.m_IsDash && severity == HitSeverity.Light)
        {
             // 処理を中断（スーパーアーマー状態）
             // アニメーション再生・硬直を行わない
             return;
        }

        // 被弾確定時にコンボ状態を強制リセット（内部状態のズレ防止）
        if (m_CS != null)
        {
            m_CS.ForceResetCombo();
        }

        // HitSeverity.Heavy の場合のみ isHeavy扱い
        bool isHeavy = (severity == HitSeverity.Heavy);

        // PlayerControllerへダメージを通知（HP減少・無敵時間発生）
        if (m_PC != null)
        {
            m_PC.TakeDamage(damage);

            // 死亡していたらここで終了（ヒットリアクションをとらない）
            if (m_PC.IsDead)
            {
                return;
            }
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
            
            // HitSeverityパラメータを設定 (Light=0, Heavy=1)
            m_Animator.SetFloat("HitSeverity", (float)severity);

            // アニメーション再生
            // Animator側のステート名 "HitCount" を再生
            m_Animator.Play("HitCount");
        }

        // ヒットストップ開始
        StartHitStop().Forget();
    }

    // ヒットストップ処理
    // ヒットストップ処理用のキャンセルトークンソース
    System.Threading.CancellationTokenSource m_StunCTS;

    // ヒットストップ処理
    private async UniTaskVoid StartHitStop()
    {
        // 前回の待機をキャンセル（上書き）
        m_StunCTS?.Cancel();
        m_StunCTS = new System.Threading.CancellationTokenSource();
        var token = m_StunCTS.Token;

        // 動作停止開始
        IsStunned = true;

        // 待機（キャンセル時は例外を投げずにboolで返す）
        bool canceled = await UniTask.Delay(System.TimeSpan.FromSeconds(m_StunDuration), cancellationToken: token).SuppressCancellationThrow();

        if (canceled)
        {
            // キャンセルされた＝次の被弾が来て上書きされた、ということなので
            // フラグはいじらずに終了する（新しいタスクに任せる）
            return;
        }

        // 動作再開
        IsStunned = false;
        
        // 硬直解除後、移動入力があれば移動アニメーションへ遷移
        if (m_Animator != null && m_PC != null)
        {
            if (m_PC.m_MoveInput.sqrMagnitude > 0.01f)
            {
                m_Animator.CrossFade("Move", 0.1f);
            }
        }
    }
}
