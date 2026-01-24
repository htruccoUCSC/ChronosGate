using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private int m_Capacity = 10;
    [SerializeField] private Sprite[] m_Items;
    [SerializeField] private Sprite m_SlotBackground;
    [SerializeField] private Vector2 m_CellSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 m_Spacing = new Vector2(8f, 8f);
    [SerializeField] private bool m_BuildOnStart = true;

    private readonly List<Image> m_Icons = new List<Image>();

    public int Capacity => m_Capacity;
    public Sprite[] Items => m_Items;

    private void Start()
    {
        if (m_BuildOnStart)
        {
            Build();
        }

        Refresh();
    }

    public bool AddItem(Sprite sprite)
    {
        if (sprite == null)
        {
            return false;
        }

        for (int i = 0; i < m_Items.Length; i++)
        {
            if (m_Items[i] == null)
            {
                m_Items[i] = sprite;
                Refresh();
                return true;
            }
        }

        return false;
    }

  
    
    public void Build()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        m_Icons.Clear();

        var grid = GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = m_CellSize;
        grid.spacing = m_Spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
        grid.childAlignment = TextAnchor.LowerCenter;

        for (int i = 0; i < m_Capacity; i++)
        {
            var slotObject = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotObject.transform.SetParent(transform, false);

            var background = slotObject.AddComponent<Image>();
            if (m_SlotBackground != null)
            {
                background.sprite = m_SlotBackground;
                background.type = Image.Type.Sliced;
            }

            var iconObject = new GameObject("Icon", typeof(RectTransform));
            iconObject.transform.SetParent(slotObject.transform, false);
            var icon = iconObject.AddComponent<Image>();
            icon.preserveAspect = true;

            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            m_Icons.Add(icon);
        }
    }

// if the number of items does not match capacity, rebuild the UI
    public void Refresh()
    {
        if (m_Icons.Count != m_Capacity)
        {
            Build();
        }

        for (int i = 0; i < m_Capacity; i++)
        {
            m_Icons[i].sprite = m_Items[i];
            m_Icons[i].enabled = m_Items[i] != null;
        }
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < m_Items.Length;
    }


//Insert and Remove methods
  public bool SetItem(int index, Sprite sprite)
    {
        if (!IsValidIndex(index))
        {
            return false;
        }

        m_Items[index] = sprite;
        Refresh();
        return true;
    }

    public bool RemoveAt(int index)
    {
        if (!IsValidIndex(index) || m_Items[index] == null)
        {
            return false;
        }

        m_Items[index] = null;
        Refresh();
        return true;
    }


}
