using UnityEngine;

public class GuardSystem : MonoBehaviour
{
    [Header("ガード成功時のゲージ加算量"), SerializeField]
    float m_GuardChargeAmount = 1.0f;

    [Header("SpecialMoveManager"), SerializeField]
    SpecialMoveManager m_SMM;

    /// <summary>
    /// ガード成功時に呼ばれる
    /// </summary>
    public void OnGuardSuccess()
    {
        if (m_SMM != null)
        {
            m_SMM.AddGauge(m_GuardChargeAmount);
            Debug.Log($"ガード成功！スペシャルゲージを {m_GuardChargeAmount} 加算しました。");
        }
    }
}
