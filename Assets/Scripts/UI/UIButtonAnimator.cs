using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Add this component to any UGUI button root.
// Set the Button's Transition to None so Unity's built-in tinting does not
// conflict with this animator.
[RequireComponent(typeof(RectTransform))]
public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale")]
    [SerializeField, Tooltip("Scale multiplier applied when the cursor hovers (PC only).")]
    private float m_HoverScale = 1.05f;

    [SerializeField, Tooltip("Scale multiplier applied while the button is held down.")]
    private float m_PressedScale = 0.96f;

    [SerializeField, Tooltip("Seconds for the scale to animate between states.")]
    private float m_TransitionDuration = 0.08f;

    [Header("Press Highlight")]
    [SerializeField, Tooltip("Optional Image used as a white overlay on press. " +
        "Create a child Image named 'PressHighlight', set its color to white with ~0.25 alpha, " +
        "and assign it here. Leave null to skip this effect.")]
    private Image m_PressHighlightOverlay;

    [Header("Disabled State")]
    [SerializeField, Tooltip("Button to watch for interactable state. Auto-detected on the same GameObject if not assigned.")]
    private Button m_Button;

    [SerializeField, Tooltip("CanvasGroup to fade when disabled. Auto-detected on the same GameObject if not assigned.")]
    private CanvasGroup m_CanvasGroup;

    [SerializeField, Tooltip("CanvasGroup alpha when the button is not interactable.")]
    private float m_DisabledAlpha = 0.4f;

    private RectTransform m_RectTransform;
    private Vector3 m_BaseScale;
    private Coroutine m_ScaleCoroutine;
    private bool m_WasInteractable = true;

    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_BaseScale = m_RectTransform.localScale;

        if (m_Button == null)
            m_Button = GetComponent<Button>();

        if (m_CanvasGroup == null)
            m_CanvasGroup = GetComponent<CanvasGroup>();

        if (m_PressHighlightOverlay != null)
            m_PressHighlightOverlay.enabled = false;

        // Sync initial disabled state.
        if (m_Button != null)
        {
            m_WasInteractable = m_Button.interactable;
            ApplyDisabledVisual(!m_WasInteractable);
        }
    }

    private void Update()
    {
        if (m_Button == null) return;

        bool interactable = m_Button.interactable;
        if (interactable == m_WasInteractable) return;

        m_WasInteractable = interactable;

        if (!interactable)
        {
            SetScale(m_BaseScale);
            if (m_PressHighlightOverlay != null) m_PressHighlightOverlay.enabled = false;
        }

        ApplyDisabledVisual(!interactable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        AnimateScale(m_BaseScale * m_HoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        AnimateScale(m_BaseScale);
        if (m_PressHighlightOverlay != null) m_PressHighlightOverlay.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        AnimateScale(m_BaseScale * m_PressedScale);
        if (m_PressHighlightOverlay != null) m_PressHighlightOverlay.enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        AnimateScale(m_BaseScale * m_HoverScale);
        if (m_PressHighlightOverlay != null) m_PressHighlightOverlay.enabled = false;
    }

    private bool IsInteractable()
    {
        return m_Button == null || m_Button.interactable;
    }

    private void ApplyDisabledVisual(bool disabled)
    {
        if (m_CanvasGroup == null) return;

        m_CanvasGroup.alpha = disabled ? m_DisabledAlpha : 1f;
    }

    private void AnimateScale(Vector3 target)
    {
        if (m_ScaleCoroutine != null) StopCoroutine(m_ScaleCoroutine);

        m_ScaleCoroutine = StartCoroutine(ScaleTo(target));
    }

    private void SetScale(Vector3 target)
    {
        if (m_ScaleCoroutine != null) StopCoroutine(m_ScaleCoroutine);

        m_RectTransform.localScale = target;
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = m_RectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < m_TransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            m_RectTransform.localScale = Vector3.Lerp(start, target, elapsed / m_TransitionDuration);
            yield return null;
        }

        m_RectTransform.localScale = target;
    }
}
