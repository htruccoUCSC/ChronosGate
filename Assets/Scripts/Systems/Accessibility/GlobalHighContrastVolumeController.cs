using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
///This file allows the grayscale filter to work across all scenes (Whole Game)
/// </summary>
[RequireComponent(typeof(Volume))]
public class GlobalHighContrastVolumeController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The global Volume that holds your grayscale Color Adjustments profile.")]
    [SerializeField] private Volume m_Volume;

    [Header("Toggle")]
    [Tooltip("Keyboard key used to turn the high-contrast grayscale effect on or off.")]
    [SerializeField] private Key m_ToggleKey = Key.G;

    [Tooltip("Whether the effect starts enabled the first time the game launches.")]
    [SerializeField] private bool m_StartEnabled = true;

    [Header("Persistence")]
    [Tooltip("If true, the on/off state is saved and restored between play sessions.")]
    [SerializeField] private bool m_SaveToPlayerPrefs = true;

    private const string k_PrefsKey = "Accessibility_GlobalHighContrastEnabled";

   // Checks if there is already an instance of the GlobalHighContrastVolumeController
    private static GlobalHighContrastVolumeController s_Instance;

    // Checks if affect is already enabled
    private bool m_IsEnabled;

    private void Awake()
    {
        //If there is a duplicate it dstroys th duplicate to prevent weird bugs.
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject); // Destroys the duplicate
            return;
        }

        s_Instance = this;

       // This allows the object to exist btween scenes and doesn't get destroyed when a new scene is loaded.
        DontDestroyOnLoad(gameObject);

        // Uses the Volume on the same GameObject.
        m_Volume = GetComponent<Volume>();

        if (m_SaveToPlayerPrefs && PlayerPrefs.HasKey(k_PrefsKey))
        {
            m_IsEnabled = PlayerPrefs.GetInt(k_PrefsKey, m_StartEnabled ? 1 : 0) == 1;
        }
        else
        {
            m_IsEnabled = m_StartEnabled;
        }

        // Apply the saved/default state immediately so the effect is correct on startup.
        ApplyState();

        // Makes all Existing scene Cameras use post processing
        EnablePostProcessingForAllCameras();

        // Makes sure Newly loaded Cameras use post processing
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Always unsubscribe from scene events when this object goes away.
        if (s_Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            s_Instance = null;
        }
    }

    private void Update()
    {
        // Adds a Keyboard Toggle for Filter
        if (Keyboard.current != null && Keyboard.current[m_ToggleKey].wasPressedThisFrame)
        {
            ToggleEffect();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // New scenes often bring in new cameras, makes sure settings reapply to new cameras.
        EnablePostProcessingForAllCameras();

        // Reapply volume weight after loading in case the scene changed references
        ApplyState();
    }

    private void ApplyState()
    {
        // Sets the Volume weight to 1 if the effect is enabled and 0 if it is disabled.
        m_Volume.weight = m_IsEnabled ? 1f : 0f;

        // Save the setting so the player's accessibility preference persists.
        if (m_SaveToPlayerPrefs)
        {
            PlayerPrefs.SetInt(k_PrefsKey, m_IsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void EnablePostProcessingForAllCameras()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera cam = cameras[i];
            UniversalAdditionalCameraData cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null) continue;

            cameraData.renderPostProcessing = true;
            cameraData.volumeLayerMask = ~0;
        }
    }

    // Public helper functions so UI buttons or other scripts can force the effect on.
    public void EnableEffect()
    {
        m_IsEnabled = true;
        ApplyState();
    }

    public void DisableEffect()
    {
        m_IsEnabled = false;
        ApplyState();
    }

    public void ToggleEffect()
    {
        m_IsEnabled = !m_IsEnabled;
        ApplyState();
    }

    // Read Only Property.
    public bool IsEnabled => m_IsEnabled;
}
