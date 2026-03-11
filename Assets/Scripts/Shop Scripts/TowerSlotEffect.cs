using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerCardJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Rotation Settings")]
    public float maxRotationAngle = 15f;
    public float lerpSpeed = 10f;

    [Header("Scale Settings")]
    public float hoverScale = 1.2f;
    public float scaleSpeed = 12f;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Vector3 targetScale;
    private bool isHovered = false;

    private Vector3 targetRotation;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        targetScale = Vector3.one;
    }

    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, deltaTime * scaleSpeed);

        if (isHovered)
        {
            CalculateTilt();
        }
        else
        {
            targetRotation = Vector3.zero;
        }

        Quaternion targetQuaternion = Quaternion.Euler(targetRotation.x, targetRotation.y, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetQuaternion, deltaTime * lerpSpeed);
    }

    private void CalculateTilt()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, parentCanvas.worldCamera, out localPoint))
        {
            float xFactor = (localPoint.x / (rectTransform.rect.width / 2f));
            float yFactor = (localPoint.y / (rectTransform.rect.height / 2f));

            xFactor = Mathf.Clamp(xFactor, -1f, 1f);
            yFactor = Mathf.Clamp(yFactor, -1f, 1f);

            targetRotation = new Vector3(-yFactor * maxRotationAngle, xFactor * maxRotationAngle, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = Vector3.one * hoverScale;

        if (TryGetComponent<Canvas>(out Canvas cv))
        {
            cv.overrideSorting = true;
            cv.sortingOrder = 10000;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = Vector3.one;

        if (TryGetComponent<Canvas>(out Canvas cv))
        {
            cv.sortingOrder = 1010;
        }
    }
}
