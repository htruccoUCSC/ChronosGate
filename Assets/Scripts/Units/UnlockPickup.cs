using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// A clickable pickup that represents a newly unlocked unit.
/// Player must click to claim the unit before progressing.
/// Uses the unit's prefab sprite automatically - no need to create separate prefabs for each unit!
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class UnlockPickup : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private SpriteRenderer unitIcon;
    [SerializeField] private TextMeshPro unitNameText;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private ParticleSystem unlockParticles;
    
    [Header("Animation")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmount = 0.3f;
    [SerializeField] private float rotationSpeed = 30f;
    
    [Header("Icon Settings")]
    [SerializeField] private float iconTargetSize = 1f; // Target size in Unity units (1x1)
    [SerializeField] private Vector3 iconOffset = Vector3.zero;
    [SerializeField] private int sortingOrder = 100; // High value to render on top
    [SerializeField] private string sortingLayerName = "Default";
    
    [Header("Debug")]
    [SerializeField] private bool showColliderDebug = true;
    [SerializeField] private Color debugColliderColor = Color.green;
    
    private UnitDefinition unlockedUnit;
    private WaveUnlock unlockData;
    private bool hasBeenClaimed = false;
    private Vector3 startPosition;
    private BoxCollider2D boxCollider;
    
    public event System.Action<UnitDefinition> OnPickupClaimed;
    
    private void Awake()
    {
        // Ensure we have a collider for mouse detection
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Set collider size to match the icon area
        boxCollider.size = new Vector2(iconTargetSize * 1.2f, iconTargetSize * 1.2f);
        boxCollider.isTrigger = false; // Must be false for OnMouseDown to work
        
        // CRITICAL: Make sure this GameObject is NOT on "Ignore Raycast" layer
        if (gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
        {
            Debug.LogWarning("[UnlockPickup] GameObject was on 'Ignore Raycast' layer! Changing to Default.");
            gameObject.layer = 0; // Default layer
        }
        
        Debug.Log($"[UnlockPickup] Collider setup: size={boxCollider.size}, isTrigger={boxCollider.isTrigger}, layer={LayerMask.LayerToName(gameObject.layer)}");
    }
    
    private void Start()
    {
        startPosition = transform.position;
        
        if (unlockParticles != null)
        {
            unlockParticles.Play();
        }
    }
    
    private void Update()
    {
        if (hasBeenClaimed) return;
        
        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Gentle rotation
        if (glowEffect != null)
        {
            glowEffect.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        
        // Debug: Manual click detection using NEW Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            if (hit.collider != null)
            {
                Debug.Log($"[UnlockPickup] Manual raycast hit: {hit.collider.gameObject.name} at {hit.point}");
                
                if (hit.collider == boxCollider)
                {
                    Debug.Log("[UnlockPickup] Manual raycast hit this pickup's collider! Claiming...");
                    ClaimPickup();
                }
            }
            else
            {
                Debug.Log($"[UnlockPickup] Manual raycast hit nothing at mouse position {mousePos}");
            }
        }
    }
    
    /// <summary>
    /// Initializes the pickup with unit data.
    /// Automatically loads the sprite from the unit's prefab and scales it to fit 1x1 Unity units!
    /// </summary>
    public void Initialize(UnitDefinition unitDef, WaveUnlock unlock)
    {
        unlockedUnit = unitDef;
        unlockData = unlock;
        
        if (unitDef == null)
        {
            Debug.LogError("[UnlockPickup] Cannot initialize with null UnitDefinition!");
            return;
        }
        
        // Automatically set the sprite from the unit's prefab
        if (unitIcon != null)
        {
            Sprite sprite = unitDef.Icon;
            if (sprite != null)
            {
                unitIcon.sprite = sprite;
                
                // Scale the sprite to fit within iconTargetSize (1x1 Unity units)
                ScaleSpriteToFit(unitIcon, iconTargetSize);
                
                unitIcon.transform.localPosition = iconOffset;
                
                // Set high sorting order to render on top
                unitIcon.sortingLayerName = sortingLayerName;
                unitIcon.sortingOrder = sortingOrder;
                
                Debug.Log($"[UnlockPickup] Loaded and scaled sprite for {unitDef.Name} from prefab.");
            }
            else
            {
                Debug.LogWarning($"[UnlockPickup] Could not load Icon for {unitDef.Name}. Check PrefabPath: {unitDef.PrefabPath}");
            }
        }
        else
        {
            Debug.LogWarning("[UnlockPickup] unitIcon SpriteRenderer not assigned! Create a child GameObject with SpriteRenderer component.");
        }
        
        // Set name text
        if (unitNameText != null)
        {
            string displayText = !string.IsNullOrEmpty(unlock?.unlockMessage) 
                ? unlock.unlockMessage 
                : $"New Unit: {unitDef.Name}";
            unitNameText.text = displayText;
            
            // Make sure text renders on top too
            unitNameText.sortingOrder = sortingOrder + 1;
        }
        
        // Set glow effect sorting if it exists
        if (glowEffect != null)
        {
            SpriteRenderer glowRenderer = glowEffect.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
            {
                glowRenderer.sortingLayerName = sortingLayerName;
                glowRenderer.sortingOrder = sortingOrder - 1; // Behind the icon
            }
        }
        
        Debug.Log($"[UnlockPickup] Initialized with unit: {unitDef.Name}");
    }
    
    /// <summary>
    /// Scales a sprite to fit within a target size while maintaining aspect ratio.
    /// </summary>
    private void ScaleSpriteToFit(SpriteRenderer spriteRenderer, float targetSize)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        
        // Get the sprite's actual size in world units
        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        float maxDimension = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y);
        
        // Calculate scale factor to fit within targetSize
        float scaleFactor = targetSize / maxDimension;
        
        spriteRenderer.transform.localScale = Vector3.one * scaleFactor;
        
        Debug.Log($"[UnlockPickup] Scaled sprite: original max dimension = {maxDimension}, scale factor = {scaleFactor}");
    }
    
    /// <summary>
    /// Called when player clicks the pickup.
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log("[UnlockPickup] OnMouseDown triggered!");
        ClaimPickup();
    }
    
    /// <summary>
    /// Called when mouse is over the collider.
    /// </summary>
    private void OnMouseOver()
    {
        if (!hasBeenClaimed)
        {
            Debug.Log("[UnlockPickup] OnMouseOver - Mouse is over the pickup!");
        }
    }
    
    /// <summary>
    /// Claims the pickup and unlocks the unit.
    /// </summary>
    public void ClaimPickup()
    {
        if (hasBeenClaimed) return;
        
        hasBeenClaimed = true;
        
        // Add unit to unlock manager
        if (UnitUnlockManager.Instance != null && unlockedUnit != null)
        {
            UnitUnlockManager.Instance.UnlockUnit(unlockedUnit.UnitID);
        }
        
        Debug.Log($"[UnlockPickup] Player claimed: {unlockedUnit?.Name}");
        
        // Notify listeners
        OnPickupClaimed?.Invoke(unlockedUnit);
        
        // Play claim effect
        if (unlockParticles != null)
        {
            unlockParticles.Stop();
        }
        
        // Destroy after a brief delay
        Destroy(gameObject, 0.5f);
    }
    
    /// <summary>
    /// Shows a highlight effect when mouse hovers.
    /// </summary>
    private void OnMouseEnter()
    {
        if (hasBeenClaimed) return;
        
        Debug.Log("[UnlockPickup] Mouse entered!");
        
        if (glowEffect != null)
        {
            // Brighten glow
            var spriteRenderer = glowEffect.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.8f;
                spriteRenderer.color = color;
            }
        }
        
        transform.localScale = Vector3.one * 1.1f;
    }
    
    /// <summary>
    /// Removes highlight when mouse leaves.
    /// </summary>
    private void OnMouseExit()
    {
        if (hasBeenClaimed) return;
        
        Debug.Log("[UnlockPickup] Mouse exited!");
        
        if (glowEffect != null)
        {
            var spriteRenderer = glowEffect.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.5f;
                spriteRenderer.color = color;
            }
        }
        
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Debug visualization of the collider bounds.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showColliderDebug) return;
        
        BoxCollider2D col = boxCollider != null ? boxCollider : GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = debugColliderColor;
            
            // Draw the collider bounds
            Vector3 center = transform.position + (Vector3)col.offset;
            Vector3 size = col.size;
            
            // Draw wire cube to show collider bounds
            Gizmos.DrawWireCube(center, size);
            
            // Draw a small sphere at the center
            Gizmos.DrawWireSphere(center, 0.1f);
        }
    }
}
