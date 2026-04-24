using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private int m_Capacity = 10;       //number of inventory slots
    [SerializeField] private Sprite m_SlotBackground;   //unfilled slot background
    [SerializeField] private Sprite m_SlotFilledBackground; //filled slot background
    [SerializeField] private Vector2 m_CellSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 m_Spacing = new Vector2(8f, 8f);

    private UnitDefinition[] m_Units;               //array of unit sprites in inventory
    private readonly List<Image> m_Icons = new List<Image>();

    public UnitSpawner unitSpawner; // Reference to UnitSpawner to spawn units from inventory

    private void Start()
    {
        EnsureStorageInitialized();
        Build();
    }

    private void EnsureStorageInitialized()
    {
        if (m_Units == null || m_Units.Length != m_Capacity)
        {
            m_Units = new UnitDefinition[m_Capacity];
        }
    }

    // Build the inventory UI
    public void Build()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        m_Icons.Clear();

        var grid = gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = m_CellSize;
        grid.spacing = m_Spacing;
        grid.childAlignment = TextAnchor.LowerCenter;

        for (int i = 0; i < m_Capacity; i++)
        {
            var slotObject = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotObject.transform.SetParent(transform, false);

            var bg = slotObject.AddComponent<Image>();
            
            if (m_SlotBackground && m_Units[i] == null) bg.sprite = m_SlotBackground;

            if (m_SlotFilledBackground && m_Units[i] != null) bg.sprite = m_SlotFilledBackground;


            var iconObject = new GameObject("Icon", typeof(RectTransform));
            iconObject.transform.SetParent(slotObject.transform, false);
            var icon = iconObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.enabled = false; // Hide by default

            // Stretch icon
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            m_Icons.Add(icon);

            var drag = slotObject.AddComponent<InventorySlotDrag>();
            drag.Initialize(this, i);
        }
    }

    public void Refresh()
    {
        EnsureStorageInitialized();

        if (m_Icons.Count < m_Capacity)
        {
            return;
        }

        for (int i = 0; i < m_Capacity; i++)
        {
            if (m_Units[i] != null)
            {
                // get unit icon from definition
                m_Icons[i].sprite = m_Units[i].Icon;
                m_Icons[i].enabled = true;
            }
            else
            {
                m_Icons[i].sprite = null;
                m_Icons[i].enabled = false;
            }
        }
    }

    public bool AddUnit(UnitDefinition def)
    {
        EnsureStorageInitialized();

        for (int i = 0; i < m_Units.Length; i++)
        {
            if (m_Units[i] == null)
            {
                m_Units[i] = def;
                Refresh();
                return true;
            }
        }
        return false;
    }

    // gets unit definition at index
    public UnitDefinition GetUnit(int index)
    {
        EnsureStorageInitialized();
        if (index < 0 || index >= m_Units.Length) return null;
        return m_Units[index];
    }

    public void SetUnit(int index, UnitDefinition unit)
    {
        EnsureStorageInitialized();
        if (index >= 0 && index < m_Units.Length)
        {
            m_Units[index] = unit;
            Refresh();
        }
    }

    public void SwapUnits(int indexA, int indexB)
    {
        EnsureStorageInitialized();
        var temp = m_Units[indexA];
        m_Units[indexA] = m_Units[indexB];
        m_Units[indexB] = temp;
        Refresh();
    }
}

// Drag and Drop behavior for inventory slots
public class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventoryUI m_InventoryUI;
    private int m_Index;
    private GameObject m_DragObject;
    private CanvasGroup m_CanvasGroup;
    private UnitRangePreview m_RangePreview;
    private BaseUnit m_PreviewProvider;
    private UnitDefinition m_DragUnit;
    private BoardManager m_Board;

    public void Initialize(InventoryUI inventoryUI, int index)
    {
        m_InventoryUI = inventoryUI;
        m_Index = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var unit = m_InventoryUI.GetUnit(m_Index);
        if (unit == null) return;

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayTowerPickup();
        }

        m_DragUnit = unit;
        m_Board = m_InventoryUI.unitSpawner != null ? m_InventoryUI.unitSpawner.board : null;
        if (m_Board != null)
        {
            m_RangePreview = UnitRangePreview.GetOrCreate(m_Board);
            m_PreviewProvider = UnitDragPreviewUtility.GetPreviewProvider(unit);
            UpdateRangePreview(eventData);
        }

        // Create Drag Visual
        var canvas = m_InventoryUI.GetComponentInParent<Canvas>();
        m_DragObject = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        m_DragObject.transform.SetParent(canvas.transform, false);

        var img = m_DragObject.GetComponent<Image>();
        img.sprite = unit.Icon;
        img.preserveAspect = true;
        img.raycastTarget = false; // IMPORTANT: Ignore raycasts so we can drop "through" it

        // Make it slightly transparent
        m_CanvasGroup = m_DragObject.GetComponent<CanvasGroup>();
        m_CanvasGroup.alpha = 0.7f;
        m_CanvasGroup.blocksRaycasts = false;

        UpdatePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_DragObject != null) UpdatePosition(eventData);
        UpdateRangePreview(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_DragObject != null) Destroy(m_DragObject);

        ClearRangePreview();

        GameObject hoveredObject = eventData.pointerCurrentRaycast.gameObject;
        if (hoveredObject != null)
        {
            bool isUiTarget = hoveredObject.GetComponent<RectTransform>() != null;
            if (isUiTarget)
            {
                return;
            }
        }

        // Get the Definition from the slot
        UnitDefinition defToSpawn = m_InventoryUI.GetUnit(m_Index);

        if (defToSpawn != null)
        {
            // Pass Definition to Spawner
            bool success = m_InventoryUI.unitSpawner.TrySpawnAtScreenPosition(defToSpawn, eventData.position);

            if (success)
            {
                // Remove the unit from the inventory
                m_InventoryUI.SetUnit(m_Index, null);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // This handles Slot-to-Slot swapping
        var source = eventData.pointerDrag?.GetComponent<InventorySlotDrag>();
        if (source != null && source.m_InventoryUI == m_InventoryUI)
        {
            m_InventoryUI.SwapUnits(source.m_Index, m_Index);
        }
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        if (m_DragObject == null) return;

        Canvas canvas = m_InventoryUI.GetComponentInParent<Canvas>();
        RectTransform dragRect = m_DragObject.GetComponent<RectTransform>();

        if (canvas == null || dragRect == null) return;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            dragRect.position = eventData.position;
            return;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null) return;

        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventCamera,
            out localPoint))
        {
            dragRect.localPosition = localPoint;
        }
    }

    private void UpdateRangePreview(PointerEventData eventData)
    {
        if (m_RangePreview == null || m_Board == null || m_DragUnit == null)
        {
            return;
        }

        if (Camera.main == null || m_Board.GameTilemap == null)
        {
            m_RangePreview.ClearPreview();
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;
        Vector3Int cellPos = m_Board.GameTilemap.WorldToCell(worldPos);

        if (!m_Board.GameTilemap.HasTile(cellPos))
        {
            m_RangePreview.ClearPreview();
            return;
        }

        if (m_PreviewProvider == null)
        {
            m_PreviewProvider = UnitDragPreviewUtility.GetPreviewProvider(m_DragUnit);
            if (m_PreviewProvider == null)
            {
                m_RangePreview.ClearPreview();
                return;
            }
        }

        m_RangePreview.ShowPreview(m_PreviewProvider, cellPos, m_DragUnit);
    }

    private void ClearRangePreview()
    {
        if (m_RangePreview != null)
        {
            m_RangePreview.ClearPreview();
        }

        m_PreviewProvider = null;
        m_RangePreview = null;
        m_Board = null;
        m_DragUnit = null;
    }
}
