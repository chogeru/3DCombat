using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 矢印をプレイヤーの向きに同期させるスクリプト
/// </summary>
public class MinimapPlayerArrowSync : MonoBehaviour
{
    [Header("プレイヤーオブジェクト"), SerializeField]
    Camera m_MainCamera;
    [Header("視野角の値をScaleに変換する際の倍率"), SerializeField]
    float m_WidthMultiplier = 0.015f;

    private void Start()
    {
        //アタッチされていない場合
        if(m_MainCamera==null)
        m_MainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // イベント中は同期しない（カメラが動くため、ミニマップの矢印が荒ぶるのを防ぐ）
        if (GameStateManager.Instance != null)
        {
            var state = GameStateManager.Instance.CurrentState;
            if (state == GameStateManager.GameState.Event || state == GameStateManager.GameState.Dialogue) return;
        }

        //カメラ本体のY軸取得
        float cameraYAngle=m_MainCamera.transform.eulerAngles.y;

        //X軸を平面にし、Y軸回転方向、Z軸は0で固定化し、プレイヤーカメラの向きと同じにする
        transform.rotation=Quaternion.Euler(transform.rotation.x, cameraYAngle, 0);

        //カメラの視野角を取得
        float currentFov = m_MainCamera.fieldOfView;

        //FOVが大きくなると、扇形も横に広がるように計算
        float targetWidth = currentFov * m_WidthMultiplier;

        //現在のスケール取得
        Vector3 objScale = transform.localScale;

        //X幅のみ書き換え
        objScale.x = targetWidth;

        //適応
        transform.localScale = objScale;
    }
}
