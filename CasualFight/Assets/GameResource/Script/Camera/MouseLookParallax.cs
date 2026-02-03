using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パララックス対象のデータクラス
/// </summary>
[System.Serializable]
public class ParallaxTarget
{
    [Tooltip("動かす対象のTransform（通常オブジェクト）またはRectTransform（UI）")]
    public Transform target;

    [Tooltip("X方向の動きの強さ（1=基準と同じ、0.5=半分、-0.5=逆方向で半分）")]
    public float multiplierX = 1f;

    [Tooltip("Y方向の動きの強さ")]
    public float multiplierY = 1f;

    [HideInInspector]
    public Vector3 initialPosition;      // 通常Transform用の初期位置
    [HideInInspector]
    public Vector2 initialAnchoredPos;   // RectTransform用の初期位置
    [HideInInspector]
    public RectTransform rectTransform;  // RectTransformのキャッシュ
}

public class MouseLookParallax : MonoBehaviour
{
    [Header("カメラの向きが変わる最大角度（度）")]
    [SerializeField] float m_RotationRangeX = 5f;  // 左右の最大回転角度
    [SerializeField] float m_RotationRangeY = 3f;  // 上下の最大回転角度

    [Header("UI用の動く範囲（ピクセル単位）")]
    [SerializeField] float m_UIRangeX = 50f;
    [SerializeField] float m_UIRangeY = 30f;

    [Header("動きの滑らかさ")]
    [SerializeField] float m_SmoothTime = 5f;

    [Header("パララックス対象リスト")]
    [Tooltip("複数のオブジェクトをそれぞれ異なる強度で動かせます")]
    [SerializeField] ParallaxTarget[] m_ParallaxTargets;

    // このオブジェクト自身の初期回転
    Quaternion m_InitialRotation;

    /// <summary>
    /// コンポーネントが有効化されるたびに初期回転を記録
    /// </summary>
    void OnEnable()
    {
        // このオブジェクト（カメラ等）の現在の回転を初期回転として記録
        m_InitialRotation = transform.rotation;

        // 各パララックス対象の初期位置を記録
        if (m_ParallaxTargets != null)
        {
            foreach (var parallaxTarget in m_ParallaxTargets)
            {
                if (parallaxTarget.target != null)
                {
                    // RectTransformかどうかをチェック
                    parallaxTarget.rectTransform = parallaxTarget.target as RectTransform;
                    
                    if (parallaxTarget.rectTransform != null)
                    {
                        // UI要素の場合はanchoredPositionを記録
                        parallaxTarget.initialAnchoredPos = parallaxTarget.rectTransform.anchoredPosition;
                    }
                    else
                    {
                        // 通常のTransformの場合はpositionを記録
                        parallaxTarget.initialPosition = parallaxTarget.target.position;
                    }
                }
            }
        }
    }

    void Update()
    {
        // マウスの座標を -1.0 ～ 1.0 の範囲に変換
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // このオブジェクト自身の向きを変える（回転）
        // マウスが右 → カメラは右を向く（Y軸回転）
        // マウスが上 → カメラは上を向く（X軸回転、負の方向）
        Quaternion targetRotation = m_InitialRotation 
            * Quaternion.Euler(-mouseY * m_RotationRangeY, mouseX * m_RotationRangeX, 0);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_SmoothTime);

        // 各パララックス対象を動かす（UI等）
        if (m_ParallaxTargets != null)
        {
            foreach (var parallaxTarget in m_ParallaxTargets)
            {
                if (parallaxTarget.target != null)
                {
                    if (parallaxTarget.rectTransform != null)
                    {
                        // UI要素の場合：anchoredPositionを使用
                        float offsetX = mouseX * m_UIRangeX * parallaxTarget.multiplierX;
                        float offsetY = mouseY * m_UIRangeY * parallaxTarget.multiplierY;

                        Vector2 targetAnchoredPos = parallaxTarget.initialAnchoredPos + new Vector2(offsetX, offsetY);
                        parallaxTarget.rectTransform.anchoredPosition = Vector2.Lerp(
                            parallaxTarget.rectTransform.anchoredPosition,
                            targetAnchoredPos,
                            Time.deltaTime * m_SmoothTime
                        );
                    }
                    else
                    {
                        // 通常オブジェクトの場合：positionを使用
                        float offsetX = mouseX * m_UIRangeX * parallaxTarget.multiplierX;
                        float offsetY = mouseY * m_UIRangeY * parallaxTarget.multiplierY;

                        Vector3 parallaxTargetPos = parallaxTarget.initialPosition + new Vector3(offsetX, offsetY, 0);
                        parallaxTarget.target.position = Vector3.Lerp(
                            parallaxTarget.target.position,
                            parallaxTargetPos,
                            Time.deltaTime * m_SmoothTime
                        );
                    }
                }
            }
        }
    }
}
