using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class AugmentInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform augmentInventoryContainer;
    [SerializeField] private Button augmentSlotPrefab; // Simple button prefab for each augment
    [SerializeField] private TextMeshProUGUI augmentCountText;
    
    [Header("Info Panel")]
    [SerializeField] private GameObject augmentInfoPanel;
    [SerializeField] private Button viewAllAugmentsButton;
    [SerializeField] private Button closeInfoPanelButton;
    [SerializeField] private Transform augmentInfoContainer;
    
    [Header("Augment Tooltip")]
    [SerializeField] private Transform augmentTooltipContainer;
    
    private AugmentManager augmentManager;
    private List<Button> augmentSlots = new List<Button>();
    private Augment currentSelectedAugment; // Track which augment is currently displayed
    
    private void Start()
    {
        augmentManager = FindFirstObjectByType<AugmentManager>();
        
        if (augmentManager == null)
        {
            Debug.LogError("[AugmentInventoryUI] AugmentManager not found in scene!");
            return;
        }

        // Initialize info panel
        if (augmentInfoPanel != null)
        {
            augmentInfoPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[AugmentInventoryUI] augmentInfoPanel NOT assigned in inspector!");
        }

        // Hook up view all augments button
        if (viewAllAugmentsButton != null)
        {
            viewAllAugmentsButton.onClick.AddListener(ShowAllAugmentInfo);
        }
        else
        {
            Debug.LogError("[AugmentInventoryUI] viewAllAugmentsButton NOT assigned in inspector!");
        }

        // Hook up close info panel button
        if (closeInfoPanelButton != null)
        {
            closeInfoPanelButton.onClick.AddListener(HideAugmentInfo);
        }
        else
        {
            Debug.LogError("[AugmentInventoryUI] closeInfoPanelButton NOT assigned in inspector!");
        }

        // Check critical references
        if (augmentInventoryContainer == null)
        {
            Debug.LogError("[AugmentInventoryUI] CRITICAL: augmentInventoryContainer NOT assigned in inspector!");
        }

        if (augmentSlotPrefab == null)
        {
            Debug.LogError("[AugmentInventoryUI] CRITICAL: augmentSlotPrefab NOT assigned in inspector!");
        }

        if (augmentCountText == null)
        {
            Debug.LogError("[AugmentInventoryUI] augmentCountText NOT assigned in inspector!");
        }

        if (augmentTooltipContainer == null)
        {
            Debug.LogError("[AugmentInventoryUI] augmentTooltipContainer NOT assigned in inspector!");
        }
        
        RefreshAugmentDisplay();
    }
    
    public void RefreshAugmentDisplay()
    {
        if (augmentManager == null)
        {
            Debug.LogError("[AugmentInventoryUI] AugmentManager not found!");
            return;
        }

        if (augmentInventoryContainer == null)
        {
            Debug.LogError("[AugmentInventoryUI] augmentInventoryContainer NOT assigned in inspector! Cannot display augments.");
            return;
        }

        if (augmentSlotPrefab == null)
        {
            Debug.LogError("[AugmentInventoryUI] augmentSlotPrefab NOT assigned in inspector! Cannot create augment buttons.");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in augmentInventoryContainer)
        {
            Destroy(child.gameObject);
        }
        augmentSlots.Clear();
        
        // Get owned augments from manager
        List<Augment> ownedAugments = augmentManager.GetAugmentInventory();
        
        if (ownedAugments.Count == 0)
        {
            return;
        }
        
        // Create button for each owned augment (show name only)
        for (int i = 0; i < ownedAugments.Count; i++)
        {
            Augment augment = ownedAugments[i];
            Button augmentButton = Instantiate(augmentSlotPrefab, augmentInventoryContainer);
            
            // Set button text to augment name only
            TextMeshProUGUI buttonText = augmentButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = augment.Name;
            }
            else
            {
                Debug.LogWarning($"[AugmentInventoryUI] augmentSlotPrefab has no TextMeshProUGUI child component!");
            }
            
            // Add click listener to display tooltip for this augment
            augmentButton.onClick.AddListener(() => ShowAugmentTooltip(augment));
            
            augmentButton.gameObject.SetActive(true);
            augmentSlots.Add(augmentButton);
        }
        
        // Update count display
        if (augmentCountText != null)
        {
            augmentCountText.text = $"Owned Augments: {ownedAugments.Count}";
        }
        else
        {
            Debug.LogWarning("[AugmentInventoryUI] augmentCountText NOT assigned in inspector!");
        }
    }
    
    /// <summary>
    /// Show detailed information for all owned augments with vertical columns of name and description
    /// </summary>
    private void ShowAllAugmentInfo()
    {
        if (augmentManager == null) return;

        if (augmentInfoPanel != null)
        {
            augmentInfoPanel.SetActive(true);
        }

        // Clear existing displays
        if (augmentInfoContainer != null)
        {
            foreach (Transform child in augmentInfoContainer)
            {
                Destroy(child.gameObject);
            }
        }

        List<Augment> ownedAugments = augmentManager.GetAugmentInventory();
        if (ownedAugments.Count == 0) return;

        // Create main horizontal container for all augment columns
        GameObject mainContainer = new GameObject("MainContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        mainContainer.transform.SetParent(augmentInfoContainer, false);
        RectTransform mainRT = mainContainer.GetComponent<RectTransform>();
        mainRT.anchorMin = Vector2.zero;
        mainRT.anchorMax = Vector2.one;
        mainRT.offsetMin = Vector2.zero;
        mainRT.offsetMax = Vector2.zero;

        HorizontalLayoutGroup mainHLG = mainContainer.GetComponent<HorizontalLayoutGroup>();
        mainHLG.spacing = 10;
        mainHLG.childForceExpandWidth = true;
        mainHLG.childForceExpandHeight = true;
        mainHLG.padding = new RectOffset(10, 10, 10, 10);

        // Create a column for each augment (vertical stack of name button + description)
        foreach (Augment augment in ownedAugments)
        {
            // Create vertical column container
            GameObject columnContainer = new GameObject($"Column_{augment.Name}", typeof(RectTransform), typeof(VerticalLayoutGroup));
            columnContainer.transform.SetParent(mainContainer.transform, false);

            VerticalLayoutGroup vlg = columnContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            // Create name button
            GameObject nameButton = new GameObject($"NameButton_{augment.Name}", typeof(RectTransform), typeof(Button), typeof(Image), typeof(LayoutElement));
            nameButton.transform.SetParent(columnContainer.transform, false);

            Image btnImage = nameButton.GetComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            LayoutElement nameBtnLayout = nameButton.GetComponent<LayoutElement>();
            nameBtnLayout.preferredHeight = 60;

            // Add text to button
            GameObject nameText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameText.transform.SetParent(nameButton.transform, false);
            TextMeshProUGUI nameTextComp = nameText.GetComponent<TextMeshProUGUI>();
            nameTextComp.text = augment.Name;
            nameTextComp.fontSize = 24;
            nameTextComp.alignment = TextAlignmentOptions.Center;

            RectTransform nameTextRT = nameText.GetComponent<RectTransform>();
            nameTextRT.anchorMin = Vector2.zero;
            nameTextRT.anchorMax = Vector2.one;
            nameTextRT.offsetMin = Vector2.zero;
            nameTextRT.offsetMax = Vector2.zero;

            // Create description area
            GameObject descContainer = new GameObject($"Description_{augment.Name}", typeof(RectTransform), typeof(Image));
            descContainer.transform.SetParent(columnContainer.transform, false);

            Image descImage = descContainer.GetComponent<Image>();
            descImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Add description text
            GameObject descText = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descText.transform.SetParent(descContainer.transform, false);
            TextMeshProUGUI descTextComp = descText.GetComponent<TextMeshProUGUI>();
            descTextComp.text = augment.Description;
            descTextComp.fontSize = 16;
            descTextComp.wordWrappingRatios = 0.3f;

            RectTransform descTextRT = descText.GetComponent<RectTransform>();
            descTextRT.anchorMin = Vector2.zero;
            descTextRT.anchorMax = Vector2.one;
            descTextRT.offsetMin = new Vector2(5, 5);
            descTextRT.offsetMax = new Vector2(-5, -5);
        }
    }

    /// <summary>
    /// Hide the augment information panel
    /// </summary>
    private void HideAugmentInfo()
    {
        if (augmentInfoPanel != null)
        {
            augmentInfoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Display tooltip for the selected augment. Replaces previous tooltip content.
    /// The tooltip persists on screen until another augment is selected.
    /// </summary>
    private void ShowAugmentTooltip(Augment augment)
    {
        if (augmentTooltipContainer == null)
        {
            Debug.LogError("[AugmentInventoryUI] augmentTooltipContainer NOT assigned in inspector!");
            return;
        }

        currentSelectedAugment = augment;

        // Clear existing tooltip content
        foreach (Transform child in augmentTooltipContainer)
        {
            Destroy(child.gameObject);
        }

        // Create name text
        GameObject nameObject = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObject.transform.SetParent(augmentTooltipContainer, false);
        TextMeshProUGUI nameText = nameObject.GetComponent<TextMeshProUGUI>();
        nameText.text = $"<b>{augment.Name}</b>";
        nameText.fontSize = 36;

        // Create description text
        GameObject descObject = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
        descObject.transform.SetParent(augmentTooltipContainer, false);
        TextMeshProUGUI descText = descObject.GetComponent<TextMeshProUGUI>();
        descText.text = augment.Description;
        descText.fontSize = 24;
        descText.wordWrappingRatios = 0.4f;

        // Layout setup - match container dimensions (467.88 x 231.6)
        RectTransform tooltipRT = augmentTooltipContainer.GetComponent<RectTransform>();
        if (tooltipRT != null)
        {
            tooltipRT.sizeDelta = new Vector2(467.88f, 231.6f);
        }

        // Position name at the top
        RectTransform nameRT = nameObject.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.5f, 1f);
        nameRT.anchorMax = new Vector2(0.5f, 1f);
        nameRT.pivot = new Vector2(0.5f, 1f);
        nameRT.anchoredPosition = new Vector2(0, 0);
        nameRT.sizeDelta = new Vector2(467.88f, 60);

        // Position description below name with adequate spacing
        RectTransform descRT = descObject.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0.5f, 1f);
        descRT.anchorMax = new Vector2(0.5f, 1f);
        descRT.pivot = new Vector2(0.5f, 1f);
        descRT.anchoredPosition = new Vector2(0, -70);
        descRT.sizeDelta = new Vector2(467.88f, 160);
    }
}