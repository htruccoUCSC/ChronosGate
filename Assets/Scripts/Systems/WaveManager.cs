using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; } // global access so enemies can register / report escapes

    [Header("References")]
    public GameObject enemyPrefab; // what enemy gets spawned
    public Tilemap tilemap;        // tilemap used to figure out spawn + map edges

    [Header("Spawn")]
    public int spawnOffsetCells = 0;   // how far past the right edge enemies spawn
    public float spawnDelay = 0.75f;   // delay between enemy spawns in a wave

    [Header("Waves")]
    public int currentWave = 1;          // current wave number
    public int enemiesPerWave = 1;       // how many enemies spawn this wave
    public int enemiesAddedPerWave = 0;  // extra enemies added each new wave
    public float timeBetweenWaves = 2f;  // wait time between waves

    [Header("Lives")]
    public int lives = 3; // you start with 3 lives

    private readonly HashSet<BaseEnemy> aliveEnemies = new HashSet<BaseEnemy>(); // tracks living enemies
    private bool gameOver = false; // stops spawning when you hit 0 lives
    private bool waveActive = false;
    // bool that enables the old automatic wave behavior
    private bool autoRunWaves = false;

    private float leftLoseX = 0f; // world X where enemies count as "reached the end"

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
                if (tilemap.HasTile(cell) && x < leftmostXWithTile)
                    leftmostXWithTile = x;
            }
        }

        if (leftmostXWithTile == int.MaxValue)
        {
            Debug.LogError("WaveManager: Tilemap has no tiles.");
            return;
        }

        // left edge "lose line" is the center of that leftmost tile column
        leftLoseX = tilemap.GetCellCenterWorld(new Vector3Int(leftmostXWithTile, 0, 0)).x;
    }

    public float GetLoseLineX()
    {
        return leftLoseX; // enemies use this to know when they reached the end
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
            yield return new WaitUntil(() => aliveEnemies.Count == 0 || gameOver);

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
        return !waveActive && aliveEnemies.Count == 0;
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

        yield return new WaitUntil(() => aliveEnemies.Count == 0 || gameOver);

        Debug.Log($"--- WAVE {currentWave} CLEARED ---");

        currentWave++;
        enemiesPerWave += enemiesAddedPerWave;
        waveActive = false;
    }

    private void TrySpawnEnemyOnTile()
    {
        // safety checks
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveManager: enemyPrefab not assigned.");
            return;
        }
        
        if (enemyPrefab.GetComponent<BaseEnemy>() == null)
        {
            Debug.LogError("WaveManager: enemyPrefab does not have a BaseEnemy component. Assign a real enemy prefab.");
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
                if (tilemap.HasTile(cell) && x > rightmostXWithTile)
                    rightmostXWithTile = x;
            }
        }

        if (rightmostXWithTile == int.MinValue)
        {
            Debug.LogError("WaveManager: Tilemap has no tiles.");
            return;
        }

        int spawnX = rightmostXWithTile + spawnOffsetCells;

        // collect all valid Y rows in that rightmost tile column
        List<int> validYs = new List<int>();
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            if (tilemap.HasTile(new Vector3Int(rightmostXWithTile, y, 0)))
                validYs.Add(y);
        }

        if (validYs.Count == 0)
        {
            Debug.LogError("WaveManager: No valid Y rows found.");
            return;
        }

        // limit spawns to the 5 middle rows
        validYs.Sort();
        int takeCount = Mathf.Min(5, validYs.Count);
        int midIndex = validYs.Count / 2;
        int startIndex = Mathf.Clamp(midIndex - (takeCount / 2), 0, validYs.Count - takeCount);

        List<int> middleYs = validYs.GetRange(startIndex, takeCount);
        int chosenY = middleYs[Random.Range(0, middleYs.Count)];

        // convert tile position to world space and spawn enemy
        Vector3Int spawnCell = new Vector3Int(spawnX, chosenY, 0);
        Vector3 spawnWorld = tilemap.GetCellCenterWorld(spawnCell);

        Instantiate(enemyPrefab, spawnWorld, Quaternion.identity);
    }

    public void RegisterEnemy(BaseEnemy enemy)
    {
        aliveEnemies.Add(enemy); // called when an enemy spawns
    }

    public void UnregisterEnemy(BaseEnemy enemy)
    {
        aliveEnemies.Remove(enemy); // called when an enemy dies or gets destroyed
    }

    public void EnemyReachedEnd(BaseEnemy enemy)
    {
        if (gameOver) return;

        lives -= 1;
        Debug.Log($"Enemy reached the end! Lives left: {lives}");

        // destroy the enemy so it unregisters (OnDestroy in BaseEnemy handles it)
        if (enemy != null)
            Destroy(enemy.gameObject);

        if (lives <= 0)
        {
            gameOver = true;
            Debug.Log("GAME OVER (0 lives).");
        }
    }
}
