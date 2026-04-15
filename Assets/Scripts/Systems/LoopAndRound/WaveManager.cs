using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; } // global access so enemies can register / report escapes

    [Header("References")]
    public GameObject enemyPrefab; //BaseEnemy Default
     public GameObject enemyRedPrefab; //BaseEnemy Red
     public GameObject enemyYellowPrefab; //BaseEnemy Yellow
     public GameObject enemyGreenPrefab; //BaseEnemy Green

    public List<GameObject> enemyPrefabs = new List<GameObject>(); //OPTIONAL RANDOM SPAWN FROM LIST
    public GameObject baseEnemyPrefab; //explicit base enemy prefab
    public GameObject shadowEnemyPrefab; //explicit shadow enemy prefab
    public Tilemap tilemap;        // tilemap used to figure out spawn + map edges
    public PortalManager portalManager;
    public SpawnableManager spawnableManager;



    [Header("Spawn")]
    public int spawnOffsetCells = 0;   // how far past the right edge enemies spawn
    public float spawnDelayMinimum = 0.5f;
    public float spawnDelayMaximum = 1f;   // delay between enemy spawns in a wave
    [Range(0f, 1f)] public float shadowSpawnChance = 0.2f;
    public int shadowUnlockWave = 999;
    // Bayo Bandele - 4/11/2026: must match the PortalPreSpawn clip length (6 frames at 25 samples = 0.24s)
    private float m_PreSpawnAnimDuration = 0f;

    [SerializeField] private float m_PreSpawnEarlyOffset = 0.24f;

    [Header("Waves")]
    public int currentWave = 1;          // current wave number
    public int enemiesPerWave = 2;       // how many enemies spawn this wave
    public int enemiesAddedPerWave = 4;  // extra enemies added each new wave
    public float timeBetweenWaves = 3f;  // wait time between waves

    [Header("Lives")]
    public int lives = 3; // you start with 3 lives

    // keep BOTH systems alive for now so we don't break older units/scripts
    private readonly HashSet<BaseEnemy> aliveBaseEnemies = new HashSet<BaseEnemy>();              // "real" enemies
    private readonly HashSet<TargetDummyTest> aliveTestEnemies = new HashSet<TargetDummyTest>(); // older test enemies

    private bool gameOver = false; // stops spawning when you hit 0 lives
    private bool waveActive = false;

    // bool that enables the old automatic wave behavior
    private bool autoRunWaves = false;

    private float leftLoseX = 0f; // world X where enemies count as "reached the end"
    private BoardManager boardManager;
    private int m_CurrentWaveTotalEnemies;
    private int m_CurrentWaveSpawnedEnemies;

    public int CurrentWaveTotalEnemies => m_CurrentWaveTotalEnemies;
    public int CurrentWaveSpawnedEnemies => m_CurrentWaveSpawnedEnemies;
    public int CurrentWaveEnemiesRemaining
    {
        get
        {
            int spawnedRemaining = Mathf.Max(0, AliveEnemyCount());
            int pendingSpawns = Mathf.Max(0, m_CurrentWaveTotalEnemies - m_CurrentWaveSpawnedEnemies);
            return spawnedRemaining + pendingSpawns;
        }
    }

    void Awake()
    {
        // make sure only one WaveManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("WaveManager started.");

        boardManager = FindFirstObjectByType<BoardManager>();
        if (spawnableManager == null)
        {
            spawnableManager = FindFirstObjectByType<SpawnableManager>();
        }

        // Bayo Bandele - 4/11/2026: fall back to scene search if portalManager wasn't wired in the Inspector
        if (portalManager == null)
        {
            portalManager = FindFirstObjectByType<PortalManager>();
        }
        // Bayo Bandele - 4/11/2026: initialize portals at game start so lane visuals are ready before the first wave
        if (portalManager != null)
        {
            portalManager.init();
        }

        UpdateSpawnablesForCurrentWave();

        // compute map end threshold once at start
        RecomputeMapEndX();

        // If you want the old behavior (auto waves), set autoRunWaves to true
        if (autoRunWaves)
        {
            StartCoroutine(RunWaves());
        }

        // Bayo Bandele - 4/11/2026: read the PortalPreSpawn clip length at startup so spawn timing auto-syncs to the animation
        CachePreSpawnAnimDuration();
    }

    private void RecomputeMapEndX()
    {
        if (tilemap == null)
        {
            Debug.LogError("WaveManager: tilemap not assigned.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        // find the leftmost column that actually has tiles
        int leftmostXWithTile = int.MaxValue;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (IsSpawnableCell(cell) && x < leftmostXWithTile)
                    leftmostXWithTile = x;
            }
        }

        if (leftmostXWithTile == int.MaxValue)
        {
            Debug.LogError("WaveManager: Tilemap has no tiles.");
            return;
        }

        // Enemies should count as escaped once they fully cross the left edge of the lane.
        leftLoseX = tilemap.CellToWorld(new Vector3Int(leftmostXWithTile, 0, 0)).x;
    }

    public float GetLoseLineX()
    {
        return leftLoseX; // enemies use this to know when they reached the end
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    private int AliveEnemyCount()
    {
        return aliveBaseEnemies.Count + aliveTestEnemies.Count;
    }

    private IEnumerator RunWaves()
    {
        while (!gameOver)
        {
            Debug.Log($"--- WAVE {currentWave} START ---");
            UpdateSpawnablesForCurrentWave();
            BeginWaveTracking();

            // spawn enemies for this wave
            for (int i = 0; i < enemiesPerWave; i++)
            {
                if (gameOver) yield break;
                yield return StartCoroutine(TrySpawnEnemyOnTile());
                yield return new WaitForSeconds(Random.Range(spawnDelayMinimum, spawnDelayMaximum));
            }

            // wait until all enemies are dead or escaped
            yield return new WaitUntil(() => AliveEnemyCount() == 0 || gameOver);

            if (gameOver) yield break;

            Debug.Log($"--- WAVE {currentWave} CLEARED ---");
            AdvancePortalsForClearedWave();

            yield return new WaitForSeconds(timeBetweenWaves);
            EndWaveTracking();
            currentWave++;
            enemiesPerWave += enemiesAddedPerWave;
        }
    }

    // function for the gameloop manager to start a wave
    public void StartNextWave()
    {
        if (gameOver || waveActive) return;
        StartCoroutine(RunSingleWave());
    }

    // returns true if the current wave is fully cleared
    public bool IsWaveComplete()
    {
        return !waveActive && AliveEnemyCount() == 0;
    }

    // run wave helper function that only runs one wave and then stops
    private IEnumerator RunSingleWave()
    {
        waveActive = true;
        Debug.Log($"--- WAVE {currentWave} START ---");
        UpdateSpawnablesForCurrentWave();

        // EnemyCountMult scales the number of enemies this wave.
        // Captured as a local so it stays constant for the whole wave even if context
        // changes mid-wave. Does NOT permanently modify enemiesPerWave — the base value
        // is preserved so future waves scale correctly from the unmodified baseline.
        RoundModifierContext ctx = RoundModifierContext.Instance;
        int effectiveEnemyCount = ctx != null && ctx.EnemyCountMult != 1f
            ? Mathf.Max(1, Mathf.RoundToInt(enemiesPerWave * ctx.EnemyCountMult))
            : enemiesPerWave;

        BeginWaveTracking(effectiveEnemyCount);

        // spawn enemies for this wave
        for (int i = 0; i < effectiveEnemyCount; i++)
        {
            if (gameOver)
            {
                EndWaveTracking();
                waveActive = false;
                yield break;
            }

            yield return StartCoroutine(TrySpawnEnemyOnTile());

            // EnemySpawnIntervalMult scales the delay between spawns.
            // Below 1.0 = enemies swarm in faster; above 1.0 = they trickle in slower.
            float spawnDelay = Random.Range(spawnDelayMinimum, spawnDelayMaximum);
            if (ctx != null && ctx.EnemySpawnIntervalMult != 1f)
                spawnDelay *= ctx.EnemySpawnIntervalMult;

            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitUntil(() => AliveEnemyCount() == 0 || gameOver);

        if (gameOver)
        {
            EndWaveTracking();
            waveActive = false;
            yield break;
        }

        Debug.Log($"--- WAVE {currentWave} CLEARED ---");
        AdvancePortalsForClearedWave();

        EndWaveTracking();
        currentWave++;
        enemiesPerWave += enemiesAddedPerWave;
        waveActive = false;
    }

    private IEnumerator TrySpawnEnemyOnTile()
    {
        // // safety checks
        // if (enemyPrefab == null && enemyRedPrefab == null && enemyYellowPrefab == null && enemyGreenPrefab == null
        //     && (enemyPrefabs == null || enemyPrefabs.Count == 0)
        //     && baseEnemyPrefab == null && shadowEnemyPrefab == null)
        // {
        //     Debug.LogError("WaveManager: no enemy prefab assigned.");
        //     return;
        // }

        // if (tilemap == null)
        // {
        //     Debug.LogError("WaveManager: tilemap not assigned.");
        //     return;
        // }

        if (!TryGetRightmostSpawnableColumn(out int rightmostXWithTile))
        {
            Debug.LogError("WaveManager: Could not determine a valid rightmost spawn column.");
            yield break;
        }

        BoundsInt bounds = tilemap.cellBounds;

        int spawnX = rightmostXWithTile + Mathf.Max(1, spawnOffsetCells);

        // collect all valid Y rows in that rightmost tile column
        List<int> validYs = new List<int>();
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            if (IsSpawnableCell(new Vector3Int(rightmostXWithTile, y, 0)))
                validYs.Add(y);
        }

        if (validYs.Count == 0)
        {
            Debug.LogError("WaveManager: No valid Y rows found.");
            yield break;
        }

        int chosenY = ChooseSpawnRow(validYs);

        // convert tile position to world space and spawn enemy
        Vector3Int spawnCell = new Vector3Int(spawnX, chosenY, 0);
        Vector3 spawnWorld = tilemap.GetCellCenterWorld(spawnCell);

        GameObject prefabToSpawn = null;
        bool canSpawnShadow = shadowEnemyPrefab != null && currentWave >= shadowUnlockWave;
        if (canSpawnShadow && Random.value < shadowSpawnChance)
        {
            prefabToSpawn = shadowEnemyPrefab;
        }
        else
        {
            prefabToSpawn = GetRandomNonShadowPrefab();
            if (prefabToSpawn == null && canSpawnShadow)
            {
                prefabToSpawn = shadowEnemyPrefab;
            }
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("WaveManager: failed to choose a spawn prefab.");
            yield break;
        }

        // Bayo Bandele - 4/11/2026: spawn enemy at portal world position and arc it to the lane entry point
        Vector3 spawnOrigin = spawnWorld;
        bool hasPortal = false;
        if (portalManager != null)
        {
            Portal portal = portalManager.GetPortal(chosenY);
            if (portal != null)
            {
                // use PortalManager's own method so the offset stays in one place
                spawnOrigin = portalManager.GetPortalWorldPosition(portal);
                hasPortal = true;
                // Bayo Bandele - 4/13/2026: wait for the portal spawn animation to finish before triggering pre-spawn
                yield return new WaitUntil(() => portalManager.IsPortalSpawnComplete(chosenY));
                // Bayo Bandele - 4/11/2026: play warning animation then wait for it to finish before spawning
                portalManager.TriggerPortalPreSpawn(chosenY);
                yield return new WaitForSeconds(m_PreSpawnAnimDuration - m_PreSpawnEarlyOffset);
            }
        }

        GameObject go = Instantiate(prefabToSpawn, spawnOrigin, Quaternion.identity);
        m_CurrentWaveSpawnedEnemies++;

        // try BaseEnemy first (new system), otherwise fall back to TargetDummyTest (older system)
        BaseEnemy baseEnemy = go.GetComponentInParent<BaseEnemy>();
        if (baseEnemy != null)
        {
            RegisterEnemy(baseEnemy);
            if (hasPortal)
            {
                // Bayo Bandele - 4/11/2026: land on the rightmost tile, not off-screen, so arc goes onto the map
                Vector3Int landingCell = new Vector3Int(rightmostXWithTile - 2, chosenY, 0);
                Vector3 landingPos = tilemap.GetCellCenterWorld(landingCell);
                baseEnemy.LaunchFromPortal(landingPos);
            }
            yield break;
        }

        Debug.LogWarning("WaveManager: spawned enemyPrefab but it has no BaseEnemy or TargetDummyTest component");
    }

    private GameObject GetRandomNonShadowPrefab()
    {
        if (spawnableManager != null)
        {
            return spawnableManager.GetRandomSpawnable();
        }

        if (baseEnemyPrefab != null)
        {
            return baseEnemyPrefab;
        }

        return enemyPrefab;
    }

    private void UpdateSpawnablesForCurrentWave()
    {
        if (spawnableManager == null)
        {
            return;
        }

        spawnableManager.UpdateSpawnablesForRound(currentWave);
    }

    public bool SpawnEnemyInLane(GameObject enemyPrefabToSpawn, int laneY)
    {
        if (enemyPrefabToSpawn == null)
        {
            Debug.LogError("WaveManager: SpawnEnemyInLane called with null prefab.");
            return false;
        }

        if (tilemap == null)
        {
            Debug.LogError("WaveManager: tilemap not assigned.");
            return false;
        }

        if (!TryGetRightmostSpawnableColumn(out int rightmostXWithTile))
        {
            Debug.LogError("WaveManager: Could not determine a valid rightmost spawn column.");
            return false;
        }

        Vector3Int laneCell = new Vector3Int(rightmostXWithTile, laneY, 0);
        if (!IsSpawnableCell(laneCell))
        {
            Debug.LogWarning($"WaveManager: lane {laneY} is not spawnable.");
            return false;
        }

        int spawnX = rightmostXWithTile + Mathf.Max(1, spawnOffsetCells);
        Vector3Int spawnCell = new Vector3Int(spawnX, laneY, 0);
        Vector3 spawnWorld = tilemap.GetCellCenterWorld(spawnCell);

        GameObject go = Instantiate(enemyPrefabToSpawn, spawnWorld, Quaternion.identity);
        if (boardManager != null && boardManager.EnemyParent != null)
        {
            go.transform.SetParent(boardManager.EnemyParent);
        }

        BaseEnemy baseEnemy = go.GetComponentInParent<BaseEnemy>();
        if (baseEnemy != null)
        {
            RegisterEnemy(baseEnemy);
        }
        else
        {
            Debug.LogWarning("WaveManager: SpawnEnemyInLane spawned object without BaseEnemy component.");
        }

        return true;
    }

    private bool TryGetRightmostSpawnableColumn(out int rightmostXWithTile)
    {
        rightmostXWithTile = int.MinValue;

        if (tilemap == null)
        {
            return false;
        }

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (IsSpawnableCell(cell) && x > rightmostXWithTile)
                {
                    rightmostXWithTile = x;
                }
            }
        }

        return rightmostXWithTile != int.MinValue;
    }

    private int ChooseSpawnRow(List<int> validYs)
    {
        if (validYs == null || validYs.Count == 0)
        {
            return 0;
        }

        if (portalManager == null)
        {
            Debug.Log("Cant find portal");
            return validYs[Random.Range(0, validYs.Count)];
        }

        float totalWeight = 0f;
        List<(int row, float cumulativeWeight)> weightedRows = new List<(int row, float cumulativeWeight)>();

        for (int i = 0; i < validYs.Count; i++)
        {
            int row = validYs[i];
            Portal portal = portalManager.GetPortal(row);
            if (portal == null)
            {
                continue;
            }

            float weight = Mathf.Max(0f, portal.tier);
            if (weight <= 0f)
            {
                continue;
            }

            totalWeight += weight;
            weightedRows.Add((row, totalWeight));
        }

        if (weightedRows.Count == 0 || totalWeight <= 0f)
        {
            return validYs[Random.Range(0, validYs.Count)];
        }

        float roll = Random.Range(0f, totalWeight);
        for (int i = 0; i < weightedRows.Count; i++)
        {
            if (roll <= weightedRows[i].cumulativeWeight)
            {
                return weightedRows[i].row;
            }
        }

        return weightedRows[weightedRows.Count - 1].row;
    }

    private bool IsSpawnableCell(Vector3Int cell)
    {
        if (tilemap == null || !tilemap.HasTile(cell))
        {
            return false;
        }

        // Spawn/lose-line lane checks should ignore tower occupancy.
        // If we use BoardManager.IsWalkable here, a tower in the front lane can block all enemy spawns.
        if (boardManager != null)
        {
            if (cell.x < 0 || cell.x >= boardManager.Width || cell.y < 0 || cell.y >= boardManager.Height)
            {
                return false;
            }

            return cell.y != 0 && cell.y != boardManager.Height - 1;
        }

        BoundsInt bounds = tilemap.cellBounds;
        return cell.y > bounds.yMin && cell.y < bounds.yMax - 1;
    }

    // --- registration for BaseEnemy (new) ---
    public void RegisterEnemy(BaseEnemy enemy)
    {
        aliveBaseEnemies.Add(enemy); // called when an enemy spawns
    }

    public void UnregisterEnemy(BaseEnemy enemy)
    {
        aliveBaseEnemies.Remove(enemy); // called when an enemy dies or gets destroyed
    }

    public void EnemyReachedEnd(BaseEnemy enemy)
    {
        if (gameOver) return;

        lives -= 1;
        Debug.Log($"Enemy reached the end! Lives left: {lives}");

        if (enemy != null)
            Destroy(enemy.gameObject);

        if (lives <= 0)
        {
            gameOver = true;
            Debug.Log("GAME OVER (0 lives).");
        }
    }

    // --- registration for TargetDummyTest (old) ---
    public void RegisterEnemy(TargetDummyTest enemy)
    {
        aliveTestEnemies.Add(enemy); // called when an enemy spawns
    }

    public void UnregisterEnemy(TargetDummyTest enemy)
    {
        aliveTestEnemies.Remove(enemy); // called when an enemy dies or gets destroyed
    }

    public void EnemyReachedEnd(TargetDummyTest enemy)
    {
        if (gameOver) return;

        lives -= 1;
        Debug.Log($"Enemy reached the end! Lives left: {lives}");

        if (enemy != null)
            Destroy(enemy.gameObject);

        if (lives <= 0)
        {
            gameOver = true;
            Debug.Log("GAME OVER (0 lives).");
        }
    }

    private void BeginWaveTracking()
    {
        BeginWaveTracking(enemiesPerWave);
    }

    // Overload used by RunSingleWave when a round modifier has adjusted enemy count.
    // The RunWaves() auto-wave path still calls the no-arg version above.
    private void BeginWaveTracking(int totalEnemies)
    {
        m_CurrentWaveTotalEnemies   = Mathf.Max(0, totalEnemies);
        m_CurrentWaveSpawnedEnemies = 0;
    }

    private void AdvancePortalsForClearedWave()
    {
        if (portalManager == null)
        {
            return;
        }

        portalManager.addPortal(1);
    }

    private void EndWaveTracking()
    {
        m_CurrentWaveTotalEnemies = 0;
        m_CurrentWaveSpawnedEnemies = 0;
    }

    // Bayo Bandele - 4/11/2026: walks the portal animators at startup and caches the PortalPreSpawn clip length so the spawn delay matches the animation exactly
    private void CachePreSpawnAnimDuration()
    {
        if (portalManager == null) return;

        foreach (var kv in portalManager.GetPortalAnimators())
        {
            Animator anim = kv.Value;
            if (anim == null) continue;

            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "PortalPreSpawn")
                {
                    m_PreSpawnAnimDuration = clip.length;
                    Debug.Log($"[WaveManager] PreSpawn duration auto-synced: {m_PreSpawnAnimDuration}s");
                    return;
                }
            }
        }

        Debug.LogWarning("clip not found");
        m_PreSpawnAnimDuration = 0.24f;
    }
}
