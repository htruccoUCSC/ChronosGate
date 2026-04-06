using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevEnemySpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private WaveManager waveManager;

    [Header("Lane Buttons")]
    [SerializeField] private RectTransform laneButtonRoot;
    [SerializeField] private Button laneButtonPrefab;
    [SerializeField] private Vector2 laneButtonScreenOffset = new Vector2(48f, 0f);

    [Header("Enemy Selection")]
    [SerializeField] private TMP_Dropdown enemyDropdown;
    [SerializeField] private TextMeshProUGUI statusText;

    private readonly List<LaneButtonEntry> laneButtons = new List<LaneButtonEntry>();
    private readonly List<GameObject> availableEnemies = new List<GameObject>();

    private void Start()
    {
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }

        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        BuildEnemyOptionsFromWaveManager();
        BuildLaneButtons();

        SetStatus("Dev enemy spawner ready.");
    }

    private void LateUpdate()
    {
        RefreshLaneButtonPositions();
    }

    [ContextMenu("Refresh Enemy Dropdown")]
    public void RefreshEnemyDropdown()
    {
        BuildEnemyOptionsFromWaveManager();
    }

    private void BuildEnemyOptionsFromWaveManager()
    {
        availableEnemies.Clear();

        if (waveManager != null)
        {
            AddEnemyIfValid(waveManager.baseEnemyPrefab);
            AddEnemyIfValid(waveManager.shadowEnemyPrefab);
            AddEnemyIfValid(waveManager.enemyPrefab);
            AddEnemyIfValid(waveManager.enemyRedPrefab);
            AddEnemyIfValid(waveManager.enemyYellowPrefab);
            AddEnemyIfValid(waveManager.enemyGreenPrefab);

            if (waveManager.enemyPrefabs != null)
            {
                for (int i = 0; i < waveManager.enemyPrefabs.Count; i++)
                {
                    AddEnemyIfValid(waveManager.enemyPrefabs[i]);
                }
            }
        }

        if (enemyDropdown == null)
        {
            return;
        }

        enemyDropdown.ClearOptions();
        List<string> labels = new List<string>();
        for (int i = 0; i < availableEnemies.Count; i++)
        {
            labels.Add(availableEnemies[i].name);
        }

        if (labels.Count == 0)
        {
            labels.Add("No enemies configured");
        }

        enemyDropdown.AddOptions(labels);
        enemyDropdown.value = 0;
        enemyDropdown.RefreshShownValue();
    }

    private void AddEnemyIfValid(GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (!availableEnemies.Contains(prefab))
        {
            availableEnemies.Add(prefab);
        }
    }

    private void BuildLaneButtons()
    {
        if (laneButtonRoot == null || laneButtonPrefab == null || boardManager == null)
        {
            SetStatus("Missing lane button references.");
            return;
        }

        foreach (Transform child in laneButtonRoot)
        {
            Destroy(child.gameObject);
        }

        laneButtons.Clear();

        int minLane = 1;
        int maxLane = Mathf.Max(minLane, boardManager.Height - 2);

        for (int laneY = minLane; laneY <= maxLane; laneY++)
        {
            Button laneButton = Instantiate(laneButtonPrefab, laneButtonRoot);
            int capturedLane = laneY;
            laneButton.onClick.RemoveAllListeners();
            laneButton.onClick.AddListener(() => SpawnSelectedEnemyInLane(capturedLane));

            TextMeshProUGUI label = laneButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = $"Spawn L{capturedLane}";
            }

            laneButtons.Add(new LaneButtonEntry(capturedLane, laneButton.GetComponent<RectTransform>()));
        }

        RefreshLaneButtonPositions();
    }

    private void RefreshLaneButtonPositions()
    {
        if (boardManager == null || boardManager.GameTilemap == null || laneButtonRoot == null)
        {
            return;
        }

        Camera cameraToUse = Camera.main;
        if (cameraToUse == null)
        {
            return;
        }

        Canvas rootCanvas = laneButtonRoot.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        for (int i = 0; i < laneButtons.Count; i++)
        {
            LaneButtonEntry entry = laneButtons[i];
            if (entry.RectTransform == null)
            {
                continue;
            }

            Vector3 worldAnchor = GetLaneEndWorldPosition(entry.LaneY);
            Vector3 screenPosition = cameraToUse.WorldToScreenPoint(worldAnchor);

            if (screenPosition.z < 0f)
            {
                entry.RectTransform.gameObject.SetActive(false);
                continue;
            }

            entry.RectTransform.gameObject.SetActive(true);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                laneButtonRoot,
                (Vector2)screenPosition + laneButtonScreenOffset,
                uiCamera,
                out Vector2 localPoint))
            {
                entry.RectTransform.anchoredPosition = localPoint;
            }
        }
    }

    private Vector3 GetLaneEndWorldPosition(int laneY)
    {
        int x = Mathf.Max(0, boardManager.Width - 1);
        Vector3Int cell = new Vector3Int(x, laneY, 0);
        Vector3 center = boardManager.GameTilemap.GetCellCenterWorld(cell);
        return center + new Vector3(0.5f, 0f, 0f);
    }

    private void SpawnSelectedEnemyInLane(int laneY)
    {
        if (waveManager == null)
        {
            SetStatus("WaveManager not found.");
            return;
        }

        if (availableEnemies.Count == 0)
        {
            SetStatus("No enemy prefabs configured in WaveManager.");
            return;
        }

        int selectedIndex = enemyDropdown != null ? enemyDropdown.value : 0;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, availableEnemies.Count - 1);

        GameObject selectedEnemy = availableEnemies[selectedIndex];
        bool spawned = waveManager.SpawnEnemyInLane(selectedEnemy, laneY);
        if (spawned)
        {
            SetStatus($"Spawned {selectedEnemy.name} in lane {laneY}.");
        }
        else
        {
            SetStatus($"Failed to spawn {selectedEnemy.name} in lane {laneY}.");
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private readonly struct LaneButtonEntry
    {
        public int LaneY { get; }
        public RectTransform RectTransform { get; }

        public LaneButtonEntry(int laneY, RectTransform rectTransform)
        {
            LaneY = laneY;
            RectTransform = rectTransform;
        }
    }
}
