using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;

/// <summary>
/// タイトル画面のカメラ遷移を制御するクラス
/// CinemachineBlendListCameraに対応
/// </summary>
public class TitleCameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] CinemachineBlendListCamera m_BlendListCamera;
    [SerializeField] CinemachineVirtualCamera m_Vcam2; // 移動先（到達判定用）

    [Header("Target")]
    [SerializeField] GameObject m_CameraLookTarget;

    [Header("Parallax")]
    [Tooltip("遷移完了後に有効化するMouseLookParallaxコンポーネント")]
    [SerializeField] MouseLookParallax m_MouseLookParallax;

    void Start()
    {
        // MouseLookParallaxを最初は無効にする
        if (m_MouseLookParallax != null)
        {
            m_MouseLookParallax.enabled = false;
        }

        MonitorBlendListAsync().Forget();
    }

    /// <summary>
    /// BlendListCameraの状態を監視し、指定のカメラへ遷移完了後にターゲットを有効化する
    /// </summary>
    private async UniTaskVoid MonitorBlendListAsync()
    {
        if (m_BlendListCamera == null || m_Vcam2 == null) return;

        // 1. LiveChild が Vcam2 になるのを待つ
        // LiveChildインターフェースとの参照比較
        await UniTask.WaitUntil(() => IsLiveChild(m_Vcam2));

        // 2. ブレンド（移動）時間を取得して待機する
        float blendTime = 0f;
        if (m_BlendListCamera.m_Instructions != null)
        {
            // Vcam2への遷移設定（Instruction）を探す
            var instruction = m_BlendListCamera.m_Instructions
                .FirstOrDefault(ins => ins.m_VirtualCamera == m_Vcam2);
            
            // 設定が見つかればそのブレンド時間を採用
            if (instruction.m_VirtualCamera == m_Vcam2)
            {
                blendTime = instruction.m_Blend.m_Time;
            }
        }

        // ブレンド時間分だけ待機（ミリ秒変換）
        // Mathf.FloorToInt で確実に整数にする
        if (blendTime > 0f)
        {
            await UniTask.Delay((int)(blendTime * 1000));
        }

        // 3. ターゲットを有効化
        if (m_CameraLookTarget != null)
        {
            m_CameraLookTarget.SetActive(true);
        }

        // 4. MouseLookParallaxを有効化
        if (m_MouseLookParallax != null)
        {
            m_MouseLookParallax.enabled = true;
        }
    }

    /// <summary>
    /// 指定された仮想カメラが現在のLiveChildか判定する
    /// </summary>
    private bool IsLiveChild(ICinemachineCamera targetCam)
    {
        return m_BlendListCamera.LiveChild == targetCam;
    }
}
