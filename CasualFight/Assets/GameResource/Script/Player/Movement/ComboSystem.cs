using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// コンボ攻撃担当
/// (OnComboWindowをコンボの途中アニメーションイベントで設定する・OnAttackEndに最後のアニメーションイベントに設定する)
/// </summary>
public class ComboSystem : MonoBehaviour
{
    [Header("攻撃コンボ番号"), SerializeField]
    int m_ComboNo = 0;

    [Header("最後にクリックした時間"), SerializeField]
    float m_ClickLastTime = 0f;

    [Header("コンボが途切れるまでの猶予時間"), SerializeField]
    float m_ComboDelay = 0.7f;

    [Header("プレイヤーオブジェクト"), SerializeField]
    Animator m_Animator;

    [Header("プレイヤーオブジェクト"),SerializeField]
    WeaponSwitch m_WeaponSwitch;

    [Header("プレイヤーオブジェクト"), SerializeField]
    AttackModifier m_AM;

    [Header("プレイヤーオブジェクト"), SerializeField]
    PlayerController m_PC;

    [Header("アビリティシステム"), SerializeField]
    AbilityAttackSystem m_AbilityAttackSystem;

    [HideInInspector,Tooltip("クリックされたか判定フラグ")]
    public bool m_InputReserved = false;

    //ゲーム側がOK出してるか判定フラグ
    bool m_CanNextCombo = true;


    /// <summary>
    /// クリックされたときの処理
    /// </summary>
    public void InputAttack()
    {
        // アビリティ使用中（クールダウン中）は通常攻撃不可
        if (m_AbilityAttackSystem != null && m_AbilityAttackSystem.IsAnyAbilityActive())
        {
            return;
        }

        // 最後にクリックした時間を更新（タイムアウト判定用）
        m_ClickLastTime = Time.time;

        //カウントダウン武器の開始
        m_WeaponSwitch.ShowWeapon();

        if (m_ComboNo == 0)
        {
            // まだ何もしていない（待機状態）なら、即座に1打目を出す
            m_InputReserved = false;
            ComboCount();
        }
        else
        {
            // 既に攻撃中なら「予約」だけ入れる
            // ここではまだ m_ComboNo は増やさない！
            m_InputReserved = true;
            Debug.Log("入力を予約しました");
        }
    }

    /// <summary>
    /// アニメーションから次の攻撃OKと言われた時の処理
    /// </summary>
    /// <param name="anim"></param>
    public void OnComboWindow()
    {
        m_CanNextCombo = true;

        if (m_InputReserved)
        {
            ComboCount();
        }
    }

    // 次の段に進んだ直後に古い段の Exit イベントでリセットされるのを防ぐフラグ
    bool m_InputJustUpdated = false;

    /// <summary>
    /// 条件がそろったとき、次の攻撃を出す
    /// </summary>
    public void ComboCount()
    {
        //アニメ前半での誤発動防止
        if (!m_CanNextCombo)
            return;

        //攻撃フラグを立てて、PlayerController側での回転干渉を防ぐ
        if (m_PC != null)
        {
            m_PC.m_IsAttack = true;
        }

        //敵の方向に向かせる
        if (m_AM!=null)
        {
            m_AM.LookAtenemy();
        }

        m_InputReserved = false;
        m_CanNextCombo = false;

        //加算
        m_ComboNo++;

        if (m_ComboNo > 4)
            m_ComboNo = 1;

        // 今のコンボ番号をセットする
        m_Animator.SetInteger("AttackNo", m_ComboNo);
        m_InputJustUpdated = true;

        // 具体的なステート名を直接 CrossFade 指定することで、サブステートマシンの不確定要素を排除
        string stateName = $"Combo_Attack_02_0{m_ComboNo}";
        Debug.Log($"ComboCount: {stateName} を CrossFade 再生します。");
        
        m_Animator.Play(stateName, 0, 0f);
    }

    /// <summary>
    /// 一定時間操作がなかったらコンボを終了する
    /// </summary>
    public void ResetCombo(Animator anim)
    {
        //攻撃してないなら何もしない
        if (m_ComboNo == 0)
            return;

        //最後の攻撃から指定時間を超えたら
        if (Time.time - m_ClickLastTime > m_ComboDelay)
        {
            //リセット処理
            m_ComboNo = 0;
            anim.SetInteger("AttackNo", 0);
            
            // 攻撃フラグをリセット
            if (m_PC != null)
            {
                m_PC.m_IsAttack = false;
            }
            
            // 待機に戻るための処理が必要な場合はここで CrossFade("Angry") 等を呼ぶ
            // 今回は animator 側の自然な遷移に任せる
        }
    }



    /// <summary>
    /// アニメーションがおわったので、コンボ状態初期化
    /// </summary>
    public void OnAttackEnd()
    {
        // 追記：もし次のコンボが入力された直後の Exit イベントなら、リセットをスキップする
        if (m_InputJustUpdated)
        {
            m_InputJustUpdated = false;
            Debug.Log("OnAttackEnd: 次のコンボへ遷移中のためリセットをスキップしました。");
            return;
        }

        m_InputReserved = false;
        m_CanNextCombo = true;
        m_ComboNo = 0;
        m_ClickLastTime = 0f;

        // AnimatorのAttackNoも0にする
        if (m_Animator != null)
        {
            m_Animator.SetInteger("AttackNo", 0);
        }

        // ルートモーションをONに戻す
        if (m_Animator != null)
        {
            m_Animator.applyRootMotion = true;
        }

        // 攻撃終了を通知
        if (m_PC != null)
        {
            m_PC.OnAttackEnd();
        }

        Debug.Log("OnAttackEnd: 攻撃状態を終了し、待機状態に戻ります。");
    }

    /// <summary>
    /// 最後のアニメーションがおわったので、コンボ状態初期化
    /// </summary>
    public void OnFinishAttackEnd()
    {
        m_InputReserved = false;
        m_CanNextCombo = true;
        m_ComboNo = 0;
        m_ClickLastTime = 0f;
        m_Animator.SetInteger("AttackNo", 0);

        // ルートモーションをONに戻す
        m_Animator.applyRootMotion = true;

        // 攻撃終了を通知
        if (m_PC != null)
        {
            m_PC.OnAttackEnd();
        }

        Debug.Log("コンボを完全にリセットしました。次は1段目から出せます。");
    }

    /// <summary>
    /// 強制的にコンボを中断
    /// </summary>
    public void ForceResetCombo()
    {
        m_ComboNo = 0;
        m_InputReserved = false;
        m_CanNextCombo = true;
        m_ClickLastTime = 0f;

        m_Animator.SetInteger("AttackNo", 0);

        // ルートモーションをONに戻す
        m_Animator.applyRootMotion = true;
        
        // 攻撃フラグをリセット
        if (m_PC != null)
        {
            m_PC.m_IsAttack = false;
        }
    }
}
