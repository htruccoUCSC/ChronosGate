using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays a notification panel at the start of each combat cycle showing
/// which round modifier is active. Fades in, holds, then fades out automatically.
///
/// UNITY SETUP:
///   1. In your Canvas, create a Panel (e.g. "RoundModifierPanel") containing:
///        - A TextMeshProUGUI named "ModifierNameText"   (large, centred — the modifier title)
///        - A TextMeshProUGUI named "ModifierDescText"   (smaller — the description)
///        - Optionally an Image named "ModifierIcon"     (assign sprites per modifier in future)
///      Place the panel wherever you want it on screen (centre, top-of-screen, etc.)
///
///   2. Add this component to the RoundModifierPanel GameObject.
///
///   3. In the Inspector, wire up:
///        - Panel Root     → the RoundModifierPanel RectTransform itself
///        - Name Text      → the ModifierNameText child
///        - Desc Text      → the ModifierDescText child
///        - Canvas Group   → a CanvasGroup component on the RoundModifierPanel
///          (Add Component > CanvasGroup if it isn't there yet)
///
///   4. No other wiring needed — this script subscribes to RoundModifierManager.OnModifiersApplied
///      automatically and drives itself.
///
/// STYLING TIPS:
///   - Give the panel a semi-transparent dark background so it reads over gameplay.
///   - The panel starts hidden (alpha 0, not interactable). It appears only when a
///     modifier fires, holds for 'displayDuration' seconds, then fades back out.
/// </summary>
public class RoundModifierUI : MonoBehaviour
{
    public static RoundModifierUI Instance { get; private set; }

    [Header("References")]
    [Tooltip("The root panel RectTransform to show/hide.")]
    [SerializeField] private RectTransform panelRoot;

    [Tooltip("CanvasGroup on the panel — controls alpha and blocks raycasts while visible.")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("Displays the modifier name (e.g. 'Blitzkrieg').")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("Displays the modifier description.")]
    [SerializeField] private TMP_Text descText;

    [Header("Timing")]
    [Tooltip("How long the panel stays fully visible before fading out.")]
    [SerializeField] private float displayDuration = 3f;

    [Tooltip("Duration of the fade-in and fade-out transitions.")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine activeRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        RoundModifierManager.OnModifiersApplied += HandleModifiersApplied;
    }

    private void OnDisable()
    {
        RoundModifierManager.OnModifiersApplied -= HandleModifiersApplied;
    }

    private void Start()
    {
        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Yields until the current show/hide animation has fully completed.
    /// Used by GameLoopManager.StartCombatWithPreview() to hold the enemy preview
    /// camera pan until this notification has finished displaying.
    /// Returns immediately if no animation is running (no modifier this cycle,
    /// or UI not in scene), so callers never hang.
    /// </summary>
    public IEnumerator WaitUntilHidden()
    {
        while (activeRoutine != null)
            yield return null;
    }

    /// <summary>
    /// Called by the OnModifiersApplied event when a new cycle begins.
    /// Builds the display text and runs the show → hold → hide coroutine.
    /// </summary>
    private void HandleModifiersApplied(IReadOnlyList<RoundModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
            return;

        // For now display only the first modifier — extend this loop if
        // modifiersPerCycle is ever set above 1 in RoundModifierManager
        RoundModifier mod = modifiers[0];

        if (nameText != null) nameText.text = mod.Name;
        if (descText  != null) descText.text  = mod.Description;

        // Cancel any in-progress animation before starting a new one
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        if (canvasGroup != null)
        {
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        if (canvasGroup != null)
        {
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }

        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        activeRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed            += Time.deltaTime;
            canvasGroup.alpha   = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
