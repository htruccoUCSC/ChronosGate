using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private int m_Capacity = 10;       //number of inventory slots
    [SerializeField] private Sprite[] m_Units;          //array of unit sprites
    [SerializeField] private Sprite m_SlotBackground;   //slot background
    [SerializeField] private Vector2 m_CellSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 m_Spacing = new Vector2(8f, 8f);   
    [SerializeField] private bool m_BuildOnStart = true;

    private readonly List<Image> m_Icons = new List<Image>();

    public int Capacity => m_Capacity;
    public Sprite[] Units => m_Units;

    private void Start()
    {
        EnsureUnitArray();
        if (m_BuildOnStart)
        {
            Build();
        }

        Refresh();
    }

    //Build the inventory UI
    public void Build()
    {
        //wipe existing UI
        foreach (Transform child in transform)
        {Destroy(child.gameObject);}

        m_Icons.Clear();
        
        var grid = GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = gameObject.AddComponent<GridLayoutGroup>();
        }

        //Layout settings
        grid.cellSize = m_CellSize;
        grid.spacing = m_Spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
        grid.childAlignment = TextAnchor.LowerCenter;

        for (int i = 0; i < m_Capacity; i++)
        {   //create slot objects
            var slotObject = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotObject.transform.SetParent(transform, false);
            //background for each slot
            var background = slotObject.AddComponent<Image>();
            if (m_SlotBackground != null)
            {
                background.sprite = m_SlotBackground;
                background.type = Image.Type.Sliced;
            }
            //icon for each slot
            var iconObject = new GameObject("Icon", typeof(RectTransform));
            iconObject.transform.SetParent(slotObject.transform, false);
            var icon = iconObject.AddComponent<Image>();
            icon.preserveAspect = true;

            //stretch icon to fill slot
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            m_Icons.Add(icon);

            // Drag/drop behavior per slot for swapping units
            var drag = slotObject.AddComponent<InventorySlotDrag>();
            drag.Initialize(this, i);
        }
    }

    public void Refresh()
    {   //update UI to match unit array
        EnsureUnitArray();
        if (m_Icons.Count != m_Capacity)
        {
            Build();
        }

        for (int i = 0; i < m_Capacity; i++)
        {
            m_Icons[i].sprite = m_Units[i];
            m_Icons[i].enabled = m_Units[i] != null;
        }
    }

    //Validations
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < m_Units.Length;
    }

    private void EnsureUnitArray()
    {
        if (m_Units == null || m_Units.Length != m_Capacity)
        {
            System.Array.Resize(ref m_Units, m_Capacity);
        }
    }
    //Insert and Remove methods
    public bool SetUnit(int index, Sprite sprite)
    {
        if (!IsValidIndex(index))
        {return false;}

        m_Units[index] = sprite;
        Refresh();
        return true;
    }

    // Add a unit to the first available slot in the inventory: not used in the current implementation (Will be used for shop interaction)
    public bool AddUnit(Sprite sprite)
    {
        if (sprite == null)
        {return false;}

        EnsureUnitArray();

        for (int i = 0; i < m_Units.Length; i++)
        {
            if (m_Units[i] == null)
            {
                m_Units[i] = sprite;
                Refresh();
                return true;
            }
        }

        return false;
    }

    public bool RemoveUnitAt(int index)
    {
        if (!IsValidIndex(index) || m_Units[index] == null)
        {
            return false;
        }

        m_Units[index] = null;
        Refresh();
        return true;
    }

    public void SwapUnits(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB)
        {
            return;
        }

        var temp = m_Units[indexA];
        m_Units[indexA] = m_Units[indexB];
        m_Units[indexB] = temp;
        Refresh();
    }

    public Sprite GetUnit(int index)
    {
        if (!IsValidIndex(index))
        {return null;}

        return m_Units[index];
    }
}

// Drag and Drop behavior for inventory slots
public class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventoryUI m_InventoryUI;
    private int m_Index;
    private RectTransform m_DragIconRect;
    private Image m_DragIconImage;

    public void Initialize(InventoryUI inventoryUI, int index)
    {
        m_InventoryUI = inventoryUI;
        m_Index = index;
    }

    // Begin drag: create a floating icon that follows the cursor.
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Get the sprite being dragged (index in inventory and sprite)
        var sprite = m_InventoryUI.GetUnit(m_Index);
        if (sprite == null){return;}
        var canvas = m_InventoryUI.GetComponentInParent<Canvas>();
        if (canvas == null){return;}

        //Temporary drag icon
        var dragObject = new GameObject("DragIcon", typeof(RectTransform));
        dragObject.transform.SetParent(canvas.transform, false);
        m_DragIconRect = dragObject.GetComponent<RectTransform>();
        m_DragIconImage = dragObject.AddComponent<Image>();
        m_DragIconImage.raycastTarget = false;
        m_DragIconImage.sprite = sprite;
        m_DragIconImage.preserveAspect = true;

        m_DragIconRect.sizeDelta = ((RectTransform)transform).rect.size;
        UpdateDragIconPosition(eventData);
    }

    // While dragging: call update to keep the floating icon under the cursor
    public void OnDrag(PointerEventData eventData)
    {
        if (m_DragIconRect == null){return;}
        UpdateDragIconPosition(eventData);
    }

    // End drag: get rid of the temp icon
    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_DragIconRect != null)
        {
            Destroy(m_DragIconRect.gameObject);
            m_DragIconRect = null;
            m_DragIconImage = null;
        }
    }

    // Drop on a slot: swap units between source and target slots.
    public void OnDrop(PointerEventData eventData)
    {
        var source = eventData.pointerDrag?.GetComponent<InventorySlotDrag>();
        if (source == null || source.m_InventoryUI != m_InventoryUI)
        {return;}

        // Swap units between source and target slots
        m_InventoryUI.SwapUnits(source.m_Index, m_Index);
    }

    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (m_DragIconRect == null)
        {return;}
        // While dragging: keep temp icon under cursor
        m_DragIconRect.position = eventData.position;
    }
}
