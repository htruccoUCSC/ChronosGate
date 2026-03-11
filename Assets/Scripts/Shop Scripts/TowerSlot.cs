using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI eraText;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;
    [SerializeField] private bool active = false;

    private UnitDefinition unitDefinition;
    private InventoryUI inventoryUI;
    private ShopManager shopManager;
    private ShopManagerOld shopManagerOld;
    private GameObject m_DragObject;
    private BoardManager m_Board;
    private UnitRangePreview m_RangePreview;
    private BaseUnit m_PreviewProvider;

    private void Awake()
    {
        // Only proceed if this is actually a TowerSlot with required components
        if (button == null || eraText == null)
        {
            Debug.LogWarning($"[TowerSlot] {gameObject.name} is missing required components. Skipping tooltip setup.");
            return;
        }
        
        button.onClick.RemoveListener(OnSlotClicked);
        button.onClick.AddListener(OnSlotClicked);
    }

    public void Initialize(InventoryUI inventory)
    {
        inventoryUI = inventory;
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (shopManagerOld == null)
        {
            shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        }
    }

    public void Setup(UnitDefinition data)
    {
        unitDefinition = data;

        if (data != null)
        {
            bool useInlineDescription = shopManagerOld != null && shopManager == null;

            if (eraText != null) eraText.text = data.Faction;
            if (towerNameText != null) towerNameText.text = data.Name;
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.color = iconImage.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
            if (costText != null) costText.text = $"{data.Cost}";
            if (descriptionText != null) descriptionText.text = useInlineDescription ? data.Description : "";
            if (button != null) button.interactable = true;
            active = true;

            // Set faction color on background image
            if (backgroundImage != null)
            {
                backgroundImage.color = GetFactionColor(data.Faction);
            }

            Debug.Log($"Tower slot setup complete for: {data.Name}");
        }
        else
        {
            Debug.LogWarning($"UnitDefinition is null for {gameObject.name}");

            ClearSlot();
        }
    }

    private void ClearSlot()
    {
        if (eraText != null) eraText.text = "";
        if (towerNameText != null) towerNameText.text = "Empty";
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
        }
        if (costText != null) costText.text = "";
        if (descriptionText != null) descriptionText.text = "";
        if (button != null) button.interactable = false;
        active = false;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
    }

    private void OnSlotClicked()
    {
        if (!CanInteract())
        {
            return;
        }

        if (TryPurchaseToInventory())
        {
            ClearSlot();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanInteract())
        {
            return;
        }

        if (inventoryUI == null || inventoryUI.unitSpawner == null)
        {
            Debug.LogWarning("[TowerSlot] UnitSpawner not assigned. Cannot drag purchase to board.");
            return;
        }

        m_Board = inventoryUI.unitSpawner.board;
        if (m_Board != null)
        {
            m_RangePreview = UnitRangePreview.GetOrCreate(m_Board);
            m_PreviewProvider = UnitDragPreviewUtility.GetPreviewProvider(unitDefinition);
            UpdateRangePreview(eventData);
        }

        Canvas canvas = inventoryUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        m_DragObject = new GameObject("TowerDragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        m_DragObject.transform.SetParent(canvas.transform, false);

        Image image = m_DragObject.GetComponent<Image>();
        image.sprite = unitDefinition.Icon;
        image.preserveAspect = true;
        image.raycastTarget = false;

        CanvasGroup dragCanvasGroup = m_DragObject.GetComponent<CanvasGroup>();
        dragCanvasGroup.alpha = 0.7f;
        dragCanvasGroup.blocksRaycasts = false;

        UpdatePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_DragObject != null)
        {
            UpdatePosition(eventData);
        }

        UpdateRangePreview(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_DragObject != null)
        {
            Destroy(m_DragObject);
        }

        ClearRangePreview();

        if (!CanInteract())
        {
            return;
        }

        if (IsPointerOverUi(eventData))
        {
            return;
        }

        if (inventoryUI == null || inventoryUI.unitSpawner == null)
        {
            return;
        }

        if (TryPurchaseToBoard(eventData.position))
        {
            ClearSlot();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitDefinition == null)
        {
            return;
        }

        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (shopManagerOld == null)
        {
            shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        }

        if (shopManager != null)
        {
            shopManager.ShowConsumableTooltip(unitDefinition.Description);
            return;
        }

        if (shopManagerOld != null)
        {
            if (shopManagerOld.UsesTooltipOverlay())
            {
                shopManagerOld.ShowConsumableTooltip(unitDefinition.Description);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (shopManagerOld == null)
        {
            shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        }

        if (shopManager != null)
        {
            shopManager.HideConsumableTooltip();
            return;
        }

        if (shopManagerOld != null)
        {
            if (shopManagerOld.UsesTooltipOverlay())
            {
                shopManagerOld.HideConsumableTooltip();
            }
        }
    }

    private bool CanInteract()
    {
        if (!active || unitDefinition == null)
        {
            return false;
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("InventoryUI not assigned for tower slot.");
            return false;
        }

        return true;
    }

    private bool TryPurchaseToInventory()
    {
        if (!TrySpendCurrency(out CurrencyManager currencyManager))
        {
            return false;
        }

        if (inventoryUI.AddUnit(unitDefinition))
        {
            Debug.Log($"Purchased {unitDefinition.Name} for {unitDefinition.Cost} gold!");
            return true;
        }

        Debug.Log("Inventory is full.");
        currencyManager.AddCurrency(unitDefinition.Cost);
        return false;
    }

    private bool TryPurchaseToBoard(Vector2 screenPosition)
    {
        if (!TrySpendCurrency(out CurrencyManager currencyManager))
        {
            return false;
        }

        bool spawned = inventoryUI.unitSpawner.TrySpawnAtScreenPosition(unitDefinition, screenPosition);
        if (spawned)
        {
            Debug.Log($"Purchased {unitDefinition.Name} for {unitDefinition.Cost} gold!");
            return true;
        }

        currencyManager.AddCurrency(unitDefinition.Cost);
        return false;
    }

    private bool TrySpendCurrency(out CurrencyManager currencyManager)
    {
        currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager not found!");
            return false;
        }

        if (currencyManager.TrySpendCurrency(unitDefinition.Cost))
        {
            return true;
        }

        Debug.Log($"Cannot afford unit! Need {unitDefinition.Cost}, have {currencyManager.GetCurrency()}");
        return false;
    }

    private bool IsPointerOverUi(PointerEventData eventData)
    {
        for (int i = 0; i < eventData.hovered.Count; i++)
        {
            GameObject hoveredObject = eventData.hovered[i];
            if (hoveredObject != null && hoveredObject.GetComponentInParent<Canvas>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        if (m_DragObject == null)
        {
            return;
        }

        Canvas canvas = inventoryUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        RectTransform dragRect = m_DragObject.GetComponent<RectTransform>();
        if (canvasRect == null || dragRect == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, canvas.worldCamera, out Vector2 localPoint))
        {
            dragRect.localPosition = localPoint;
        }
    }

    private void UpdateRangePreview(PointerEventData eventData)
    {
        if (m_RangePreview == null || m_Board == null || unitDefinition == null)
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
            m_PreviewProvider = UnitDragPreviewUtility.GetPreviewProvider(unitDefinition);
            if (m_PreviewProvider == null)
            {
                m_RangePreview.ClearPreview();
                return;
            }
        }

        m_RangePreview.ShowPreview(m_PreviewProvider, cellPos, unitDefinition);
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
    }

    private Color GetFactionColor(string faction)
    {
        switch (faction)
        {
            case "Prehistoric":
                return new Color(0.6f, 0.4f, 0.2f); // Brown
            case "Fantasy":
                return new Color(0.5f, 0.3f, 0.7f); // Purple
            case "Medieval":
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case "Mystic":
                return new Color(0.2f, 0.6f, 0.8f); // Light Blue
            case "Modern":
                return new Color(0.3f, 0.3f, 0.3f); // Dark Gray
            case "Future":
                return new Color(0.0f, 0.8f, 0.4f); // Green
            case "Cosmic":
                return new Color(0.5f, 0.0f, 0.8f); // Deep Purple
            default:
                return Color.white;
        }
    }
}
