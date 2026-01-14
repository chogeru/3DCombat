using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Attack : State<AITester>
{
    // 移動開始位置
    private Vector3 m_StartPos;
    // 攻撃対象への移動完了フラグ
    private bool m_IsAttacked;
    // 攻撃動作のタイマー
    private float m_StateTimer;
    // 攻撃ステートの最大滞在時間（アニメーション長さに合わせるのが理想だが仮置き）
    private float m_AttackDuration = 1.6f;

    // 移動速度（踏み込みと戻り）
    private float m_StepSpeed = 5.0f;
    // 踏み込みの距離率（0〜1。1ならターゲット位置まで完走）
    private float m_StepRatio = 0.6f;

    public State_Attack(AITester owner) : base(owner) { }

    public override void Enter()
    {
        // 変数初期化
        m_StartPos = owner.transform.position;
        m_IsAttacked = false;
        m_StateTimer = 0f;

        // ルートモーションを無効化（スクリプトで制御するため）
        if (owner.m_Animator != null)
        {
            owner.m_Animator.applyRootMotion = false;
            // 攻撃アニメーション再生
            owner.m_Animator.Play(owner.m_EnemyData.m_AttackAnimName);
        }
    }

    public override void Stay()
    {
        m_StateTimer += Time.deltaTime;

        // ターゲットの方を向く（常に補正、あるいは攻撃前だけ補正など調整可）
        // ここでは攻撃判定が出るまでは向き続けるとする
        if (!m_IsAttacked && owner.m_Player != null)
        {
            Vector3 direction = (owner.m_Player.position - owner.transform.position).normalized;
            direction.y = 0; // 高さは無視
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        // 移動処理
        if (owner.m_Player != null)
        {
            if (!m_IsAttacked)
            {
                // 攻撃前（踏み込み）：ターゲットへ近づく
                // 完全なターゲット位置ではなく、少し手前や一定距離まで
                Vector3 targetPos = Vector3.Lerp(m_StartPos, owner.m_Player.position, m_StepRatio);
                owner.transform.position = Vector3.Lerp(owner.transform.position, targetPos, Time.deltaTime * m_StepSpeed);

                // もし距離が近すぎる場合は止まる、などの処理も可能
            }
            else
            {
                // 攻撃後（戻り）：開始位置へ戻る
                owner.transform.position = Vector3.Lerp(owner.transform.position, m_StartPos, Time.deltaTime * m_StepSpeed);
            }
        }

        // 一定時間経過あるいはアニメーションが終了したら待機に戻る
        if (m_StateTimer >= m_AttackDuration)
        {
            owner.ChangeState(AIState_Type.Idle);
        }
    }

    public override void Exit()
    {
        // ルートモーションを有効化（復帰）
        if (owner.m_Animator != null)
        {
            owner.m_Animator.applyRootMotion = true;
        }
    }

    /// <summary>
    /// Animation Event から呼ばれる攻撃判定
    /// </summary>
    public void OnCheckHit()
    {
        // 既に攻撃済みなら二重発生を防ぐ
        if (m_IsAttacked) return;

        m_IsAttacked = true;

        // 判定発生（球形判定）
        // 位置は自キャラ中心 + 前方少し
        Vector3 center = owner.transform.position + owner.transform.forward * 1.0f;
        float radius = owner.m_EnemyData.m_AttackRange;

        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            // 自分自身は無視
            if (hitCollider.gameObject == owner.gameObject) continue;

            // プレイヤーかどうかの判定（タグやコンポーネントで判断）
            if (hitCollider.CompareTag("Player"))
            {
                // ここでダメージ処理を行う
                Debug.Log($"Hit Player: {hitCollider.name}");

                // 例: PlayerController等のHPを減らす処理
                // var player = hitCollider.GetComponent<PlayerController>();
                // if(player != null) player.TakingDamage(10);
            }
        }

        // デバッグ描画（Sceneビューで確認用）
        Debug.DrawRay(center, Vector3.up, Color.red, 1.0f);
    }
}
