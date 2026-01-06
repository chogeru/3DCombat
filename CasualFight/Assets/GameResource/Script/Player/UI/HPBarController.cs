using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

/// <summary>
/// HPバーを更新する処理
/// </summary>
public class HPBarController : MonoBehaviour
{
    [Header("UIのバー")]
    [Header("前方"), SerializeField]
    Slider m_ForegroundBar;
    [Header("後方"), SerializeField]
    Slider m_BackgroundBar;

    [Space]

    [Header("設定")]
    [Header("遅延開始までの時間"), SerializeField]
    float m_LagDelaySeconds = 0.5f;
    [Header("後方が追いつくまでのスピード"), SerializeField]
    float m_ShrinkSpeed = 2f;

    //valueの値    
    private float m_TargetHealth = 1f;

    /// <summary>
    /// 後方のバーを徐々に減らしていく処理
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid UpdateBackgroundBar()
    {
        //指定時間待機
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_LagDelaySeconds));

        while (m_BackgroundBar.value > m_TargetHealth)
        {
            //徐々に近づけていく
            m_BackgroundBar.value -= Time.deltaTime * m_ShrinkSpeed;
            
            //フレーム待機
            await UniTask.Yield();
        }

        //誤差の修正
        m_BackgroundBar.value = m_TargetHealth;
    }

    /// <summary>
    /// ダメージを受けた時に呼ばれる
    /// </summary>
    /// <param name="currentHP">0.0 ? 1.0</param>
    public void OnTakeDamage(float currentHP)
    {
        //value値更新
        m_TargetHealth=currentHP;

        //前方バーの数値更新
        m_ForegroundBar.value = m_TargetHealth;

        //後方処理開始
        UpdateBackgroundBar().Forget();
    }
}
