using Cinemachine;
using System.Globalization;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class CinemachineUserInputZoom : CinemachineExtension
{
    [Header("Input Managerに登録されている入力名"), SerializeField]
    string m_InputName = "Mouse ScrollWheel";

    [Header("スクロールにかける倍率"), SerializeField]
    float m_InputScale = 100f;

    [Header("ズーム可能なFOVの最小値"), SerializeField, Range(1, 179)]
    float m_MinFOV = 10f;

    [Header("ズーム可能なFOVの最小値"), SerializeField, Range(1, 179)]
    float m_MaxFOV = 90f;

    //入力を使う宣言
    public override bool RequiresUserInput => true;

    //1フレーム分のスクロール入力値
    float m_ScrollDelta;

    //現在の FOV 補正量
    float m_AdjustFOV;

    private void Update()
    {
        //マウスホイールの入力を毎フレーム加算(「一瞬の入力がなかったことにならないように代入ではなく加算)
        m_ScrollDelta += Input.GetAxis(m_InputName);
    }


    /// <summary>
    /// Cinemachine の各ステージ処理後に呼ばれるコールバック
    /// </summary>
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        //Aim直後のみ処理
        if (stage != CinemachineCore.Stage.Aim)
            return;

        //Cinemachine が作ったカメラ設定(レンズ)をコピー
        var lens =state.Lens;

        //FOV 補正量を計算
        //スクロール方向に応じて増減
        if (!Mathf.Approximately(m_ScrollDelta, 0))
        {
            m_AdjustFOV=Mathf.Clamp(
                m_AdjustFOV - m_ScrollDelta * m_InputScale,
                m_MinFOV - lens.FieldOfView,
                m_MaxFOV - lens.FieldOfView
            );

            //リセット(初期化)
            m_ScrollDelta = 0;
        }

        //CameraStateは毎フレーム作り直されるため毎回FOV補正を加える
        lens.FieldOfView += m_AdjustFOV;

        //補正したレンズ情報をstateに戻す
        state.Lens = lens;
    }

}
