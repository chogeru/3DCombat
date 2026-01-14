using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Idle : State<AITester>
{
    public State_Idle(AITester owner) : base(owner){}

    public override void Enter()
    {
        owner.m_Animator.Play(owner.m_EnemyData.m_IdleAnimName,0, 0f);
    }

    public override void Stay()
    {
        //プレイヤーと自身の距離感を求める
        float distance = Vector3.Distance(owner.transform.position, owner.m_Player.position);

        //索敵範囲に入ったら
        if(distance<owner.m_EnemyData.m_SearchRange)
        {
            //追跡に移動
            owner.ChangeState(AIState_Type.Tracking);
        }
    }

    public override void Exit()
    {
        Debug.Log("追跡に移行");
    }

}
