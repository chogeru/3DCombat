using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミニマップ上で敵の方向を示す処理＆ボスの場合鼓動しているみたいに表示
/// </summary>
public class MinimapIconClamper : MonoBehaviour
{
    [Header("プレイヤー"), SerializeField]
    Transform m_Player;
    [Header("自分自身"), SerializeField]
    Transform m_MyEnemy;
    [Header("アイコン"), SerializeField]
    MeshRenderer m_Renderer;

    [Space]

    [Header("ミニマップ設定"), SerializeField]
    float m_MaxRadius = 18f;
    [Header("アイコンの高さ(強さによって大きく)"), SerializeField]
    float m_IconHeight = 15f;

    [Space]

    [Header("強敵演出設定")]
    [Header("ボスならチェック"), SerializeField]
    bool m_IsBoss = false;
    [Header("基本の大きさ（強敵なら大きくする）"), SerializeField]
    float m_BaseScale = 1f;
    [Header("点滅・拡大縮小の速さ"), SerializeField]
    float m_PulseSpeed;

    Vector3 m_InitialScale;

    private void Start()
    {
        //プレイヤーの取得
        var player = Object.FindAnyObjectByType<PlayerController>();

        //座標の適応
        m_Player = player.transform;

        //アイコンサイズの変更
        m_InitialScale = transform.localScale * m_BaseScale;
    }

    private void LateUpdate()
    {
        //プレイヤー・敵それぞれ座標取得
        // 戦闘状態の確認と表示切り替え
        // BattleManagerが存在し、かつ自分がActiveEnemiesに含まれているか確認
        bool isBattleActive = false;
        if (BattleManager.m_BattleInstance != null)
        {
            isBattleActive = BattleManager.m_BattleInstance.m_ActiveEnemies.Contains(m_MyEnemy);
        }

        // 表示・非表示の適用
        if (m_Renderer != null)
        {
            m_Renderer.enabled = isBattleActive;
        }

        // 戦闘中でなければ位置計算などの重い処理はスキップ
        if (!isBattleActive) return;

        Vector3 playerPos = m_Player.position;
        Vector3 enemyPos = m_MyEnemy.position;

        //距離感の計算(高さは無視)
        Vector3 direction = enemyPos - playerPos;
        direction.y = 0f;

        //Float値として取得
        float distance = direction.magnitude;

        //ミニマップ外にいたらtrue
        bool isClamped = distance > m_MaxRadius;

        //エリア外なら
        if (isClamped)
        {
            //プレイヤーの位置を起点として敵がいる方向にミニマップの端まで計算
            Vector3 clampedPos = playerPos + direction.normalized * m_MaxRadius;

            //設定済みの高さを維持
            clampedPos.y = playerPos.y + m_IconHeight;

            //最終的な座標をアイコンにセット
            transform.position = clampedPos;
        }
        //エリア内なら
        else
        {
            Vector3 normalPos = enemyPos;

            //無理やりプレイヤーの高さ+設定した高さに変更
            normalPos.y = playerPos.y + m_IconHeight;

            //計算した新しい位置にアイコンを移動
            transform.position = normalPos;
        }

        //ボスの場合
        if (m_IsBoss)
        {
            //時間経過でサイン波を作り、スケールを伸縮させる
            float pulse = 1.0f + Mathf.Sin(Time.time * m_PulseSpeed) * 0.2f;

            //適応
            transform.localScale = m_InitialScale * pulse;

            //端に張り付いている時だけ、さらに色を点滅させる演出も可能
            if (isClamped && m_Renderer != null)
            {
                //透明度の計算
                float alpha = 0.5f + Mathf.PingPong(Time.time * m_PulseSpeed, 0.5f);

                //透明度の適応
                m_Renderer.material.color = new Color(1, 1, 1, alpha);
            }
        }
        //回転を固定
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
