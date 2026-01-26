using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyDissolveController : MonoBehaviour
{
    [Header("消滅処理を作ったシェーダーグラフアタッチ"), SerializeField]
    Renderer m_Renderer;

    [Header("消滅にかかる時間(秒)"), SerializeField]
    float m_DissolveDuration = 1.5f;

    //[Header("自身のオブジェクト"), SerializeField]
    //GameObject m_DeathVfxPrefab;

    //消滅処理を作ったシェーダーグラフのマテリアルの入れる
    Material m_DissolveMaterial;

    //シェーダーの名前と一致
    readonly int m_DissolveHandle = Shader.PropertyToID("_DissolveAmount");

    // 多重実行防止フラグ
    bool m_IsDissolving = false;

    private void Start()
    {
        //インスタンス化されたマテリアルの取得
        if (m_Renderer != null)
            m_DissolveMaterial = m_Renderer.material;
    }

    /// <summary>
    /// 死んだときに外部から呼ぶ関数
    /// </summary>
    public void StartDissolve()
    {
        // 既に実行中なら何もしない
        if (m_IsDissolving) return;
        m_IsDissolving = true;

        //オブジェクト破棄時にタスクを安全に止めるためのトークン
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        //実行
        DissolveAsync(token).Forget();
    }


    async UniTaskVoid DissolveAsync(CancellationToken token)
    {
        float elapsedTime = 0f;

        //接触対策（死体への追撃や接触を防ぐ）
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }

        //if (m_DeathVfxPrefab != null)
        //{
        //    //敵の場所にエフェクトを生成(親子関係にはしない)
        //    Instantiate(m_DeathVfxPrefab, transform.position, Quaternion.identity);
        //}

        //経過時間内なら
        while (elapsedTime < m_DissolveDuration)
        {
            //オブジェクトが破壊されたら処理を中断
            if (token.IsCancellationRequested)
                return;

            //時間の加算
            elapsedTime += Time.deltaTime;

            //時間内に終わらせるために進捗率を計算
            float rate = elapsedTime / m_DissolveDuration;

            //その結果をシェーダーに送る
            m_DissolveMaterial.SetFloat(m_DissolveHandle, rate);

            //次のフレームまで待機
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        //自身を削除
        Destroy(gameObject);
    }
}
