using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの入力処理
/// </summary>
public class ActionController : MonoBehaviour
{
    [Header("プレイヤーのアニメーター"), SerializeField]
    Animator m_Animator;

    [Header("攻撃処理"), SerializeField]
    ComboSystem m_ComboSystem;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            m_ComboSystem.InputAttack();
        }

        m_ComboSystem.ResetCombo(m_Animator);
    }
}
