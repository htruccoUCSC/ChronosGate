using UnityEngine;

public class AugmentUITester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AugmentSelectionUI augmentUI;
    
    [Header("Test Settings")]
    [SerializeField] private KeyCode testKey = KeyCode.Space;
    [SerializeField] private bool showOnStart = false;
    
    private void Start()
    {
        // Subscribe to augment selection events
        if (augmentUI != null)
        {
            augmentUI.OnAugmentSelected += OnAugmentChosen;
        }
        
        // Optionally show UI on start for quick testing
        if (showOnStart && augmentUI != null)
        {
            Invoke("ShowUIDelayed", 0.5f); // Small delay to ensure everything is initialized
        }
    }
    
    private void Update()
    {
        // Press test key to show augment UI
        if (Input.GetKeyDown(testKey) && augmentUI != null)
        {
            Debug.Log($"[TEST] Showing Augment UI (Press {testKey} to toggle)");
            augmentUI.ShowAugmentSelection();
        }
    }
    
    private void ShowUIDelayed()
    {
        Debug.Log("[TEST] Auto-showing Augment UI on start");
        augmentUI.ShowAugmentSelection();
    }
    
    private void OnAugmentChosen(int augmentIndex)
    {
        Debug.Log($"[TEST] ✓ Player selected augment #{augmentIndex}");
        
        // Here you would normally apply the augment's effects
        switch (augmentIndex)
        {
            case 0:
                Debug.Log("[TEST] Applying 'Rapid Fire' augment");
                break;
            case 1:
                Debug.Log("[TEST] Applying 'Extra Damage' augment");
                break;
            case 2:
                Debug.Log("[TEST] Applying 'Slow Effect' augment");
                break;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (augmentUI != null)
        {
            augmentUI.OnAugmentSelected -= OnAugmentChosen;
        }
    }
}