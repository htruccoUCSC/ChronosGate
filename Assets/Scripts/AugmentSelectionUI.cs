using UnityEngine;
using UnityEngine.UI;
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

    private AugmentManager augmentManager;
    private List<Augment> selectedAugments = new List<Augment>();

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

        // Start with panel hidden
        augmentSelectionPanel.SetActive(false);
        
        // Hook up augment selection buttons
        augment1SelectButton.onClick.AddListener(() => SelectAugment(0));
        augment2SelectButton.onClick.AddListener(() => SelectAugment(1));
        augment3SelectButton.onClick.AddListener(() => SelectAugment(2));
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
        Time.timeScale = 0f; // Pause the game
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
    /// Explicitly hide the augment selection panel
    /// </summary>
    public void HideAugmentSelection()
    {
        augmentSelectionPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
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
        
        // Hide the panel first
        HideAugmentSelection();
        
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
    }
}