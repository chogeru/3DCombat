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

        //-----攻撃処理関連-----
        //初期化
        if (m_animator != null)
            m_comboSystem.OnAttackEnd();

        //溜まったトリガーを消去
        if (m_comboSystem != null)
            m_animator.ResetTrigger("Attack_Combo");
    }

    //public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    ComboSystem combo = animator.GetComponent<ComboSystem>();
    //    if (combo != null)
    //    {
    //        combo.OnAttackEnd();
    //    }

    //    animator.ResetTrigger("Attack_Combo");
    //}
}
