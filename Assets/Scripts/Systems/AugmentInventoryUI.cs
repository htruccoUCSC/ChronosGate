using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class AugmentInventoryUI : MonoBehaviour
{
    [SerializeField] private Transform augmentInventoryContainer;
    [SerializeField] private Button augmentInventoryPrefab; // Simple button prefab for each augment
    [SerializeField] private TextMeshProUGUI augmentCountText;
    
    private AugmentManager augmentManager;
    private List<Button> augmentButtons = new List<Button>();
    
    private void Start()
    {
        augmentManager = FindObjectOfType<AugmentManager>();
        
        if (augmentManager == null)
        {
            Debug.LogError("AugmentManager not found!");
            return;
        }
        
        RefreshAugmentDisplay();
    }
    
    public void RefreshAugmentDisplay()
    {
        // Clear existing buttons
        foreach (Transform child in augmentInventoryContainer)
        {
            Destroy(child.gameObject);
        }
        augmentButtons.Clear();
        
        // Create button for each owned augment
        foreach (Augment augment in augmentManager.GetAugmentInventory())
        {
            Button augmentButton = Instantiate(augmentInventoryPrefab, augmentInventoryContainer);
            
            TextMeshProUGUI buttonText = augmentButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = augment.Name;
            }
            
            // Add tooltip/description on hover
            augmentButton.onClick.AddListener(() => OnAugmentSelected(augment));
            augmentButtons.Add(augmentButton);
        }
        
        // Update count display
        if (augmentCountText != null)
        {
            augmentCountText.text = $"Augments: {augmentManager.GetAugmentInventory().Count}";
        }
    }
    
    private void OnAugmentSelected(Augment augment)
    {
        Debug.Log($"Selected augment: {augment.Name}");
        // Optional: Show augment details, equip/unequip options
    }
}