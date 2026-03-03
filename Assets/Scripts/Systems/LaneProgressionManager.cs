using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages progressive lane unlocking over the first waves.
/// Integrates with UnitUnlockManager and ProgressionGameLoopManager.
/// </summary>
public class LaneProgressionManager : MonoBehaviour
{
    public static LaneProgressionManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private TileMapManager tileMapManager;
    
    [Header("Lane Progression Settings")]
    [SerializeField] private int startingLanes = 1;
    [SerializeField] private int maxLanes = 6;
    [SerializeField] private List<LaneUnlock> laneUnlockSequence = new List<LaneUnlock>
    {
        new LaneUnlock { waveNumber = 1, laneCount = 1 },
        new LaneUnlock { waveNumber = 2, laneCount = 2 },
        new LaneUnlock { waveNumber = 4, laneCount = 3 },
        new LaneUnlock { waveNumber = 6, laneCount = 4 },
        new LaneUnlock { waveNumber = 8, laneCount = 5 },
        new LaneUnlock { waveNumber = 10, laneCount = 6 }
    };
    
    [Header("Runtime")]
    [SerializeField] private int currentLaneCount;
    
    // Events
    public event System.Action<int> OnLanesUnlocked;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (tileMapManager == null)
        {
            tileMapManager = FindFirstObjectByType<TileMapManager>();
        }
        
        if (tileMapManager == null)
        {
            Debug.LogError("[LaneProgressionManager] TileMapManager not found!");
        }
    }
    
    private void Start()
    {
        // Set starting lane count
        SetLaneCount(startingLanes);
        currentLaneCount = startingLanes;
    }
    
    /// <summary>
    /// Checks if a wave should unlock new lanes and applies the change.
    /// </summary>
    public bool CheckAndUnlockLanes(int waveNumber)
    {
        LaneUnlock unlock = laneUnlockSequence.FirstOrDefault(u => u.waveNumber == waveNumber);
        
        if (unlock != null && unlock.laneCount > currentLaneCount)
        {
            SetLaneCount(unlock.laneCount);
            currentLaneCount = unlock.laneCount;
            
            Debug.Log($"[LaneProgressionManager] Wave {waveNumber}: Unlocked {unlock.laneCount} lanes!");
            OnLanesUnlocked?.Invoke(unlock.laneCount);
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Sets the number of playable lanes on the board.
    /// </summary>
    private void SetLaneCount(int laneCount)
    {
        if (tileMapManager == null)
        {
            Debug.LogError("[LaneProgressionManager] Cannot set lane count - TileMapManager is null!");
            return;
        }
        
        // Clamp to valid range
        laneCount = Mathf.Clamp(laneCount, 1, maxLanes);
        
        // Board height = lanes + 2 (top and bottom walls)
        int boardHeight = laneCount + 2;
        
        Debug.Log($"[LaneProgressionManager] Setting board to {laneCount} lanes (height: {boardHeight})");
        tileMapManager.SetHeight(boardHeight);
    }
    
    /// <summary>
    /// Gets the current number of playable lanes.
    /// </summary>
    public int GetCurrentLaneCount()
    {
        return currentLaneCount;
    }
    
    /// <summary>
    /// Gets the next lane unlock wave number.
    /// </summary>
    public int? GetNextLaneUnlockWave()
    {
        LaneUnlock nextUnlock = laneUnlockSequence
            .Where(u => u.laneCount > currentLaneCount)
            .OrderBy(u => u.waveNumber)
            .FirstOrDefault();
        
        return nextUnlock?.waveNumber;
    }
    
    /// <summary>
    /// Debug: Unlock all lanes immediately.
    /// </summary>
    [ContextMenu("Unlock All Lanes")]
    public void UnlockAllLanes()
    {
        SetLaneCount(maxLanes);
        currentLaneCount = maxLanes;
        Debug.Log("[LaneProgressionManager] All lanes unlocked (debug).");
    }
    
    /// <summary>
    /// Debug: Reset to starting lanes.
    /// </summary>
    [ContextMenu("Reset to Starting Lanes")]
    public void ResetLanes()
    {
        SetLaneCount(startingLanes);
        currentLaneCount = startingLanes;
        Debug.Log("[LaneProgressionManager] Reset to starting lanes.");
    }
}

/// <summary>
/// Defines when a lane count unlocks.
/// </summary>
[System.Serializable]
public class LaneUnlock
{
    public int waveNumber;
    public int laneCount;
}
