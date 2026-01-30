using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    // ダメージを与えるための窓口
    void TakeDamage(int damage);

    // 演出中に敵を止めるための窓口
    void SetFreeze(bool isFrozen);
}
