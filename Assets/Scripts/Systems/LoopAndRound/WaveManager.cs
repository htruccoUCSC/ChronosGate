using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; } // global access so enemies can register / report escapes

    [Header("References")]
    public GameObject enemyPrefab; //BaseEnemy Default
    public List<GameObject> enemyPrefabs = new List<GameObject>(); //OPTIONAL RANDOM SPAWN FROM LIST
    public GameObject baseEnemyPrefab; //explicit base enemy prefab 
    public GameObject shadowEnemyPrefab; //explicit shadow enemy prefab
    public Tilemap tilemap;        // tilemap used to figure out spawn + map edges

    [Header("Spawn")]
    public int spawnOffsetCells = 0;   // how far past the right edge enemies spawn
    public float spawnDelay = 0.75f;   // delay between enemy spawns in a wave
    [Range(0f, 1f)] public float shadowSpawnChance = 0.2f;

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

        // compute map end threshold once at start
        RecomputeMapEndX();

        // If you want the old behavior (auto waves), set autoRunWaves to true
        if (autoRunWaves)
        {
            StartCoroutine(RunWaves());
        }
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

            // spawn enemies for this wave
            for (int i = 0; i < enemiesPerWave; i++)
            {
                if (gameOver) yield break;
                TrySpawnEnemyOnTile();
                yield return new WaitForSeconds(spawnDelay);
            }

            // wait until all enemies are dead or escaped
            yield return new WaitUntil(() => AliveEnemyCount() == 0 || gameOver);

            if (gameOver) yield break;

            Debug.Log($"--- WAVE {currentWave} CLEARED ---");

            yield return new WaitForSeconds(timeBetweenWaves);
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

        // spawn enemies for this wave
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (gameOver)
            {
                waveActive = false;
                yield break;
            }

            TrySpawnEnemyOnTile();
            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitUntil(() => AliveEnemyCount() == 0 || gameOver);

        if (gameOver)
        {
            waveActive = false;
            yield break;
        }

        Debug.Log($"--- WAVE {currentWave} CLEARED ---");

        currentWave++;
        enemiesPerWave += enemiesAddedPerWave;
        waveActive = false;
    }

    private void TrySpawnEnemyOnTile()
    {
        // safety checks
        if (enemyPrefab == null && (enemyPrefabs == null || enemyPrefabs.Count == 0)
            && baseEnemyPrefab == null && shadowEnemyPrefab == null)
        {
            Debug.LogError("WaveManager: no enemy prefab assigned.");
            return;
        }

        if (tilemap == null)
        {
            Debug.LogError("WaveManager: tilemap not assigned.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        // find the rightmost column that actually has tiles
        int rightmostXWithTile = int.MinValue;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (IsSpawnableCell(cell) && x > rightmostXWithTile)
                    rightmostXWithTile = x;
            }
        }

        if (rightmostXWithTile == int.MinValue)
        {
            Debug.LogError("WaveManager: Tilemap has no tiles.");
            return;
        }

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
            return;
        }

        // allow spawning on any valid row in the spawn column
        int chosenY = validYs[Random.Range(0, validYs.Count)];

        // convert tile position to world space and spawn enemy
        Vector3Int spawnCell = new Vector3Int(spawnX, chosenY, 0);
        Vector3 spawnWorld = tilemap.GetCellCenterWorld(spawnCell);

        GameObject prefabToSpawn = enemyPrefab;
        if (baseEnemyPrefab != null && shadowEnemyPrefab != null)
        {
            prefabToSpawn = Random.value < shadowSpawnChance ? shadowEnemyPrefab : baseEnemyPrefab;
        }
        else if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        }

        GameObject go = Instantiate(prefabToSpawn, spawnWorld, Quaternion.identity);

        // try BaseEnemy first (new system), otherwise fall back to TargetDummyTest (older system)
        BaseEnemy baseEnemy = go.GetComponentInParent<BaseEnemy>();
        if (baseEnemy != null)
        {
            RegisterEnemy(baseEnemy);
            return;
        }

        TargetDummyTest testEnemy = go.GetComponentInParent<TargetDummyTest>();
        if (testEnemy != null)
        {
            RegisterEnemy(testEnemy);
            return;
        }

        Debug.LogWarning("WaveManager: spawned enemyPrefab but it has no BaseEnemy or TargetDummyTest component");
    }

    private bool IsSpawnableCell(Vector3Int cell)
    {
        if (boardManager != null)
            return boardManager.IsWalkable(cell);

        return tilemap.HasTile(cell);
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
}
