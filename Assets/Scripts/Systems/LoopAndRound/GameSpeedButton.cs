using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSpeedButton : MonoBehaviour
{
    public static GameSpeedButton Instance { get; private set; }

    [Header("Speed Settings")]
    [SerializeField] private float[] m_Speeds = { 1f, 1.5f, 2f,4f  };
    [SerializeField] private int m_DefaultSpeedIndex = 0;

    [Header("UI Settings")]
    [SerializeField] private Vector2 m_ButtonSize = new Vector2(120f, 42f);
    [SerializeField] private Vector2 m_BottomRightOffset = new Vector2(-20f, 20f);
    [SerializeField] private string m_ButtonName = "GameSpeedButton";

    private int m_CurrentSpeedIndex;
    private bool m_IsPaused;
    private Button m_Button;
    private TextMeshProUGUI m_Label;
    private RectTransform m_ButtonRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        m_CurrentSpeedIndex = Mathf.Clamp(m_DefaultSpeedIndex, 0, m_Speeds.Length - 1);
    }

    private void Start()
    {
        EnsureButtonExists();
        ApplyTimeScale();
        RefreshLabel();
        UpdateVisibility();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    public void SetPaused(bool isPaused)
    {
        m_IsPaused = isPaused;
        ApplyTimeScale();
        RefreshLabel();
    }

    public void ResetToDefaultSpeed()
    {
        m_CurrentSpeedIndex = Mathf.Clamp(m_DefaultSpeedIndex, 0, m_Speeds.Length - 1);
        ApplyTimeScale();
        RefreshLabel();
    }

    private void OnSpeedButtonClicked()
    {
        if (m_Speeds == null || m_Speeds.Length == 0)
        {
            return;
        }

        m_CurrentSpeedIndex = (m_CurrentSpeedIndex + 1) % m_Speeds.Length;
        ApplyTimeScale();
        RefreshLabel();
    }

    private void ApplyTimeScale()
    {
        if (m_IsPaused)
        {
            Time.timeScale = 0f;
            return;
        }

        float speed = GetCurrentSpeed();
        Time.timeScale = Mathf.Max(0.01f, speed);
    }

    private float GetCurrentSpeed()
    {
        if (m_Speeds == null || m_Speeds.Length == 0)
        {
            return 1f;
        }

        int safeIndex = Mathf.Clamp(m_CurrentSpeedIndex, 0, m_Speeds.Length - 1);
        return m_Speeds[safeIndex];
    }

    private void RefreshLabel()
    {
        if (m_Label == null)
        {
            return;
        }

        if (m_IsPaused)
        {
            m_Label.text = "Paused";
            return;
        }

        m_Label.text = $"{GetCurrentSpeed():0.##}x";
    }

    private void UpdateVisibility()
    {
        if (m_Button == null)
        {
            return;
        }

        bool show = true;
        if (GameLoopManager.Instance != null)
        {
            show = GameLoopManager.Instance.CurrentState != GameLoopManager.GameState.AugmentSelection
                && GameLoopManager.Instance.CurrentState != GameLoopManager.GameState.GameOver;
        }

        if (m_Button.gameObject.activeSelf != show)
        {
            m_Button.gameObject.SetActive(show);
        }
    }

    private void EnsureButtonExists()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Transform existing = canvas.transform.Find(m_ButtonName);
        if (existing != null)
        {
            m_Button = existing.GetComponent<Button>();
            m_Label = existing.GetComponentInChildren<TextMeshProUGUI>(true);
            m_ButtonRect = existing.GetComponent<RectTransform>();
            if (m_Button != null)
            {
                m_Button.onClick.RemoveListener(OnSpeedButtonClicked);
                m_Button.onClick.AddListener(OnSpeedButtonClicked);
            }
            return;
        }

        GameObject buttonObject = new GameObject(m_ButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);

        m_ButtonRect = buttonObject.GetComponent<RectTransform>();
        m_ButtonRect.anchorMin = new Vector2(1f, 0f);
        m_ButtonRect.anchorMax = new Vector2(1f, 0f);
        m_ButtonRect.pivot = new Vector2(1f, 0f);
        m_ButtonRect.sizeDelta = m_ButtonSize;
        m_ButtonRect.anchoredPosition = m_BottomRightOffset;

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.65f);

        m_Button = buttonObject.GetComponent<Button>();
        m_Button.targetGraphic = background;
        m_Button.onClick.AddListener(OnSpeedButtonClicked);

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        m_Label = textObject.GetComponent<TextMeshProUGUI>();
        m_Label.alignment = TextAlignmentOptions.Center;
        m_Label.fontSize = 22;
        m_Label.color = Color.white;
        m_Label.raycastTarget = false;
        m_Label.text = "1x";
    }
}
