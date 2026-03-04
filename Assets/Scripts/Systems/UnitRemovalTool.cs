using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitRemovalTool : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager m_BoardManager;
    [SerializeField] private Button m_RemoveButton;
    [SerializeField] private Image m_CursorImage;

    private bool m_IsActive;
    private int m_IgnoreClickFrame = -1;

    private void Awake()
    {
        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
        }

        if (m_RemoveButton != null)
        {
            m_RemoveButton.onClick.AddListener(ToggleRemoveMode);
        }

        if (m_CursorImage != null)
        {
            m_CursorImage.raycastTarget = false;
        }

        SetActive(false);
    }

    private void OnDestroy()
    {
        if (m_RemoveButton != null)
        {
            m_RemoveButton.onClick.RemoveListener(ToggleRemoveMode);
        }
    }

    private void Update()
    {
        if (!m_IsActive)
        {
            return;
        }

        UpdateCursorPosition();

        if (WasLeftClickPressed())
        {
            if (Time.frameCount == m_IgnoreClickFrame)
            {
                return;
            }
            HandleClick();
        }
    }

    public void ToggleRemoveMode()
    {
        SetActive(!m_IsActive);
    }

    public void SetActive(bool isActive)
    {
        m_IsActive = isActive;

        if (m_IsActive)
        {
            m_IgnoreClickFrame = Time.frameCount;
        }

        if (m_CursorImage != null)
        {
            m_CursorImage.gameObject.SetActive(isActive);
        }
    }

    private void UpdateCursorPosition()
    {
        if (m_CursorImage == null)
        {
            return;
        }

        Vector2 screenPos = GetMouseScreenPosition();
        m_CursorImage.transform.position = screenPos;
    }

    private void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            SetActive(false);
            return;
        }

        if (m_BoardManager == null)
        {
            SetActive(false);
            return;
        }

        Vector3 worldPos = GetMouseWorldPosition();
        if (m_BoardManager.GameTilemap != null)
        {
            Vector3Int cellPos = m_BoardManager.GameTilemap.WorldToCell(worldPos);
            if (m_BoardManager.TryGetUnitAtCell(cellPos, out BaseUnit unitAtCell))
            {
                m_BoardManager.RemoveUnit(unitAtCell);
                SetActive(false);
                return;
            }
        }

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        for (int i = 0; i < hits.Length; i++)
        {
            BaseUnit unit = hits[i].GetComponentInParent<BaseUnit>();
            if (unit != null)
            {
                m_BoardManager.RemoveUnit(unit);
                SetActive(false);
                return;
            }
        }

        SetActive(false);
    }

    private static bool WasLeftClickPressed()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }

        return Input.GetMouseButtonDown(0);
    }

    private static Vector2 GetMouseScreenPosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Input.mousePosition;
    }

    private static Vector3 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;
        Vector3 screenPos = GetMouseScreenPosition();
        if (cam == null)
        {
            return Vector3.zero;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }
}
