using UnityEngine;

public class StatusOverlayVfx : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;
    private Vector3 m_BaseLocalOffset;
    private Vector3 m_SpawnOffset;
    private float m_FloatDistance;
    private float m_Duration;
    private float m_SpawnMoveDuration;
    private float m_Elapsed;
    private Color m_StartColor;

    public void Initialize(
        SpriteRenderer spriteRenderer,
        Vector3 baseLocalOffset,
        Vector3 spawnOffset,
        float floatDistance,
        float duration,
        float spawnMoveDuration,
        float scale,
        Vector3 parentLossyScale)
    {
        m_SpriteRenderer = spriteRenderer;
        m_BaseLocalOffset = baseLocalOffset;
        m_SpawnOffset = spawnOffset;
        m_FloatDistance = floatDistance;
        m_Duration = Mathf.Max(0.01f, duration);
        m_SpawnMoveDuration = Mathf.Max(0f, spawnMoveDuration);

        float targetScale = Mathf.Max(0.01f, scale);
        Vector3 safeParentScale = new Vector3(
            Mathf.Abs(parentLossyScale.x) < 0.0001f ? 1f : Mathf.Abs(parentLossyScale.x),
            Mathf.Abs(parentLossyScale.y) < 0.0001f ? 1f : Mathf.Abs(parentLossyScale.y),
            Mathf.Abs(parentLossyScale.z) < 0.0001f ? 1f : Mathf.Abs(parentLossyScale.z));

        transform.localScale = new Vector3(
            targetScale / safeParentScale.x,
            targetScale / safeParentScale.y,
            targetScale / safeParentScale.z);

        if (m_SpriteRenderer != null)
        {
            m_StartColor = m_SpriteRenderer.color;
        }
        else
        {
            m_StartColor = Color.white;
        }
    }

    private void Update()
    {
        m_Elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(m_Elapsed / m_Duration);
        float spawnT = m_SpawnMoveDuration > 0f ? Mathf.Clamp01(m_Elapsed / m_SpawnMoveDuration) : 1f;
        Vector3 spawnOffset = Vector3.Lerp(m_SpawnOffset, Vector3.zero, spawnT);

        transform.localPosition = m_BaseLocalOffset + spawnOffset + (Vector3.up * m_FloatDistance * t);

        if (m_SpriteRenderer != null)
        {
            Color color = m_StartColor;
            color.a = Mathf.Lerp(m_StartColor.a, 0f, t);
            m_SpriteRenderer.color = color;
        }

        if (m_Elapsed >= m_Duration)
        {
            Destroy(gameObject);
        }
    }

    public static void Spawn(
        Transform target,
        Sprite sprite,
        Vector3 baseLocalOffset,
        Vector3 spawnOffset,
        float floatDistance,
        float duration,
        float spawnMoveDuration,
        float scale,
        int sortingOrderOffset)
    {
        if (target == null || sprite == null)
        {
            return;
        }

        SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
        Transform anchor = targetRenderer != null ? targetRenderer.transform : target;
        Vector3 parentScale = anchor != null ? anchor.lossyScale : Vector3.one;

        GameObject obj = new GameObject("StatusOverlayVfx");
        obj.transform.SetParent(anchor, false);
        obj.transform.localPosition = baseLocalOffset + spawnOffset;

        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        if (targetRenderer != null)
        {
            spriteRenderer.sortingLayerName = targetRenderer.sortingLayerName;
            spriteRenderer.sortingOrder = targetRenderer.sortingOrder + sortingOrderOffset;
        }

        StatusOverlayVfx vfx = obj.AddComponent<StatusOverlayVfx>();
        vfx.Initialize(
            spriteRenderer,
            baseLocalOffset,
            spawnOffset,
            floatDistance,
            duration,
            spawnMoveDuration,
            scale,
            parentScale);
    }
}
