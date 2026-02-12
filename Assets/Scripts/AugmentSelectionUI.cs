using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AugmentSelectionUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject augmentSelectionPanel;
    [SerializeField] private GameObject toggleButton; // Changed from Button to GameObject
    
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

    // Events that other systems can subscribe to
    public event Action<int> OnAugmentSelected;

    private Button toggleButtonComponent; // Cache the button component

    private void Start()
    {
        // Get the button component from the GameObject
        toggleButtonComponent = toggleButton.GetComponent<Button>();
        
        // Start with panel hidden but toggle button visible
        augmentSelectionPanel.SetActive(false);
        toggleButton.SetActive(false); // Hidden initially until ShowAugmentSelection is called
        
        // Hook up buttons
        toggleButtonComponent.onClick.AddListener(ToggleAugmentPanel);
        augment1SelectButton.onClick.AddListener(() => SelectAugment(0));
        augment2SelectButton.onClick.AddListener(() => SelectAugment(1));
        augment3SelectButton.onClick.AddListener(() => SelectAugment(2));
        
        // Set placeholder data
        SetPlaceholderData();
    }

    private void SetPlaceholderData()
    {
        // Placeholder names
        if (augment1Name != null) augment1Name.text = "Rapid Fire";
        if (augment2Name != null) augment2Name.text = "Extra Damage";
        if (augment3Name != null) augment3Name.text = "Slow Effect";
        
        // Placeholder descriptions
        if (augment1Description != null) augment1Description.text = "Increases attack speed by 25%";
        if (augment2Description != null) augment2Description.text = "Adds 50% damage to all towers";
        if (augment3Description != null) augment3Description.text = "Enemies move 30% slower";
    }

    /// <summary>
    /// Call this function to show the augment selection interface
    /// </summary>
    public void ShowAugmentSelection()
    {
        augmentSelectionPanel.SetActive(true);
        toggleButton.SetActive(true); // Show the toggle button
        Time.timeScale = 0f; // Pause the game
    }

    /// <summary>
    /// Toggle the visibility of the augment panel (button stays visible)
    /// </summary>
    private void ToggleAugmentPanel()
    {
        bool newState = !augmentSelectionPanel.activeSelf;
        augmentSelectionPanel.SetActive(newState);
        
        // Toggle button stays visible, only pause state changes
        Time.timeScale = newState ? 0f : 1f;
    }

    /// <summary>
    /// Called when player selects an augment
    /// </summary>
    private void SelectAugment(int augmentIndex)
    {
        Debug.Log($"Augment {augmentIndex} selected!");
        
        // Trigger event for other systems
        OnAugmentSelected?.Invoke(augmentIndex);
        
        // Hide both the panel AND the toggle button
        augmentSelectionPanel.SetActive(false);
        toggleButton.SetActive(false);
        Time.timeScale = 1f; // Resume game
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
        if (toggleButtonComponent != null)
        {
            toggleButtonComponent.onClick.RemoveAllListeners();
        }
        augment1SelectButton.onClick.RemoveAllListeners();
        augment2SelectButton.onClick.RemoveAllListeners();
        augment3SelectButton.onClick.RemoveAllListeners();
    }
}