using TMPro;
using UnityEngine;

public class CurrencyFlyVfx : MonoBehaviour
{
    private const float DURATION = 0.7f;
    private const float ARC_HEIGHT = 85f;

    private RectTransform m_RectTransform;
    private TextMeshProUGUI m_Label;
    private CanvasGroup m_CanvasGroup;
    private Vector2 m_StartPosition;
    private Vector2 m_TargetPosition;
    private float m_Elapsed;

    public static void Spawn(Canvas canvas, RectTransform targetRect, Vector3 worldOrigin, int amount)
    {
        if (canvas == null || targetRect == null || amount <= 0)
        {
            return;
        }

        Camera worldCamera = Camera.main;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null || worldCamera == null)
        {
            return;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldOrigin);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out Vector2 startLocalPos))
        {
            return;
        }

        Vector3 targetScreenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetRect.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreenPoint, uiCamera, out Vector2 targetLocalPos))
        {
            return;
        }

        GameObject vfxObject = new GameObject("CurrencyFlyVfx", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(CurrencyFlyVfx));
        RectTransform rectTransform = vfxObject.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasRect, false);
        rectTransform.anchoredPosition = startLocalPos;
        rectTransform.sizeDelta = new Vector2(140f, 40f);

        TextMeshProUGUI label = vfxObject.GetComponent<TextMeshProUGUI>();
        label.text = $"+{amount}";
        label.fontSize = 26f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.86f, 0.2f, 1f);
        label.raycastTarget = false;

        CurrencyFlyVfx vfx = vfxObject.GetComponent<CurrencyFlyVfx>();
        vfx.m_RectTransform = rectTransform;
        vfx.m_Label = label;
        vfx.m_CanvasGroup = vfxObject.GetComponent<CanvasGroup>();
        vfx.m_StartPosition = startLocalPos;
        vfx.m_TargetPosition = targetLocalPos;
    }

    private void Update()
    {
        if (m_RectTransform == null || m_CanvasGroup == null)
        {
            Destroy(gameObject);
            return;
        }

        m_Elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(m_Elapsed / DURATION);
        float easedT = 1f - Mathf.Pow(1f - t, 3f);

        Vector2 position = Vector2.LerpUnclamped(m_StartPosition, m_TargetPosition, easedT);
        position.y += Mathf.Sin(t * Mathf.PI) * ARC_HEIGHT;
        m_RectTransform.anchoredPosition = position;

        float scale = Mathf.Lerp(0.85f, 1.15f, Mathf.Sin(t * Mathf.PI * 0.5f));
        m_RectTransform.localScale = new Vector3(scale, scale, 1f);
        m_CanvasGroup.alpha = 1f - Mathf.Clamp01((t - 0.55f) / 0.45f);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
