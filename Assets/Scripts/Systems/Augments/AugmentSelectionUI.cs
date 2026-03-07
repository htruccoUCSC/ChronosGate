using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class AugmentSelectionUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject augmentSelectionPanel;
    
    [Header("Augment Cards")]
    [SerializeField] private Button augment1SelectButton;
    [SerializeField] private Button augment2SelectButton;
    [SerializeField] private Button augment3SelectButton;
    
    [Header("Placeholder Text Elements")]
    [SerializeField] private TextMeshProUGUI augment1Name;
    [SerializeField] private TextMeshProUGUI augment1Description;
    [SerializeField] private TextMeshProUGUI augment2Name;
    [SerializeField] private TextMeshProUGUI augment2Description;
    [SerializeField] private TextMeshProUGUI augment3Name;
    [SerializeField] private TextMeshProUGUI augment3Description;
    
    [Header("Control Buttons")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private Button toggleButton; // Should NOT be inside augmentSelectionPanel

    private AugmentManager augmentManager;
    private List<Augment> selectedAugments = new List<Augment>();
    private int rerollCost = 3;
    private CurrencyManager currencyManager;
    private bool isPanelOpen = false;

    // Events that other systems can subscribe to
    public event Action<int> OnAugmentSelected;

    private void Start()
    {
        // Get reference to AugmentManager
        augmentManager = FindFirstObjectByType<AugmentManager>();
        if (augmentManager == null)
        {
            Debug.LogError("AugmentManager not found in scene!");
        }

        // Get reference to CurrencyManager
        currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager Instance not found!");
        }
        else
        {
            currencyManager.OnCurrencyChanged += UpdateRerollButtonState;
        }

        // Start with panel hidden
        augmentSelectionPanel.SetActive(false);
        
        // Hook up augment selection buttons
        augment1SelectButton.onClick.AddListener(() => SelectAugment(0));
        augment2SelectButton.onClick.AddListener(() => SelectAugment(1));
        augment3SelectButton.onClick.AddListener(() => SelectAugment(2));
        
        // Hook up reroll button
        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(OnRerollButtonClicked);
        }
        
        // Hook up toggle button
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleAugmentSelection);
            // Ensure toggle button is always interactable
            toggleButton.interactable = true;
            
            // Debug all button properties
            Debug.Log($"[AugmentSelectionUI] Toggle button initialized:");
            Debug.Log($"  - Button.interactable: {toggleButton.interactable}");
            Debug.Log($"  - GameObject.activeSelf: {toggleButton.gameObject.activeSelf}");
            Debug.Log($"  - GameObject.activeInHierarchy: {toggleButton.gameObject.activeInHierarchy}");
            
            // Check if there's a CanvasGroup blocking interactions
            CanvasGroup cg = toggleButton.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                Debug.Log($"  - CanvasGroup found on button");
                Debug.Log($"    - interactable: {cg.interactable}");
                Debug.Log($"    - blocksRaycasts: {cg.blocksRaycasts}");
                Debug.Log($"    - alpha: {cg.alpha}");
                
                // Fix CanvasGroup if needed
                if (!cg.interactable)
                {
                    cg.interactable = true;
                    Debug.Log($"  - Fixed: CanvasGroup.interactable set to true");
                }
                if (!cg.blocksRaycasts)
                {
                    cg.blocksRaycasts = true;
                    Debug.Log($"  - Fixed: CanvasGroup.blocksRaycasts set to true");
                }
            }
            
            // Check parent's CanvasGroup
            CanvasGroup parentCG = toggleButton.GetComponentInParent<CanvasGroup>();
            if (parentCG != null && parentCG != cg)
            {
                Debug.Log($"  - Parent CanvasGroup found");
                Debug.Log($"    - interactable: {parentCG.interactable}");
                Debug.Log($"    - blocksRaycasts: {parentCG.blocksRaycasts}");
            }
            
            // Check Image component
            Image buttonImage = toggleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                Debug.Log($"  - Image component found");
                Debug.Log($"    - raycastTarget: {buttonImage.raycastTarget}");
                if (!buttonImage.raycastTarget)
                {
                    buttonImage.raycastTarget = true;
                    Debug.Log($"  - Fixed: Image.raycastTarget set to true");
                }
            }
        }
        else
        {
            Debug.LogWarning("[AugmentSelectionUI] toggleButton is NULL! Not assigned in inspector!");
        }
        
        // Initialize reroll cost display
        UpdateRerollCostDisplay();
    }

    /// <summary>
    /// Call this function to show the augment selection interface
    /// </summary>
    public void ShowAugmentSelection()
    {
        if (augmentManager == null)
        {
            Debug.LogError("AugmentManager not initialized!");
            return;
        }

        // Get 3 random augments from inactive list
        GetRandomAugments();
        
        // Display the selected augments
        DisplayAugments();
        
        // Show the panel
        augmentSelectionPanel.SetActive(true);
        isPanelOpen = true;
        
        // Ensure toggle button is always visible and clickable
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(true);
            toggleButton.interactable = true;
        }
        
        if (GameSpeedButton.Instance != null)
        {
            GameSpeedButton.Instance.SetPaused(true);
        }
        else
        {
            Time.timeScale = 0f;
        }
        
        // Update reroll button state
        if (currencyManager != null)
        {
            UpdateRerollButtonState(currencyManager.GetCurrency());
        }
    }
    
    /// <summary>
    /// Toggle the augment selection panel open/closed
    /// </summary>
    private void ToggleAugmentSelection()
    {
        Debug.Log($"[AugmentSelectionUI] Toggle button clicked! Panel is currently {(isPanelOpen ? "OPEN" : "CLOSED")}");
        
        if (isPanelOpen)
        {
            HideAugmentSelection();
        }
        else
        {
            ShowAugmentSelection();
        }
    }
    
    /// <summary>
    /// Reset reroll cost for new augment selection round
    /// </summary>
    public void ResetRerollCost()
    {
        rerollCost = 3;
        UpdateRerollCostDisplay();
    }

    /// <summary>
    /// Randomly selects 3 augments from the inactive augments list
    /// </summary>
    private void GetRandomAugments()
    {
        selectedAugments.Clear();
        
        List<Augment> inactiveAugments = augmentManager.augmentList.inactiveAugments;
        
        if (inactiveAugments.Count < 3)
        {
            Debug.LogWarning("Not enough augments in inactive list. Need at least 3.");
            return;
        }

        // Create a temporary list to track which indices we've used
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < inactiveAugments.Count; i++)
        {
            availableIndices.Add(i);
        }

        // Randomly select 3 without replacement
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            selectedAugments.Add(inactiveAugments[availableIndices[randomIndex]]);
            availableIndices.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// Displays the selected augments in the UI
    /// </summary>
    private void DisplayAugments()
    {
        if (selectedAugments.Count < 3)
        {
            Debug.LogWarning("Not enough selected augments to display!");
            return;
        }

        // Display augment 1
        if (augment1Name != null) augment1Name.text = selectedAugments[0].Name;
        if (augment1Description != null) augment1Description.text = selectedAugments[0].Description;

        // Display augment 2
        if (augment2Name != null) augment2Name.text = selectedAugments[1].Name;
        if (augment2Description != null) augment2Description.text = selectedAugments[1].Description;

        // Display augment 3
        if (augment3Name != null) augment3Name.text = selectedAugments[2].Name;
        if (augment3Description != null) augment3Description.text = selectedAugments[2].Description;
    }

    /// <summary>
    /// Called when reroll button is clicked
    /// </summary>
    private void OnRerollButtonClicked()
    {
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager not found!");
            return;
        }

        if (!currencyManager.TrySpendCurrency(rerollCost))
        {
            Debug.Log($"Cannot afford reroll! Need {rerollCost}, have {currencyManager.GetCurrency()}");
            return;
        }

        Debug.Log($"Rerolled augments for {rerollCost} gold!");
        rerollCost++; // Increase cost by 1 for next reroll
        UpdateRerollCostDisplay();
        GetRandomAugments();
        DisplayAugments();
    }
    
    private void UpdateRerollCostDisplay()
    {
        if (rerollCostText != null)
        {
            rerollCostText.text = $"Reroll: {rerollCost}";
        }
    }
    
    private void UpdateRerollButtonState(int currentCurrency)
    {
        if (rerollButton != null && augmentSelectionPanel.activeInHierarchy)
        {
            rerollButton.interactable = currentCurrency >= rerollCost;
        }
    }

    /// <summary>
    /// Explicitly hide the augment selection panel
    /// (Toggle button remains visible)
    /// </summary>
    public void HideAugmentSelection()
    {
        augmentSelectionPanel.SetActive(false);
        isPanelOpen = false;
        if (GameSpeedButton.Instance != null)
        {
            GameSpeedButton.Instance.SetPaused(false);
        }
        else
        {
            Time.timeScale = 1f;
        }
        
        // Keep toggle button visible when panel is manually closed
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(true);
            toggleButton.interactable = true;
            
            // Force button to normal state
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Called when player selects an augment
    /// </summary>
    private void SelectAugment(int augmentIndex)
    {
        if (augmentIndex < 0 || augmentIndex >= selectedAugments.Count)
        {
            Debug.LogError("Invalid augment index!");
            return;
        }

        Augment selectedAugment = selectedAugments[augmentIndex];
        Debug.Log($"Augment '{selectedAugment.Name}' selected!");
        
        // Move augment from inactive to active
        augmentManager.augmentList.inactiveAugments.Remove(selectedAugment);
        augmentManager.AddActiveAugment(selectedAugment);
        
        // Add to augment inventory so it shows in the main screen inventory
        augmentManager.AcquireAugment(selectedAugment);

        if (selectedAugment.ApplyImmediatelyOnAcquire)
        {
            selectedAugment.Apply?.Invoke();
        }
        
        // Hide the panel first
        HideAugmentSelection();
        ResetRerollCost(); // Reset reroll cost for next time
        
        // Hide toggle button when augment is selected
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(false);
        }
        
        // Trigger event for other systems (passing the index)
        OnAugmentSelected?.Invoke(augmentIndex);
    }

    /// <summary>
    /// Update augment data (call this when you have real data)
    /// </summary>
    public void UpdateAugmentData(string[] names, string[] descriptions)
    {
        if (names.Length >= 3 && descriptions.Length >= 3)
        {
            if (augment1Name != null) augment1Name.text = names[0];
            if (augment1Description != null) augment1Description.text = descriptions[0];
            if (augment2Name != null) augment2Name.text = names[1];
            if (augment2Description != null) augment2Description.text = descriptions[1];
            if (augment3Name != null) augment3Name.text = names[2];
            if (augment3Description != null) augment3Description.text = descriptions[2];
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        augment1SelectButton.onClick.RemoveAllListeners();
        augment2SelectButton.onClick.RemoveAllListeners();
        augment3SelectButton.onClick.RemoveAllListeners();
        
        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveAllListeners();
        }
        
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
        }
        
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateRerollButtonState;
        }
    }
}
