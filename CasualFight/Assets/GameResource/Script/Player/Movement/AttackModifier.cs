using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Å‚à‹ß‚¢“G‚ğ’T‚µ‚Ä‚»‚Ì•ûŒü‚ÉŒü‚©‚¹‚éˆ—
/// </summary>
public class AttackModifier : MonoBehaviour
{
    [Header("õ“G”ÍˆÍ"), SerializeField]
    float m_SearchRadius = 5f;

    [Header("õ“G‚·‚éƒ^ƒO")]
    string m_EnemyTag = "Enemy";

    /// <summary>
    /// “G‚Ì•û‚ğŒü‚­
    /// </summary>
    public void LookAtenemy()
    {
        //EnemyTag‚Éw’è‚³‚ê‚½ƒ^ƒO‚Ìõ“G
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(m_EnemyTag);
        //ˆê”Ô‹ß‚¢“G‚ğ‰Á‚¦‚é•Ï”
        GameObject closestEnemy = null;
        //õ“G”ÍˆÍ‘ã“ü
        float minDistance = m_SearchRadius;

        foreach (GameObject enemy in enemies)
        {
            //©•ª‚ÌˆÊ’u‚Æ‚»‚Ì“G‚ÌˆÊ’u‚Ì‹——£ŠÔ‚ğ‘ª’è
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            //õ“G”ÍˆÍ‚æ‚è‹ß‚¯‚ê‚Î
            if (dist < minDistance)
            {
                //ˆê”Ô‹ß‚¢“G&õ“G”ÍˆÍ‚ğXV
                minDistance = dist;
                closestEnemy = enemy;
            }
        }

        //ˆê”Ô‹ß‚¢“G‚ª‚¢‚ê‚Î
        if (closestEnemy != null)
        {
            //“G‚ÌˆÊ’u‚ğæ“¾
            Vector3 targetPos = closestEnemy.transform.position;
            //Y²‚ÍŒÅ’è
            targetPos.y = transform.position.y;

            //“G‚Ì•ûŒü‚ÉŒü‚©‚¹‚é
            transform.LookAt(targetPos);

            //ƒLƒƒƒ‰ƒNƒ^[ƒRƒ“ƒgƒ[ƒ‰[æ“¾
            CharacterController cc = GetComponent<CharacterController>();
            
            //­‚µ‚½‚¯‚Ä“G‚Ì•ûŒü‚Ö‰Ÿ‚µo‚·
            if (cc != null)
            {
                if (minDistance > 1.0)
                {
                    cc.Move(transform.forward * 0.5f);
                }
            }
        }
    }
}
