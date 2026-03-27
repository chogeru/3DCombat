using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static SettingsManager Instance { get; private set; }

    [Header("設定画面のパネル"), SerializeField]
    GameObject m_Menu;

    [Header("シネマシーン"), SerializeField]
    CinemachineFreeLook m_FreeLookCamera;

    [Header("音量"), SerializeField]
    GameObject m_VolumeSlider;
    [SerializeField]
    GameObject m_TextVolumeObj;

    [Header("水平"), SerializeField]
    GameObject m_XSlider;
    [SerializeField]
    GameObject m_TextXObj;

    [Header("垂直"), SerializeField]
    GameObject m_YSlider;
    [SerializeField]
    GameObject m_TextYObj;

    [Header("ミニマップ"), SerializeField]
    MinimapIconController m_MIC;

    //メーニュー画面開いているかどうか
    bool m_IsMenuOpen = false;

    // メニューが開いているかどうかのプロパティ
    public bool IsMenuOpen => m_IsMenuOpen;

    const string Key_Volume = "VolumeValue";
    const string Key_X = "SensXValue";
    const string Key_Y = "SensYValue";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //数値を保存(セーブ)
        float savedVolume = PlayerPrefs.GetFloat(Key_Volume, AudioListener.volume);
        float savedX = PlayerPrefs.GetFloat(Key_X, m_FreeLookCamera.m_XAxis.m_MaxSpeed);
        float savedY = PlayerPrefs.GetFloat(Key_Y, m_FreeLookCamera.m_YAxis.m_MaxSpeed);

        //音量の反映
        InitSlider(m_VolumeSlider, m_TextVolumeObj, savedVolume);
        AudioListener.volume = savedVolume;

        //カメラ感度の反映
        if (m_FreeLookCamera != null)
        {
            InitSlider(m_XSlider, m_TextXObj, savedX);
            m_FreeLookCamera.m_XAxis.m_MaxSpeed = savedX;

            InitSlider(m_YSlider, m_TextYObj, savedY);
            m_FreeLookCamera.m_YAxis.m_MaxSpeed = savedY;
        }

        //設定画面非表示
        m_Menu.SetActive(false);

        // カーソル初期化処理
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void Update()
    {
        //エスケープキーを押したら
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TPキャンバスが開いていたら、それを閉じるだけにして終了(優先度高)
            if (TeleportManager.TPInstance != null && TeleportManager.TPInstance.IsUIOpen)
            {
                TeleportManager.TPInstance.CloseUI();
                return;
            }

            //UnlockManagerのUIが開いているか今のフレームで閉じた場合
            if (UnlockManager.m_ActiveUnlockUICount > 0 || UnlockManager.m_LastClosedFrame == Time.frameCount)
            {
                return;
            }

            ToggleSettings();
        }
    }

    /// <summary>
    /// UIの表示・非表示を切り替え処理
    /// </summary>
    void ToggleSettings()
    {
        //フラグの切り替え
        m_IsMenuOpen = !m_IsMenuOpen;

        //表示の切り替え
        m_Menu.SetActive(m_IsMenuOpen);

        //メニュー画面開いているなら
        if (m_IsMenuOpen)
        {
            //時間停止
            Time.timeScale = 0f;

            //固定化解除
            UnityEngine.Cursor.lockState = CursorLockMode.None;

            //カーソルの表示
            UnityEngine.Cursor.visible = true;
            
            // 字幕を一時非表示
            if (GameSubtitleManager.Instance != null) GameSubtitleManager.Instance.Pause();
            // ガイドを一時非表示
            if (OperationGuideManager.Instance != null) OperationGuideManager.Instance.Pause();
        }
        else
        {
            //再開
            Time.timeScale = 1f;

            //カーソルの固定
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;

            //カーソル非表示
            UnityEngine.Cursor.visible = false;
            
            // 字幕を復帰
            if (GameSubtitleManager.Instance != null) GameSubtitleManager.Instance.Resume();
            // ガイドを復帰
            if (OperationGuideManager.Instance != null) OperationGuideManager.Instance.Resume();
        }
    }

    /// <summary>
    /// 水平感度
    /// </summary>
    public void OnSensitivityXChanged(float value)
    {
        m_FreeLookCamera.m_XAxis.m_MaxSpeed = value;

        //保存
        PlayerPrefs.SetFloat(Key_X, value);

        //テキストの更新
        UpdateLabel(m_TextXObj, value);
    }

    /// <summary>
    /// 垂直感度
    /// </summary>
    public void OnSensitivityYChanged(float value)
    {
        m_FreeLookCamera.m_YAxis.m_MaxSpeed = value;

        //保存
        PlayerPrefs.SetFloat(Key_Y, value);

        //テキストの更新
        UpdateLabel(m_TextYObj, value);
    }

    /// <summary>
    /// 音量設定
    /// </summary>
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;

        //保存
        PlayerPrefs.SetFloat(Key_Volume, value);

        //テキストの更新
        UpdateLabel(m_TextVolumeObj, value);
    }

    /// <summary>
    /// ミニマップの回転処理
    /// </summary>
    public void OnMiniMapRotationChanged()
    {
        m_MIC.m_IsPlayerIcon=!m_MIC.m_IsPlayerIcon;

        // 設定画面を閉じる(自動)
        ToggleSettings();
    }

    /// <summary>
    /// 手動リスポーンボタン処理（新規追加）
    /// </summary>
    public void OnRespawnButtonClicked()
    {
        // プレイヤーを探してリスポーン実行
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ManualRespawn();
        }

        // 設定画面を閉じる(自動)
        ToggleSettings();
    }

    /// <summary>
    /// ゲームの終了
    /// </summary>
    public void QuitGame()
    {
        //念のため保存
        PlayerPrefs.Save();

        Application.Quit();
    }

    /// <summary>
    /// 初期値を設定し、テキストを更新する処理
    /// </summary>
    /// <param name="sliderObj"></param>
    /// <param name="textObj"></param>
    /// <param name="startValue"></param>
    void InitSlider(GameObject sliderObj, GameObject textObj, float startValue)
    {
        //オブジェクトからコンポーネント取得
        Slider slider = sliderObj.GetComponent<Slider>();
        slider.value = startValue;

        //テキストの更新
        UpdateLabel(textObj, startValue);
    }

    /// <summary>
    /// テキストの表示更新
    /// </summary>
    /// <param name="textObj"></param>
    /// <param name="value"></param>
    void UpdateLabel(GameObject textObj, float value)
    {
        //オブジェクトからコンポーネント取得
        Text t = textObj.GetComponent<Text>();

        //音量設定のテキストのみ整数で表示
        if (textObj == m_TextVolumeObj)
        {
            t.text = (value * 100f).ToString("0");
        }
        else
        {
            t.text = value.ToString("F2");
        }


    }

    /// <summary>
    /// 全て初期化（デフォルトのの値に直す）
    /// </summary>
    public void ResetDefault()
    {
        //音量
        float defaV = 0.5f;
        //水平
        float defaX = 300f;
        //垂直
        float defaY = 2f;

        //システムに反映
        OnVolumeChanged(defaV);
        OnSensitivityXChanged(defaX);
        OnSensitivityYChanged(defaY);

        //スライダーに反映
        SetSliderValue(m_VolumeSlider, defaV);
        SetSliderValue(m_XSlider, defaX);
        SetSliderValue(m_YSlider, defaY);

        // 設定画面を閉じる(自動)
        ToggleSettings();
    }

    /// <summary>
    /// スライダーに数値を代入処理
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public void SetSliderValue(GameObject obj, float value)
    {
        //コンポーネントの取得
        Slider slider = obj.GetComponent<Slider>();

        //数値を反映
        slider.value = value;
    }
}

