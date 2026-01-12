using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AbilityAttackSystem : MonoBehaviour
{
    [SerializeField]
    struct SkillData
    {
        [Header("クールタイムの秒数")]
        public float m_CoolTime;
        [Header("表示用Text")]
        public Text coolTimeText;
        [HideInInspector] public bool m_IsCoolingDown;
    }

    [Header("中攻撃(アビリティ攻撃)"), SerializeField]
    SkillData m_Ability;

    [Header("必殺技"), SerializeField]
    SkillData m_Ult;

    private void Update()
    {
        
    }

    async UniTask AbilityCoolTimer()
    {
        //指定した時間待機
        await UniTask.Delay(TimeSpan.FromSeconds(m_Ability.m_CoolTime));

        //1フレームの待機
        await UniTask.Yield();
    }
}
