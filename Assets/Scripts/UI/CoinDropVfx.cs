using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Attach this to the gold coin drop prefab alongside a SpriteRenderer, Animator,
// and a Collider2D (required for click detection).
// Call Launch() immediately after Instantiate to start the bounce arc.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class CoinDropVfx : MonoBehaviour
{
    [Header("Arc")]
    [SerializeField, Tooltip("Horizontal speed range (units/s). Coin flies left or right randomly within this range.")]
    private Vector2 m_HorizontalSpeedRange = new Vector2(0.8f, 1.6f);

    [SerializeField, Tooltip("Initial vertical speed (units/s) for the upward pop.")]
    private float m_InitialVerticalSpeed = 3.5f;

    [SerializeField, Tooltip("Gravity scale applied to the coin during the bounce.")]
    private float m_Gravity = 12f;

    [Header("Idle Fade")]
    [SerializeField, Tooltip("Seconds after landing before the coin auto-fades if not clicked.")]
    private float m_HoldAfterLand = 10f;

    [SerializeField, Tooltip("Seconds for the coin to auto-fade if not clicked.")]
    private float m_FadeDuration = 0.4f;

    [Header("Collect Fly")]
    [SerializeField, Tooltip("Seconds for the coin to fly to the gold counter.")]
    private float m_CollectDuration = 0.5f;

    [SerializeField, Tooltip("Gold awarded to the player on collection.")]
    private int m_GoldValue = 1;

    private enum State { Bouncing, Resting, Collected }

    private SpriteRenderer m_SpriteRenderer;
    private Collider2D m_Collider;
    private Coroutine m_ActiveRoutine;
    private State m_State = State.Bouncing;
    private Vector3 m_BaseScale;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_Collider = GetComponent<Collider2D>();
        m_BaseScale = transform.localScale;
    }

    private void Update()
    {
        if (m_State == State.Collected) return;
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        if (m_Collider != null && m_Collider.OverlapPoint(worldPoint))
            Collect();
    }

    // Kicks off the bounce arc. Called by BaseEnemy.Die() immediately after Instantiate.
    public void Launch()
    {
        transform.localScale = m_BaseScale / 4f;
        m_ActiveRoutine = StartCoroutine(BounceRoutine());
    }
    // Sets the gold value before Launch() is called.
    public void SetGoldValue(int value)
    {
        m_GoldValue = value;
    }

    // Overrides the hold duration before Launch() is called.
    public void SetHoldDuration(float seconds)
    {
        m_HoldAfterLand = seconds;
    }
    private void Collect()
    {
        m_State = State.Collected;
        if (m_ActiveRoutine != null)
            StopCoroutine(m_ActiveRoutine);
        m_ActiveRoutine = StartCoroutine(CollectRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        m_State = State.Bouncing;

        float horizontalDir = Random.value > 0.5f ? 1f : -1f;
        float horizontalSpeed = Random.Range(m_HorizontalSpeedRange.x, m_HorizontalSpeedRange.y) * horizontalDir;
        float verticalSpeed = m_InitialVerticalSpeed;

        Vector3 pos = transform.position;
        float startY = pos.y;

        // Arc upward then fall back to start height.
        while (true)
        {
            float dt = Time.deltaTime;
            verticalSpeed -= m_Gravity * dt;
            pos.x += horizontalSpeed * dt;
            pos.y += verticalSpeed * dt;
            transform.position = pos;

            if (pos.y <= startY && verticalSpeed < 0f)
            {
                pos.y = startY;
                transform.position = pos;
                break;
            }

            yield return null;
        }

        m_State = State.Resting;

        // Hold, then auto-fade if not collected.
        yield return new WaitForSeconds(m_HoldAfterLand);

        float elapsed = 0f;
        Color color = m_SpriteRenderer.color;
        while (elapsed < m_FadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsed / m_FadeDuration);
            m_SpriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator CollectRoutine()
    {
        // Disable the collider so it can't be clicked again mid-flight.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        // Restore full alpha in case the coin was mid-fade when clicked.
        Color color = m_SpriteRenderer.color;
        color.a = 1f;
        m_SpriteRenderer.color = color;

        float elapsed = 0f;
        while (elapsed < m_CollectDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / m_CollectDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic

            transform.position = Vector3.LerpUnclamped(startPos, GetCounterWorldPosition(), eased);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);

            yield return null;
        }

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCurrency(m_GoldValue);

        Destroy(gameObject);
    }

    // Converts the gold counter's screen-space rect position to a world-space position at z=0.
    private Vector3 GetCounterWorldPosition()
    {
        if (CurrencyUI.Instance == null || CurrencyUI.Instance.CurrencyTextRect == null)
        {
            // Fallback: top-right corner of the screen.
            return Camera.main.ScreenToWorldPoint(
                new Vector3(Screen.width, Screen.height, -Camera.main.transform.position.z));
        }

        // ScreenSpaceOverlay RectTransform.position is already in screen-pixel coordinates.
        Vector3 screenPos = CurrencyUI.Instance.CurrencyTextRect.position;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
        worldPos.z = 0f;
        return worldPos;
    }
}
