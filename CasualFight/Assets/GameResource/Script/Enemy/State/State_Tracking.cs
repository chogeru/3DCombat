using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Tracking : State<AITester>
{
    public State_Tracking(AITester owner) : base(owner){}

    public override void Enter()
    {
        //アニメーション再生
        owner.m_Animator.Play(owner.m_EnemyData.m_MoveAnimName,0, 0f);
    }

    public override void Stay()
    {
        //プレイヤーとの距離と方向を計算
        Vector3 targetPos = owner.m_Player.position;
        //高さを固定（地上の敵の場合）
        targetPos.y = owner.transform.position.y;

        //方向
        Vector3 direction = (targetPos - owner.transform.position).normalized;

        //距離
        float distance = Vector3.Distance(targetPos, owner.transform.position);

        //プレイヤーの方を向かせる（回転）
        if (direction != Vector3.zero)
        {
            //プレイヤーの方を向くの角度を計算
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            //じわっと回転させる（旋回速度）
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        //攻撃範囲内に入ったら移動を止めて攻撃ステートへ
        if (distance <= owner.m_EnemyData.m_AttackRange)
        {
            owner.ChangeState(AIState_Type.Attack);
        }
        else
        {
            //移動処理
            owner.m_Rigidbody.velocity = direction * owner.m_EnemyData.m_MoveSpeed;
        }
    }

    public override void Exit()
    {
    }
}
