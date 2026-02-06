using UnityEngine;
using UnityEngine.UI;
using StateMachineAI;

/// <summary>
/// 敵のHPバーUIを管理するクラス
/// AITesterと連動し、敵の頭上に追従、距離によって表示/非表示を切り替える
/// </summary>
public class EnemyHPUnit : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] Slider m_HPSlider;

    [Header("表示設定")]
    [Tooltip("プレイヤーとの距離がこれ以下で表示")]
    [SerializeField] float m_ShowDistance = 15f;

    [Header("追従設定")]
    [Tooltip("敵の頭上オフセット（Y方向）")]
    [SerializeField] Vector3 m_Offset = new Vector3(0, 2f, 0);

    [Header("アニメーション設定")]
    [Tooltip("HP減少の滑らかさ")]
    [SerializeField] float m_SmoothSpeed = 5f;

    // 内部変数
    AITester m_TargetEnemy;
    Transform m_TargetTransform;
    Transform m_Player;
    RectTransform m_RectTransform;
    RectTransform m_CanvasRectTransform;  // 親Canvasの参照
    Camera m_MainCamera;
    CanvasGroup m_CanvasGroup;
    float m_TargetValue = 1f;
    bool m_IsInitialized = false;
    bool m_IsDestroying = false;
    int m_MaxHP;

    void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_MainCamera = Camera.main;
        m_CanvasGroup = GetComponent<CanvasGroup>();

        // CanvasGroupがなければ追加
        if (m_CanvasGroup == null)
        {
            m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 親Canvasを取得
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            m_CanvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        // プレイヤーを自動で検索
        FindPlayer();

        // 初期状態で非表示
        SetVisible(false);
    }

    /// <summary>
    /// 表示/非表示を切り替え（CanvasGroupを使用）
    /// </summary>
    void SetVisible(bool visible)
    {
        if (m_IsDestroying) return;

        if (m_CanvasGroup != null)
        {
            m_CanvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    /// <summary>
    /// プレイヤーを自動検索
    /// </summary>
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            m_Player = playerObj.transform;
            return;
        }

        PlayerController pc = GameObject.FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            m_Player = pc.transform;
        }
    }

    /// <summary>
    /// AITesterとの紐付けを行う
    /// </summary>
    public void Initialize(AITester enemy)
    {
        if (enemy == null) return;

        m_TargetEnemy = enemy;
        m_TargetTransform = enemy.transform;

        if (enemy.m_EnemyData != null)
        {
            m_MaxHP = enemy.m_EnemyData.m_MaxHp;
        }
        else
        {
            m_MaxHP = enemy.m_EnemyHP > 0 ? enemy.m_EnemyHP : 100;
        }

        if (m_Player == null) FindPlayer();
        if (m_MainCamera == null) m_MainCamera = Camera.main;

        if (m_HPSlider != null)
        {
            m_HPSlider.value = 1f;
        }

        m_IsInitialized = true;
    }

    /// <summary>
    /// Transformのみで初期化（SlimeController等、AITester以外の敵用）
    /// </summary>
    public void Initialize(Transform enemyTransform, int maxHP = 100)
    {
        if (enemyTransform == null) return;

        m_TargetEnemy = null;
        m_TargetTransform = enemyTransform;
        m_MaxHP = maxHP;

        if (m_Player == null) FindPlayer();
        if (m_MainCamera == null) m_MainCamera = Camera.main;

        if (m_HPSlider != null)
        {
            m_HPSlider.value = 1f;
        }

        m_IsInitialized = true;
    }

    void LateUpdate()
    {
        if (!m_IsInitialized || m_IsDestroying) return;

        Transform targetTransform = GetTargetTransform();

        if (targetTransform == null)
        {
            DestroyUnit();
            return;
        }

        if (m_TargetEnemy != null && m_TargetEnemy.m_IsDead)
        {
            DestroyUnit();
            return;
        }

        // 距離による表示/非表示
        bool isVisible = UpdateVisibility();

        if (isVisible)
        {
            UpdatePosition();
            UpdateHPFromEnemy();
            UpdateSliderValue();
        }
    }

    /// <summary>
    /// 敵の頭上にUIを追従させる
    /// </summary>
    void UpdatePosition()
    {
        Transform targetTransform = GetTargetTransform();
        if (targetTransform == null || m_MainCamera == null || m_RectTransform == null) return;

        // 敵のワールド座標をスクリーン座標に変換
        Vector3 worldPos = targetTransform.position + m_Offset;
        Vector3 screenPoint = m_MainCamera.WorldToScreenPoint(worldPos);

        // カメラの背後にいる場合は非表示
        if (screenPoint.z < 0)
        {
            SetVisible(false);
            return;
        }

        // スクリーン座標をCanvas内のローカル座標に変換
        if (m_CanvasRectTransform != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_CanvasRectTransform,
                new Vector2(screenPoint.x, screenPoint.y),
                null,  // Screen Space - Overlayの場合はnull
                out localPoint
            );
            m_RectTransform.anchoredPosition = localPoint;
        }
        else
        {
            // フォールバック: 直接スクリーン座標を使用
            m_RectTransform.position = new Vector3(screenPoint.x, screenPoint.y, 0);
        }
    }

    Transform GetTargetTransform()
    {
        if (m_TargetEnemy != null)
            return m_TargetEnemy.transform;
        return m_TargetTransform;
    }

    void UpdateHPFromEnemy()
    {
        if (m_TargetEnemy == null || m_MaxHP <= 0) return;

        float hpRatio = (float)m_TargetEnemy.m_EnemyHP / m_MaxHP;
        m_TargetValue = Mathf.Clamp01(hpRatio);
    }

    bool UpdateVisibility()
    {
        if (m_IsDestroying) return false;

        // 設定画面が開いている場合は強制非表示
        if (SettingsManager.Instance != null && SettingsManager.Instance.IsMenuOpen)
        {
            SetVisible(false);
            return false;
        }

        Transform targetTransform = GetTargetTransform();
        if (m_Player == null || targetTransform == null) return false;

        float distance = Vector3.Distance(targetTransform.position, m_Player.position);
        bool shouldShow = distance <= m_ShowDistance;

        SetVisible(shouldShow);

        return shouldShow;
    }

    void UpdateSliderValue()
    {
        if (m_HPSlider == null) return;

        m_HPSlider.value = Mathf.Lerp(m_HPSlider.value, m_TargetValue, Time.deltaTime * m_SmoothSpeed);
    }

    public void UpdateHP(float hpRatio)
    {
        m_TargetValue = Mathf.Clamp01(hpRatio);
    }

    void DestroyUnit()
    {
        if (m_IsDestroying) return;
        m_IsDestroying = true;
        m_IsInitialized = false;

        if (m_CanvasGroup != null)
        {
            m_CanvasGroup.alpha = 0f;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        m_IsDestroying = true;
        m_IsInitialized = false;
    }

    public void OnEnemyDeath()
    {
        DestroyUnit();
    }

    public void SetHPImmediate(float hpRatio)
    {
        m_TargetValue = Mathf.Clamp01(hpRatio);
        if (m_HPSlider != null)
        {
            m_HPSlider.value = m_TargetValue;
        }
    }

    public void SetOffset(Vector3 offset)
    {
        m_Offset = offset;
    }
}
