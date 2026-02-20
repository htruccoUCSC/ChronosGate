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
    
    private AugmentManager augmentManager;
    private List<Button> augmentSlots = new List<Button>();
    
    private void Start()
    {
        augmentManager = FindObjectOfType<AugmentManager>();
        
        if (augmentManager == null)
        {
            Debug.LogError("AugmentManager not found!");
            return;
        }

        // Initialize info panel
        if (augmentInfoPanel != null)
        {
            augmentInfoPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[AugmentInventoryUI] augmentInfoPanel not assigned in inspector!");
        }

        // Hook up view all augments button
        if (viewAllAugmentsButton != null)
        {
            viewAllAugmentsButton.onClick.AddListener(ShowAllAugmentInfo);
        }
        else
        {
            Debug.LogWarning("[AugmentInventoryUI] viewAllAugmentsButton not assigned in inspector!");
        }

        // Hook up close info panel button
        if (closeInfoPanelButton != null)
        {
            closeInfoPanelButton.onClick.AddListener(HideAugmentInfo);
        }
        else
        {
            Debug.LogWarning("[AugmentInventoryUI] closeInfoPanelButton not assigned in inspector!");
        }
        
        RefreshAugmentDisplay();
    }
    
    public void RefreshAugmentDisplay()
    {
        if (augmentManager == null) return;

        // Clear existing buttons
        foreach (Transform child in augmentInventoryContainer)
        {
            Destroy(child.gameObject);
        }
        augmentSlots.Clear();
        
        // Get owned augments from manager
        List<Augment> ownedAugments = augmentManager.GetAugmentInventory();
        
        // Create button for each owned augment (show name only)
        foreach (Augment augment in ownedAugments)
        {
            Button augmentButton = Instantiate(augmentSlotPrefab, augmentInventoryContainer);
            
            // Set button text to augment name only
            TextMeshProUGUI buttonText = augmentButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = augment.Name;
            }
            
            augmentSlots.Add(augmentButton);
        }
        
        // Update count display
        if (augmentCountText != null)
        {
            augmentCountText.text = $"Owned Augments: {ownedAugments.Count}";
        }
    }
    
    /// <summary>
    /// Show detailed information for all owned augments
    /// </summary>
    private void ShowAllAugmentInfo()
    {
        if (augmentManager == null) return;

        if (augmentInfoPanel != null)
        {
            augmentInfoPanel.SetActive(true);
        }

        // Clear existing info displays
        if (augmentInfoContainer != null)
        {
            foreach (Transform child in augmentInfoContainer)
            {
                Destroy(child.gameObject);
            }
        }

        List<Augment> ownedAugments = augmentManager.GetAugmentInventory();

        // Display each augment's full details
        foreach (Augment augment in ownedAugments)
        {
            // Create a container for this augment's info
            GameObject infoDisplay = new GameObject($"AugmentInfo_{augment.Name}", typeof(RectTransform), typeof(LayoutElement));
            infoDisplay.transform.SetParent(augmentInfoContainer, false);

            // Add text components for name and description
            GameObject nameObject = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObject.transform.SetParent(infoDisplay.transform, false);
            TextMeshProUGUI nameText = nameObject.GetComponent<TextMeshProUGUI>();
            nameText.text = $"<b>{augment.Name}</b>";
            nameText.fontSize = 48;

            GameObject descObject = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObject.transform.SetParent(infoDisplay.transform, false);
            TextMeshProUGUI descText = descObject.GetComponent<TextMeshProUGUI>();
            descText.text = augment.Description;
            descText.fontSize = 32;
            descText.wordWrappingRatios = 0.4f;

            // Layout setup for better readability
            RectTransform infoRT = infoDisplay.GetComponent<RectTransform>();
            infoRT.sizeDelta = new Vector2(300, 200);

            RectTransform nameRT = nameObject.GetComponent<RectTransform>();
            nameRT.anchoredPosition = Vector2.zero;
            nameRT.sizeDelta = new Vector2(300, 60);

            RectTransform descRT = descObject.GetComponent<RectTransform>();
            descRT.anchoredPosition = new Vector2(0, -80);
            descRT.sizeDelta = new Vector2(300, 120);
        }

        Debug.Log($"[AugmentInventoryUI] Displaying info for {ownedAugments.Count} augments");
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
}