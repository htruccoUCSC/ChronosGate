using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveCycleProgressUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 m_BarSize = new Vector2(420f, 54f);
    [SerializeField] private Vector2 m_TopOffset = new Vector2(0f, -18f);
    [SerializeField] private Vector2 m_SegmentSpacing = new Vector2(10f, 0f);
    [SerializeField] private string m_RootName = "WaveCycleProgressBar";

    [Header("Colors")]
    [SerializeField] private Color m_BackgroundColor = new Color(0.05f, 0.08f, 0.12f, 0.78f);
    [SerializeField] private Color m_CompleteColor = new Color(0.18f, 0.62f, 0.34f, 1f);
    [SerializeField] private Color m_CurrentColor = new Color(0.92f, 0.70f, 0.19f, 1f);
    [SerializeField] private Color m_UpcomingColor = new Color(0.26f, 0.31f, 0.38f, 1f);
    [SerializeField] private Color m_CurrentFillBackgroundColor = new Color(0.22f, 0.18f, 0.10f, 1f);
    [SerializeField] private Color m_LabelColor = new Color(0.95f, 0.97f, 1f, 1f);

    private RectTransform m_RootRect;
    private readonly List<Image> m_SegmentImages = new List<Image>();
    private readonly List<Image> m_SegmentFillImages = new List<Image>();
    private readonly List<TextMeshProUGUI> m_SegmentLabels = new List<TextMeshProUGUI>();
    private TextMeshProUGUI m_TitleLabel;

    private void Start()
    {
        EnsureUiExists();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void EnsureUiExists()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Transform existing = canvas.transform.Find(m_RootName);
        if (existing != null)
        {
            m_RootRect = existing.GetComponent<RectTransform>();
            CacheExistingPieces(existing);
            return;
        }

        GameObject rootObject = new GameObject(m_RootName, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        rootObject.transform.SetParent(canvas.transform, false);

        m_RootRect = rootObject.GetComponent<RectTransform>();
        m_RootRect.anchorMin = new Vector2(0.5f, 1f);
        m_RootRect.anchorMax = new Vector2(0.5f, 1f);
        m_RootRect.pivot = new Vector2(0.5f, 1f);
        m_RootRect.sizeDelta = m_BarSize;
        m_RootRect.anchoredPosition = m_TopOffset;

        Image background = rootObject.GetComponent<Image>();
        background.color = m_BackgroundColor;

        VerticalLayoutGroup verticalLayout = rootObject.GetComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(12, 12, 10, 10);
        verticalLayout.spacing = 8f;
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;

        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(rootObject.transform, false);
        m_TitleLabel = titleObject.GetComponent<TextMeshProUGUI>();
        m_TitleLabel.alignment = TextAlignmentOptions.Center;
        m_TitleLabel.fontSize = 20f;
        m_TitleLabel.color = m_LabelColor;
        m_TitleLabel.raycastTarget = false;

        GameObject rowObject = new GameObject("Segments", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowObject.transform.SetParent(rootObject.transform, false);

        HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = m_SegmentSpacing.x;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        LayoutElement rowElement = rowObject.AddComponent<LayoutElement>();
        rowElement.flexibleHeight = 1f;
        rowElement.minHeight = 24f;

        for (int i = 0; i < 3; i++)
        {
            GameObject segmentObject = new GameObject($"Wave_{i + 1}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            segmentObject.transform.SetParent(rowObject.transform, false);

            Image segmentImage = segmentObject.GetComponent<Image>();
            segmentImage.color = m_UpcomingColor;
            m_SegmentImages.Add(segmentImage);

            GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(segmentObject.transform, false);

            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObject.GetComponent<Image>();
            fillImage.color = m_CurrentColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;
            m_SegmentFillImages.Add(fillImage);

            LayoutElement segmentLayout = segmentObject.GetComponent<LayoutElement>();
            segmentLayout.flexibleWidth = 1f;
            segmentLayout.minHeight = 24f;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(segmentObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 18f;
            label.color = m_LabelColor;
            label.raycastTarget = false;
            m_SegmentLabels.Add(label);
        }
    }

    private void CacheExistingPieces(Transform existing)
    {
        m_SegmentImages.Clear();
        m_SegmentFillImages.Clear();
        m_SegmentLabels.Clear();

        m_TitleLabel = existing.Find("Title")?.GetComponent<TextMeshProUGUI>();
        Transform row = existing.Find("Segments");
        if (row == null)
        {
            return;
        }

        for (int i = 0; i < row.childCount; i++)
        {
            Transform child = row.GetChild(i);
            Image image = child.GetComponent<Image>();
            Image fillImage = child.Find("Fill")?.GetComponent<Image>();
            TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>(true);
            if (image != null)
            {
                m_SegmentImages.Add(image);
            }
            if (fillImage != null)
            {
                m_SegmentFillImages.Add(fillImage);
            }
            if (label != null)
            {
                m_SegmentLabels.Add(label);
            }
        }
    }

    private void Refresh()
    {
        if (m_RootRect == null)
        {
            return;
        }

        GameLoopManager loop = GameLoopManager.Instance;
        if (loop == null)
        {
            m_RootRect.gameObject.SetActive(false);
            return;
        }

        bool shouldShow = loop.CurrentState == GameLoopManager.GameState.Shopping
            || loop.CurrentState == GameLoopManager.GameState.Combat;
        if (m_RootRect.gameObject.activeSelf != shouldShow)
        {
            m_RootRect.gameObject.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            return;
        }

        int totalWaves = Mathf.Max(1, loop.WavesPerAugmentCycle);
        int currentWaveIndex = Mathf.Clamp(loop.CurrentWaveInCycle, 0, totalWaves - 1);
        float currentWaveFill = 0f;
        if (loop.IsCombatTimerActive && loop.CombatPhaseDuration > 0f)
        {
            currentWaveFill = 1f - (loop.CurrentCombatTimeRemaining / loop.CombatPhaseDuration);
        }

        if (m_TitleLabel != null)
        {
            if (loop.IsCombatTimerActive)
            {
                m_TitleLabel.text = $"Wave Progress {currentWaveIndex + 1}/{totalWaves}  {Mathf.CeilToInt(loop.CurrentCombatTimeRemaining)}s left";
            }
            else
            {
                m_TitleLabel.text = $"Wave Progress {currentWaveIndex + 1}/{totalWaves}";
            }
        }

        int segmentCount = Mathf.Min(Mathf.Min(m_SegmentImages.Count, m_SegmentFillImages.Count), m_SegmentLabels.Count);
        for (int i = 0; i < segmentCount; i++)
        {
            if (m_SegmentLabels[i] != null)
            {
                m_SegmentLabels[i].text = $"Wave {i + 1}";
            }

            Image segmentImage = m_SegmentImages[i];
            Image fillImage = m_SegmentFillImages[i];
            if (segmentImage == null || fillImage == null)
            {
                continue;
            }

            fillImage.fillAmount = 0f;

            if (i < loop.CurrentWaveInCycle)
            {
                segmentImage.color = m_CompleteColor;
                fillImage.color = m_CompleteColor;
                fillImage.fillAmount = 1f;
            }
            else if (i == currentWaveIndex)
            {
                segmentImage.color = m_CurrentFillBackgroundColor;
                fillImage.color = m_CurrentColor;
                fillImage.fillAmount = currentWaveFill;
            }
            else
            {
                segmentImage.color = m_UpcomingColor;
                fillImage.color = m_CurrentColor;
            }
        }
    }
}
