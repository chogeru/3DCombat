using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cinemachine;

/// <summary>
/// ラスボスの死亡アニメーションとDissolve演出、および演出終了時のカメラ復帰を制御するクラス
/// </summary>
public class FinalBossDissolveController : MonoBehaviour
{
    [Header("Dissolve演出用レンダラー")]
    [SerializeField] Renderer m_Renderer;

    [Header("アニメーター")]
    [SerializeField] Animator m_Animator;

    [Header("消滅にかかる時間(秒)")]
    [SerializeField] float m_DissolveDuration = 2.0f;

    [Header("死亡トリガー名")]
    [SerializeField] string m_DieTriggerName = "Die";

    // シェーダーのプロパティID
    readonly int m_DissolveHandle = Shader.PropertyToID("_DissolveAmount");

    // マテリアルインスタンス
    Material m_DissolveMaterial;

    // 処理中フラグ
    bool m_IsDissolving = false;
    
    private void Start()
    {
        // マテリアル取得
        if (m_Renderer != null)
        {
            m_DissolveMaterial = m_Renderer.material;
            // 初期状態はDissolve 0にしておく
            m_DissolveMaterial.SetFloat(m_DissolveHandle, 0);
        }

        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// 死亡時に呼び出される処理
    /// アニメーションのTriggerを引く
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger(m_DieTriggerName);
        }
        else
        {
            Debug.LogWarning("FinalBossDissolveController: Animatorが設定されていません。直ちにDissolveを開始します。");
            StartDissolve();
        }
    }

    /// <summary>
    /// Animation Eventから呼び出されることを想定したDissolve開始関数
    /// </summary>
    public void StartDissolve()
    {
        if (m_IsDissolving) return;
        m_IsDissolving = true;

        // CancellationTokenの取得
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        // 非同期Dissolve処理開始
        DissolveAsync(token).Forget();
    }

    /// <summary>
    /// 非同期でDissolve値を操作し、完了後にカメラを戻して自身を削除する
    /// </summary>
    async UniTaskVoid DissolveAsync(CancellationToken token)
    {
        // 接触判定を消す
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }

        float elapsedTime = 0f;

        // Dissolveアニメーション
        if (m_DissolveMaterial != null)
        {
            while (elapsedTime < m_DissolveDuration)
            {
                if (token.IsCancellationRequested) return;

                elapsedTime += Time.deltaTime;
                float rate = Mathf.Clamp01(elapsedTime / m_DissolveDuration);

                m_DissolveMaterial.SetFloat(m_DissolveHandle, rate);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            // 念のため最後に1.0を入れる
            m_DissolveMaterial.SetFloat(m_DissolveHandle, 1.0f);
        }
        else
        {
            // マテリアルが無い場合は時間だけ待つ
            await UniTask.Delay(System.TimeSpan.FromSeconds(m_DissolveDuration), cancellationToken: token);
        }

        Debug.Log("FinalBossDissolveController: 完全に消滅しました。終了処理を実行します。");

        // 自身（ボスオブジェクト）を削除
        Destroy(gameObject);
    }
}
