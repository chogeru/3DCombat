using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateRestarter : StateMachineBehaviour
{

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //コンポーネント取得
        Animator m_animator = animator.GetComponent<Animator>();
        ComboSystem m_comboSystem = animator.GetComponent<ComboSystem>();

        //-----攻撃関連-----
        //コンボシステムがあれば攻撃終了を通知
        if (m_comboSystem != null)
            m_comboSystem.OnAttackEnd();

        //アニメーターがあればトリガーをリセット
        if (m_animator != null)
            m_animator.ResetTrigger("Attack_Combo");
    }
}
