using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// スライムの行動パターンを制御するクラス
    /// </summary>
    public class SlimeController : MonoBehaviour
    {
        public enum SlimeState
        {
            Idle,
            Chase,
            Attack,
            Die
        }

        [Header("State")]
        [SerializeField] private SlimeState m_CurrentState = SlimeState.Idle;

        [Header("References")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private Transform m_Target; // プレイヤーのTransform

        [Header("Settings")]
        [SerializeField] private float m_DetectRange = 10f;
        [SerializeField] private float m_AttackRange = 2f;
        [SerializeField] private float m_MoveSpeed = 3f;
        [SerializeField] private float m_AttackCooldown = 2f;

        private float m_LastAttackTime;
        private bool m_IsDead = false;

        private void Start()
        {
            if (m_Animator == null)
            {
                m_Animator = GetComponent<Animator>();
            }

            // プレイヤーを検索（タグまたは型で検索）
            if (m_Target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    m_Target = player.transform;
                }
                else
                {
                    // PlayerController型で検索
                    var playerController = FindObjectOfType<PlayerController>();
                    if (playerController != null)
                    {
                        m_Target = playerController.transform;
                    }
                }
            }
        }

        private void Update()
        {
            if (m_IsDead) return;

            switch (m_CurrentState)
            {
                case SlimeState.Idle:
                    UpdateIdle();
                    break;
                case SlimeState.Chase:
                    UpdateChase();
                    break;
                case SlimeState.Attack:
                    UpdateAttack();
                    break;
            }
        }

        private void UpdateIdle()
        {
            if (m_Target == null) return;

            float distance = Vector3.Distance(transform.position, m_Target.position);
            if (distance <= m_DetectRange)
            {
                ChangeState(SlimeState.Chase);
                // BattleManagerに戦闘開始通知
                if (BattleManager.m_BattleInstance != null)
                {
                    BattleManager.m_BattleInstance.EnemyFoundPlayer(transform);
                }
            }
        }

        private void UpdateChase()
        {
            if (m_Target == null)
            {
                ChangeState(SlimeState.Idle);
                return;
            }

            float distance = Vector3.Distance(transform.position, m_Target.position);

            // 攻撃範囲内なら攻撃
            if (distance <= m_AttackRange)
            {
                ChangeState(SlimeState.Attack);
                return;
            }

            // 追跡範囲外なら戻る
            if (distance > m_DetectRange * 1.5f) // 追跡解除は少し広め
            {
                ChangeState(SlimeState.Idle);
                if (BattleManager.m_BattleInstance != null)
                {
                    BattleManager.m_BattleInstance.EnemyLostPlayer(transform);
                }
                return;
            }

            // 移動処理（NavMeshAgent使う場合はAgent.SetDestination）
            // ここでは簡易的にTransform移動
            Vector3 direction = (m_Target.position - transform.position).normalized;
            direction.y = 0; // 高さは変えない
            transform.position += direction * m_MoveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(m_Target.position.x, transform.position.y, m_Target.position.z));

            // アニメーション更新
            if (m_Animator != null)
            {
                m_Animator.SetBool("IsMoving", true);
            }
        }

        private void UpdateAttack()
        {
            if (m_Target == null)
            {
                ChangeState(SlimeState.Idle);
                return;
            }

            float distance = Vector3.Distance(transform.position, m_Target.position);
            if (distance > m_AttackRange)
            {
                ChangeState(SlimeState.Chase);
                return;
            }

            // 攻撃クールダウンチェック
            if (Time.time - m_LastAttackTime >= m_AttackCooldown)
            {
                Attack();
            }
        }

        private void Attack()
        {
            m_LastAttackTime = Time.time;
            
            // アニメーショントリガー
            if (m_Animator != null)
            {
                m_Animator.SetTrigger("Attack");
                m_Animator.SetBool("IsMoving", false);
            }

            Debug.Log("Slime attacks!");
        }

        private void ChangeState(SlimeState newState)
        {
            if (m_CurrentState == newState) return;

            m_CurrentState = newState;

            // 状態遷移時の処理
            if (newState == SlimeState.Idle)
            {
                if (m_Animator != null) m_Animator.SetBool("IsMoving", false);
            }
        }

        // ダメージを受ける処理（外部から呼ばれる想定）
        public void TakeDamage(int damage)
        {
            if (m_IsDead) return;

            // HP処理など...
            
            // 死亡処理（例）
            // m_IsDead = true;
            // if (m_Animator != null) m_Animator.SetTrigger("Die");
            // if (BattleManager.m_BattleInstance != null) BattleManager.m_BattleInstance.EnemyLostPlayer(transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_DetectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_AttackRange);
        }
    }
}
