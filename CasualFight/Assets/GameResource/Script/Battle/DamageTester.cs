using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTester : MonoBehaviour
{
    [SerializeField] HPBarController m_HealthBarController;
    [SerializeField] SpecialMoveManager m_SMM;

    private float m_CurrentHPPercent = 1.0f; // 100%

    /// <summary>
    /// インスペクターのボタンや、UIボタンから呼び出す用
    /// </summary>
    public void TestDamage()
    {
        // 20%ずつダメージを与える
        m_CurrentHPPercent -= 0.2f;

        // 0以下にならないように制限
        if (m_CurrentHPPercent < 0) m_CurrentHPPercent = 0;

        // HPバーに通知（ここであのピンクの遅延演出が走る）
        if (m_HealthBarController != null)
        {
            m_HealthBarController.OnTakeDamage(m_CurrentHPPercent);
            Debug.Log($"Damage! Current HP: {m_CurrentHPPercent * 100}%");
        }
    }

    // スペースキー以外の「D」キーなどでもテストできるようにする場合
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TestDamage();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (m_SMM != null)
            {
                m_SMM.AddGauge(100f);
                Debug.Log("Debug: Special Gauge Maxed Out via L key.");
            }
        }
    }
}
