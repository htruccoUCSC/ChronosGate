using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// Manages unit unlock progression and persistence.
/// Singleton that tracks which units the player has unlocked.
/// </summary>
public class UnitUnlockManager : MonoBehaviour
{
    public static UnitUnlockManager Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private string unlockProgressionFile = "unlock_progression.json";
    [SerializeField] private bool debugUnlockAll = false;
    
    [Header("Runtime Data")]
    public PlayerProgressionData playerProgression;
    private UnlockProgressionData unlockConfig;
    private bool isInitialized = false;
    
    // Events
    public event System.Action<string> OnUnitUnlocked;
    public event System.Action OnProgressionReady;
    
    private const string SAVE_KEY = "PlayerProgression";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        StartCoroutine(InitializeProgression());
    }
    
    /// <summary>
    /// Initializes progression system (WebGL-Compatible).
    /// </summary>
    private IEnumerator InitializeProgression()
    {
        yield return StartCoroutine(LoadUnlockConfigurationCoroutine());
        LoadPlayerProgression();
        
        // Mark as ready and notify listeners
        isInitialized = true;
        Debug.Log("[UnitUnlockManager] Progression system initialized and ready!");
        OnProgressionReady?.Invoke();
    }
    
    /// <summary>
    /// Loads the unlock progression configuration from StreamingAssets (WebGL-compatible).
    /// </summary>
    private IEnumerator LoadUnlockConfigurationCoroutine()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, unlockProgressionFile);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        using (var request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[UnitUnlockManager] Unlock configuration not found at {filePath}: {request.error}. Creating default configuration.");
                CreateDefaultConfiguration();
                yield break;
            }

            ParseUnlockConfig(request.downloadHandler.text);
        }
#else
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            ParseUnlockConfig(jsonText);
        }
        else
        {
            Debug.LogWarning($"[UnitUnlockManager] Unlock configuration not found at {filePath}. Creating default configuration.");
            CreateDefaultConfiguration();
        }
#endif

        yield return null;
    }
    
    /// <summary>
    /// Parses the unlock configuration JSON.
    /// </summary>
    private void ParseUnlockConfig(string jsonText)
    {
        unlockConfig = JsonConvert.DeserializeObject<UnlockProgressionData>(jsonText);
        Debug.Log($"[UnitUnlockManager] Loaded unlock configuration with {unlockConfig.startingUnits.Count} starting units and {unlockConfig.unlockSequence.Count} unlocks.");
    }
    
    /// <summary>
    /// Creates a default unlock configuration if none exists.
    /// </summary>
    private void CreateDefaultConfiguration()
    {
        unlockConfig = new UnlockProgressionData
        {
            startingUnits = new List<string> { "unit_basic_archer", "unit_basic_warrior" },
            unlockSequence = new List<WaveUnlock>
            {
                new WaveUnlock { waveNumber = 3, unitID = "unit_basic_mage", unlockMessage = "Unlocked: Mage!" },
                new WaveUnlock { waveNumber = 5, unitID = "unit_advanced_knight", unlockMessage = "Unlocked: Knight!" },
                new WaveUnlock { waveNumber = 10, unitID = "unit_legendary_dragon", unlockMessage = "Unlocked: Dragon!" }
            }
        };
    }
    
    /// <summary>
    /// Loads player progression from PlayerPrefs.
    /// </summary>
    private void LoadPlayerProgression()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            playerProgression = JsonConvert.DeserializeObject<PlayerProgressionData>(json);
            Debug.Log($"[UnitUnlockManager] Loaded player progression: {playerProgression.unlockedUnitIDs.Count} units unlocked, highest wave: {playerProgression.highestWaveReached}");
        }
        else
        {
            // First time playing - initialize with starting units
            playerProgression = new PlayerProgressionData();
            if (unlockConfig != null && unlockConfig.startingUnits != null)
            {
                playerProgression.unlockedUnitIDs.AddRange(unlockConfig.startingUnits);
            }
            SavePlayerProgression();
            Debug.Log($"[UnitUnlockManager] Created new player progression with {playerProgression.unlockedUnitIDs.Count} starting units.");
        }
        
        // Debug mode: unlock everything
        if (debugUnlockAll)
        {
            UnlockAllUnits();
        }
    }
    
    /// <summary>
    /// Saves player progression to PlayerPrefs.
    /// </summary>
    public void SavePlayerProgression()
    {
        string json = JsonConvert.SerializeObject(playerProgression, Formatting.Indented);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("[UnitUnlockManager] Player progression saved.");
    }
    
    /// <summary>
    /// Checks if a unit is unlocked.
    /// </summary>
    public bool IsUnitUnlocked(string unitID)
    {
        if (string.IsNullOrEmpty(unitID)) return false;
        if (debugUnlockAll) return true;
        if (!playerProgression.isProgressionMode) return true; // All units available in sandbox mode
        
        return playerProgression.unlockedUnitIDs.Contains(unitID);
    }
    
    /// <summary>
    /// Checks if the progression system has finished initializing.
    /// </summary>
    public bool IsReady()
    {
        return isInitialized;
    }
    
    /// <summary>
    /// Gets all unlocked unit IDs.
    /// </summary>
    public List<string> GetUnlockedUnitIDs()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[UnitUnlockManager] GetUnlockedUnitIDs called before initialization complete!");
            return null;
        }
        
        if (debugUnlockAll || !playerProgression.isProgressionMode)
        {
            // Return all units if debug mode or sandbox mode
            return null; // Null means "all units available"
        }
        
        return new List<string>(playerProgression.unlockedUnitIDs);
    }
    
    /// <summary>
    /// Unlocks a unit by ID.
    /// </summary>
    public bool UnlockUnit(string unitID)
    {
        if (string.IsNullOrEmpty(unitID))
        {
            Debug.LogWarning("[UnitUnlockManager] Attempted to unlock null or empty unitID.");
            return false;
        }
        
        if (playerProgression.unlockedUnitIDs.Contains(unitID))
        {
            Debug.Log($"[UnitUnlockManager] Unit {unitID} is already unlocked.");
            return false;
        }
        
        playerProgression.unlockedUnitIDs.Add(unitID);
        SavePlayerProgression();
        
        Debug.Log($"[UnitUnlockManager] Unlocked unit: {unitID}");
        OnUnitUnlocked?.Invoke(unitID);
        
        return true;
    }
    
    /// <summary>
    /// Checks what unit (if any) should unlock at the given wave number.
    /// </summary>
    public WaveUnlock GetUnlockForWave(int waveNumber)
    {
        if (unlockConfig == null || unlockConfig.unlockSequence == null)
        {
            return null;
        }
        
        return unlockConfig.unlockSequence.FirstOrDefault(unlock => unlock.waveNumber == waveNumber);
    }
    
    /// <summary>
    /// Updates the highest wave reached and checks for unlocks.
    /// </summary>
    public WaveUnlock UpdateWaveProgress(int waveNumber)
    {
        if (waveNumber > playerProgression.highestWaveReached)
        {
            playerProgression.highestWaveReached = waveNumber;
            SavePlayerProgression();
        }
        
        // Check if this wave unlocks a new unit
        WaveUnlock unlock = GetUnlockForWave(waveNumber);
        if (unlock != null && !IsUnitUnlocked(unlock.unitID))
        {
            return unlock;
        }
        
        return null;
    }
    
    /// <summary>
    /// Toggles progression mode on/off.
    /// </summary>
    public void SetProgressionMode(bool enabled)
    {
        playerProgression.isProgressionMode = enabled;
        SavePlayerProgression();
        Debug.Log($"[UnitUnlockManager] Progression mode set to: {enabled}");
    }
    
    /// <summary>
    /// Debug: Unlocks all units.
    /// </summary>
    [ContextMenu("Unlock All Units")]
    public void UnlockAllUnits()
    {
        if (unlockConfig != null && unlockConfig.unlockSequence != null)
        {
            foreach (var unlock in unlockConfig.unlockSequence)
            {
                if (!playerProgression.unlockedUnitIDs.Contains(unlock.unitID))
                {
                    playerProgression.unlockedUnitIDs.Add(unlock.unitID);
                }
            }
        }
        SavePlayerProgression();
        Debug.Log("[UnitUnlockManager] All units unlocked (debug).");
    }
    
    /// <summary>
    /// Debug: Resets player progression.
    /// </summary>
    [ContextMenu("Reset Progression")]
    public void ResetProgression()
    {
        playerProgression = new PlayerProgressionData();
        if (unlockConfig != null && unlockConfig.startingUnits != null)
        {
            playerProgression.unlockedUnitIDs.AddRange(unlockConfig.startingUnits);
        }
        SavePlayerProgression();
        Debug.Log("[UnitUnlockManager] Player progression reset.");
    }
}
