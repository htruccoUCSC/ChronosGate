using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Simple utility that picks tiles from the game's tilemap and spawns a prefab at each cell center.
// Provides helpers to pick N random tiles or a percent of unoccupied tiles.
public class TileSpawner : MonoBehaviour
{
    // Helper: collect all walkable tiles (uses BoardManager if available, otherwise falls back to Tilemap.HasTile)
    private static List<Vector3Int> CollectWalkableTiles(Tilemap tilemap, BoardManager board)
    {
        List<Vector3Int> results = new List<Vector3Int>();
        if (tilemap == null) return results;

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                bool validTile = (board != null) ? board.IsWalkable(cell) : tilemap.HasTile(cell);
                if (validTile) results.Add(cell);
            }
        }

        return results;
    }

    // Helper: instantiate, scale, reset animator and schedule destroy
    private static GameObject InstantiateAndPrepare(GameObject prefab, Vector3 worldPos, Quaternion rotation, float spawnScale, float lifetime, BaseUnit owner, float damage)
    {
        if (prefab == null) return null;
        GameObject obj = GameObject.Instantiate(prefab, worldPos, rotation);
        if (obj == null) return null;

        if (spawnScale != 1f)
            obj.transform.localScale = obj.transform.localScale * spawnScale;

        var animator = obj.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            var state = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(state.shortNameHash, 0, 0f);
        }

        ConfigureProjectile(obj, worldPos, lifetime, owner, damage);

        if (lifetime > 0f)
            Destroy(obj, lifetime);

        return obj;
    }

    private static void ConfigureProjectile(GameObject obj, Vector3 worldPos, float lifetime, BaseUnit owner, float damage)
    {
        if (obj == null || owner == null) return;

        Projectile proj = obj.GetComponentInChildren<Projectile>();
        if (proj == null) return;

        proj.lifetime = Mathf.Max(0.01f, lifetime);
        proj.speed = 0f;
        proj.passThroughEnemies = true;
        proj.maxPenetration = 0;
        proj.SetIgnoreRowCheck(true);

        Collider2D col = proj.GetComponent<Collider2D>();
        if (col == null) col = proj.GetComponentInChildren<Collider2D>();
        if (col != null) col.isTrigger = true;

        float finalDamage = Mathf.Max(1f, damage);
        proj.Setup(finalDamage, Vector2.right, 0f, worldPos, false, owner);
    }
    // Pick `count` random occupied tiles from WaveManager.tilemap and instantiate `prefab` there.
    public static void SpawnOnRandomTiles(int count, GameObject prefab, float lifetime = 5f, LayerMask targetMask = default, BaseUnit owner = null, float damage = 0f)
    {
        if (prefab == null) return;

        Tilemap tilemap = WaveManager.Instance != null ? WaveManager.Instance.tilemap : null;
        BoardManager board = UnityEngine.Object.FindFirstObjectByType<BoardManager>();
        if (tilemap == null)
        {
            Debug.LogWarning("TileSpawner: no tilemap available to pick tiles from.");
            return;
        }

        // gather candidate tiles
        List<Vector3Int> occupied = CollectWalkableTiles(tilemap, board);

        if (occupied.Count == 0)
        {
            Debug.LogWarning("TileSpawner: tilemap has no tiles to spawn on.");
            return;
        }

        // clamp count to available tiles
        count = Mathf.Clamp(count, 1, occupied.Count);

        // choose `count` unique random indices
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, occupied.Count);
            Vector3Int chosen = occupied[idx];

            // remove chosen so we don't pick twice
            occupied.RemoveAt(idx);

            Vector3 worldPos = tilemap.GetCellCenterWorld(chosen);
            InstantiateAndPrepare(prefab, worldPos, Quaternion.Euler(0f, 0f, 90f), 1.5f, lifetime, owner, damage);
        }
    }

    // Spawn a percentage of tiles that are NOT occupied by any BaseUnit.
    public static void SpawnPercentUnoccupiedTiles(float percent, GameObject prefab, float lifetime = 5f, LayerMask targetMask = default, BaseUnit owner = null, float damage = 0f)
    {
        if (prefab == null) return;
        if (percent <= 0f) return;

        Tilemap tilemap = WaveManager.Instance != null ? WaveManager.Instance.tilemap : null;
        BoardManager board = UnityEngine.Object.FindFirstObjectByType<BoardManager>();
        if (tilemap == null)
        {
            Debug.LogWarning("TileSpawner: no tilemap available to pick tiles from.");
            return;
        }

        List<Vector3Int> walkable = CollectWalkableTiles(tilemap, board);
        List<Vector3Int> unoccupied = new List<Vector3Int>();

        // filter out tiles that have nearby units
        foreach (var cell in walkable)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(cell);
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.35f);
            bool occupied = false;
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.GetComponentInParent<BaseUnit>() != null)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied) unoccupied.Add(cell);
        }

        if (unoccupied.Count == 0)
        {
            Debug.Log("TileSpawner: no unoccupied tiles available.");
            return;
        }

        int toSpawn = Mathf.RoundToInt(unoccupied.Count * percent);
        if (toSpawn <= 0) toSpawn = 1; // at least one if percent > 0

        // pick unique random tiles from unoccupied
        for (int i = 0; i < toSpawn && unoccupied.Count > 0; i++)
        {
            int idx = Random.Range(0, unoccupied.Count);
            Vector3Int chosen = unoccupied[idx];
            unoccupied.RemoveAt(idx);

            Vector3 worldPos = tilemap.GetCellCenterWorld(chosen);
            InstantiateAndPrepare(prefab, worldPos, Quaternion.identity, 3.5f, lifetime, owner, damage);
        }
    }
}
