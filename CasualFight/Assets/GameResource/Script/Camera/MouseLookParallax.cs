using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookParallax : MonoBehaviour
{
    [Header("視線が動く最大範囲")]
    [SerializeField] float m_RangeX = 2.0f;
    [SerializeField] float m_RangeY = 1.0f;

    [Header("動きの滑らかさ")]
    [SerializeField] float m_SmoothTime = 5f;

    //初期位置
    Vector3 m_InitialPosition;

    void Start()
    {
        // 最初の位置（着地時の中心点）を記録
        m_InitialPosition = transform.position;
    }

    void Update()
    {
        // マウスの座標を -1.0 ～ 1.0 の範囲に変換
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // 目標位置を計算（マウスの方向に少しずらす）
        Vector3 targetPos = m_InitialPosition + new Vector3(mouseX * m_RangeX, mouseY * m_RangeY, 0);

        // 線形補間でヌルっと動かす
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * m_SmoothTime);
    }
}
