using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Pretty much copy-paste from Unit InventoryUI but adapted for items 
[RequireComponent(typeof(RectTransform))]
public class ItemInventoryUI : MonoBehaviour
{
    private const string StartingTowerPoolItemPath = "TowerPoolRestockItemDefinition";

    [SerializeField] private ItemSpawner m_ItemSpawner;
    [SerializeField] private int m_Capacity = 4;       //number of inventory slots
    [SerializeField] private Sprite m_SlotBackground;   //unfilled slot background
    [SerializeField] private Sprite m_SlotFilledBackground; //filled slot background
    [SerializeField] private Vector2 m_CellSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 m_Spacing = new Vector2(8f, 8f);

    private ItemDefinition[] m_Items;               //array of item definitions in inventory
    private readonly List<Image> m_Icons = new List<Image>();

    private void Start()
    {
        m_Items = new ItemDefinition[m_Capacity];
        Build();
        AddStartingTowerPoolItem();
    }

    // Build the inventory UI
    public void Build()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        m_Icons.Clear();

        var grid = gameObject.AddComponent<GridLayoutGroup>();

        grid.startAxis = GridLayoutGroup.Axis.Vertical;
        grid.cellSize = m_CellSize;
        grid.spacing = m_Spacing;
        grid.childAlignment = TextAnchor.LowerCenter;

        for (int i = 0; i < m_Capacity; i++)
        {
            var slotObject = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotObject.transform.SetParent(transform, false);

            var bg = slotObject.AddComponent<Image>();
            if (m_SlotBackground && m_Items[i] == null) bg.sprite = m_SlotBackground;
            if (m_SlotFilledBackground && m_Items[i] != null) bg.sprite = m_SlotFilledBackground;

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

            var drag = slotObject.AddComponent<ItemInventorySlotDrag>();
            drag.Initialize(this, i);
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < m_Capacity; i++)
        {
            if (m_Items[i] != null)
            {
                // get item icon from slot
                m_Icons[i].sprite = m_Items[i].Icon;
                m_Icons[i].enabled = true;
            }
            else
            {
                m_Icons[i].sprite = null;
                m_Icons[i].enabled = false;
            }
        }
    }

    public bool AddItem(ItemDefinition item)
    {
        if (item == null) return false;

        for (int i = 0; i < m_Items.Length; i++)
        {
            if (m_Items[i] == null)
            {
                m_Items[i] = item;
                Refresh();
                return true;
            }
        }
        return false;
    }

    // gets unit definition at index
    public ItemDefinition GetItem(int index)
    {
        if (index < 0 || index >= m_Items.Length) return null;
        return m_Items[index];
    }

    public void SetItem(int index, ItemDefinition item)
    {
        if (index >= 0 && index < m_Items.Length)
        {
            m_Items[index] = item;
            Refresh();
        }
    }

    public void SwapItems(int indexA, int indexB)
    {
        var temp = m_Items[indexA];
        m_Items[indexA] = m_Items[indexB];
        m_Items[indexB] = temp;
        Refresh();
    }

    public bool TryUseItemFromSlot(int index)
    {
        if (m_ItemSpawner == null) return false;

        ItemDefinition item = GetItem(index);
        if (item == null) return false;

        if (m_ItemSpawner.TryPlaceFromInventory(item))
        {
            SetItem(index, null);
            m_ItemSpawner.SetPreviewItem(null);
            return true;
        }

        return false;
    }

    public bool TryUseItemFromSlotAtPosition(int index, Vector2 screenPosition)
    {
        if (m_ItemSpawner == null) return false;

        ItemDefinition item = GetItem(index);
        if (item == null) return false;

        if (m_ItemSpawner.TryPlaceAtScreenPosition(item, screenPosition))
        {
            SetItem(index, null);
            m_ItemSpawner.SetPreviewItem(null);
            return true;
        }

        return false;
    }

    public void SetPreviewItem(ItemDefinition item)
    {
        if (m_ItemSpawner == null) return;
        m_ItemSpawner.SetPreviewItem(item);
    }

    private void AddStartingTowerPoolItem()
    {
        ItemDefinition startingItem = Resources.Load<ItemDefinition>(StartingTowerPoolItemPath);
        if (startingItem == null)
        {
            return;
        }

        AddItem(startingItem);
    }
}

// Drag and Drop behavior for inventory slots
public class ItemInventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private ItemInventoryUI m_InventoryUI;
    private int m_Index;
    private GameObject m_DragObject;
    private CanvasGroup m_CanvasGroup; // Used to let raycasts pass through the icon

    public void Initialize(ItemInventoryUI inventoryUI, int index)
    {
        m_InventoryUI = inventoryUI;
        m_Index = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var item = m_InventoryUI.GetItem(m_Index);
        if (item == null) return;

        m_InventoryUI.SetPreviewItem(item);

        // Create Drag Visual
        var canvas = m_InventoryUI.GetComponentInParent<Canvas>();
        m_DragObject = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        m_DragObject.transform.SetParent(canvas.transform, false);

        var img = m_DragObject.GetComponent<Image>();
        img.sprite = item.Icon;
        img.preserveAspect = true;
        img.raycastTarget = false; // IMPORTANT: Ignore raycasts so we can drop "through" it

        // Make it slightly transparent
        m_CanvasGroup = m_DragObject.GetComponent<CanvasGroup>();
        //reduce scale
        m_CanvasGroup.transform.localScale = new Vector3(0.3f, 0.3f);
        m_CanvasGroup.alpha = 0.7f;
        m_CanvasGroup.blocksRaycasts = false;

        UpdatePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_DragObject != null) UpdatePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_DragObject != null) Destroy(m_DragObject);

        m_InventoryUI.SetPreviewItem(null);

        GameObject hoveredObject = eventData.pointerCurrentRaycast.gameObject;
        if (hoveredObject != null)
        {
            bool isUiTarget = hoveredObject.GetComponent<RectTransform>() != null;
            if (isUiTarget)
            {
                return;
            }
        }

        m_InventoryUI.TryUseItemFromSlotAtPosition(m_Index, eventData.position);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // This handles Slot-to-Slot swapping
        var source = eventData.pointerDrag?.GetComponent<ItemInventorySlotDrag>();
        if (source != null && source.m_InventoryUI == m_InventoryUI)
        {
            m_InventoryUI.SwapItems(source.m_Index, m_Index);
        }
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        m_DragObject.transform.position = eventData.position;
    }
}
