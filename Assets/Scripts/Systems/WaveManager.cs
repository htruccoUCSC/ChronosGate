using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    public GameObject enemyPrefab;
    public Tilemap tilemap; // drag your Tilemap here

    [Header("Spawn")]
    public int spawnOffsetCells = 0;     // 0 = spawn on rightmost tile column, 1 = one cell past it, etc.
    public float spawnDelay = 0.75f;

    [Header("Waves")]
    public int currentWave = 1;
    public int enemiesPerWave = 3;
    public int enemiesAddedPerWave = 2;
    public float timeBetweenWaves = 2f;

    private readonly HashSet<TargetDummyTest> aliveEnemies = new HashSet<TargetDummyTest>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("WaveManager started.");
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (true)
        {
            Debug.Log($"--- WAVE {currentWave} START ---");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                TrySpawnEnemyOnTile();
                yield return new WaitForSeconds(spawnDelay);
            }

            yield return new WaitUntil(() => aliveEnemies.Count == 0);

            Debug.Log($"--- WAVE {currentWave} CLEARED ---");

            yield return new WaitForSeconds(timeBetweenWaves);
            currentWave++;
            enemiesPerWave += enemiesAddedPerWave;
        }
    }

    private void TrySpawnEnemyOnTile()
    {
        if (enemyPrefab == null) { Debug.LogError("WaveManager: enemyPrefab not assigned."); return; }
        if (tilemap == null) { Debug.LogError("WaveManager: tilemap not assigned."); return; }

        // Scan the tilemap bounds to find valid spawn cells
        BoundsInt bounds = tilemap.cellBounds;

        // Find the rightmost X that actually contains at least one tile
        int rightmostXWithTile = int.MinValue;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cell))
                {
                    if (x > rightmostXWithTile) rightmostXWithTile = x;
                }
            }
        }

        if (rightmostXWithTile == int.MinValue)
        {
            Debug.LogError("WaveManager: Tilemap has no tiles in its bounds.");
            return;
        }

        int spawnX = rightmostXWithTile + spawnOffsetCells;

        // Collect all Y positions in that column that have tiles
        List<int> validYs = new List<int>();
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            Vector3Int cell = new Vector3Int(rightmostXWithTile, y, 0);
            if (tilemap.HasTile(cell))
                validYs.Add(y);
        }

        if (validYs.Count == 0)
        {
            Debug.LogError("WaveManager: No valid tile rows found in the rightmost tile column.");
            return;
        }

        int chosenY = validYs[Random.Range(0, validYs.Count)];

        // Spawn on the center of the chosen cell (using the spawn column)
        Vector3Int spawnCell = new Vector3Int(spawnX, chosenY, 0);
        Vector3 spawnWorld = tilemap.GetCellCenterWorld(spawnCell);

        Instantiate(enemyPrefab, spawnWorld, Quaternion.identity);
    }

    public void RegisterEnemy(TargetDummyTest enemy) => aliveEnemies.Add(enemy);
    public void UnregisterEnemy(TargetDummyTest enemy) => aliveEnemies.Remove(enemy);
}
