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

    [Header("Pause / Resume Button")]
    [SerializeField, Tooltip("Assign your Pause/Resume button here.")]
    private Button m_ExternalButton;
    [SerializeField] private Sprite m_PauseSprite;
    [SerializeField] private Sprite m_ResumeSprite;

    [Header("Fast Forward Button")]
    [SerializeField, Tooltip("Assign your fast-forward (>>) button here.")]
    private Button m_FastForwardButton;

    private int m_CurrentSpeedIndex;
    private bool m_IsPaused;
    private Button m_Button;
    private Image m_PauseButtonImage;
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
        RefreshPauseSprite();
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
        RefreshPauseSprite();
    }

    public bool IsPaused()
    {
        return m_IsPaused;
    }

    public void TogglePaused()
    {
        SetPaused(!m_IsPaused);
    }

    public void ResetToDefaultSpeed()
    {
        m_CurrentSpeedIndex = Mathf.Clamp(m_DefaultSpeedIndex, 0, m_Speeds.Length - 1);
        ApplyTimeScale();
    }

    private void OnSpeedButtonClicked()
    {
        if (m_Speeds == null || m_Speeds.Length == 0)
        {
            return;
        }

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

        m_CurrentSpeedIndex = (m_CurrentSpeedIndex + 1) % m_Speeds.Length;
        ApplyTimeScale();
    }

    private void OnPauseButtonClicked()
    {
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

        TogglePaused();
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

    private void RefreshPauseSprite()
    {
        if (m_PauseButtonImage == null) return;

        m_PauseButtonImage.sprite = m_IsPaused ? m_ResumeSprite : m_PauseSprite;
    }

    private void UpdateVisibility()
    {
        bool show = true;
        if (GameLoopManagerOld.Instance != null)
        {
            show = GameLoopManagerOld.Instance.CurrentState != GameLoopManagerOld.GameState.AugmentSelection
                && GameLoopManagerOld.Instance.CurrentState != GameLoopManagerOld.GameState.GameOver;
        }
        else if (GameLoopManager.Instance != null)
        {
            show = GameLoopManager.Instance.CurrentState != GameLoopManager.GameState.AugmentSelection
                && GameLoopManager.Instance.CurrentState != GameLoopManager.GameState.GameOver;
        }

        if (m_Button != null && m_Button.gameObject.activeSelf != show)
            m_Button.gameObject.SetActive(show);

        if (m_FastForwardButton != null && m_FastForwardButton.gameObject.activeSelf != show)
            m_FastForwardButton.gameObject.SetActive(show);
    }

    private void EnsureButtonExists()
    {
        if (m_ExternalButton != null)
        {
            m_Button = m_ExternalButton;
            m_PauseButtonImage = m_Button.GetComponent<Image>();
            m_ButtonRect = m_Button.GetComponent<RectTransform>();
            m_Button.onClick.RemoveListener(OnPauseButtonClicked);
            m_Button.onClick.AddListener(OnPauseButtonClicked);
        }

        if (m_FastForwardButton != null)
        {
            m_FastForwardButton.onClick.RemoveListener(OnSpeedButtonClicked);
            m_FastForwardButton.onClick.AddListener(OnSpeedButtonClicked);
            return;
        }

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
