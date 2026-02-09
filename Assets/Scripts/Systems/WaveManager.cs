using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; } // global access so enemies can register

    [Header("References")]
    public GameObject enemyPrefab; // what enemy gets spawned
    public Tilemap tilemap;        // tilemap used to figure out spawn positions (this took me too long to find out)

    [Header("Spawn")]
    public int spawnOffsetCells = 0; // how far past the right edge enemies spawn
    public float spawnDelay = 0.75f; // delay between enemy spawns in a wave

    [Header("Waves")]
    public int currentWave = 1;          // current wave number
    public int enemiesPerWave = 1;       // how many enemies spawn this wave
    public int enemiesAddedPerWave = 0;  // extra enemies added each new wave
    public float timeBetweenWaves = 2f;  // wait time between waves

    private readonly HashSet<TargetDummyTest> aliveEnemies = new HashSet<TargetDummyTest>(); // tracks living enemies

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
        StartCoroutine(RunWaves()); // start the wave loop
    }

    private IEnumerator RunWaves()
    {
        while (true)
        {
            Debug.Log($"--- WAVE {currentWave} START ---");

            // spawn enemies for this wave
            for (int i = 0; i < enemiesPerWave; i++)
            {
                TrySpawnEnemyOnTile();
                yield return new WaitForSeconds(spawnDelay);
            }

            // wait until all enemies are dead
            yield return new WaitUntil(() => aliveEnemies.Count == 0);

            Debug.Log($"--- WAVE {currentWave} CLEARED ---");

            yield return new WaitForSeconds(timeBetweenWaves);
            currentWave++;                       // move to next wave
            enemiesPerWave += enemiesAddedPerWave;
        }
    }

    private void TrySpawnEnemyOnTile()
    {
        // safety checks
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveManager: enemyPrefab not assigned.");
            return;
        }

        if (tilemap == null)
        {
            Debug.LogError("WaveManager: tilemap not assigned.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds; // tilemap grid bounds

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

        int spawnX = rightmostXWithTile + spawnOffsetCells; // final spawn column

        // collect all valid Y rows in that column
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

    public void RegisterEnemy(TargetDummyTest enemy)
    {
        aliveEnemies.Add(enemy); // called when an enemy spawns
    }

    public void UnregisterEnemy(TargetDummyTest enemy)
    {
        aliveEnemies.Remove(enemy); // called when an enemy dies
    }
}
