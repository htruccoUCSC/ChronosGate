using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class BoardManager : MonoBehaviour
{
    private class UnitRespawnRecord
    {
        public Vector3Int CellPos;
        public UnitInstance Instance;
    }

    private const float TILE_FILL_RATIO = 1.0f;

    // I hade to change this to public get so other scripts can access the tilemap
    public Tilemap GameTilemap { get; private set; }
    public TileMapManager TileMapManager;
    public bool occupied;
    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    public BaseUnit[,] unitGrid;

    // dictionary to keep track of occupied tiles
    // this is way faster than looping through every object on the board
    public List<BaseUnit> unitList = new List<BaseUnit>();
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();
    private readonly Dictionary<Vector3Int, UnitRespawnRecord> m_RespawnRoster = new Dictionary<Vector3Int, UnitRespawnRecord>();
    private readonly HashSet<Vector3Int> m_DeathBlockedCells = new HashSet<Vector3Int>();

    public Transform UnitsParent;
    public Transform EnemyParent;

    // I switched this to Awake so other scripts can access the tilemap in their Start methods
    // This is a super high priority initialization that needs to be done as early as possible
    private void Awake()
    {
        GameTilemap = GetComponentInChildren<Tilemap>();
        if (TileMapManager != null)
        {
            TileMapManager.board = this;
            Width = TileMapManager.Width;
            Height = TileMapManager.Height;
        }

        // unitGrid first index is X (width), second is Y (height)
        unitGrid = new BaseUnit[Width, Height];
        // Auto-create buckets if you forgot to make them in Editor
        if (UnitsParent == null)
        {
            UnitsParent = new GameObject("Units").transform;
            UnitsParent.SetParent(this.transform);
        }
        if (EnemyParent == null)
        {
            EnemyParent = new GameObject("Enemies").transform;
            EnemyParent.SetParent(this.transform);
        
        }

        GenerateBoard();
        CenterCamera();
    }
    public void GenerateBoard()
    {
        int newWidth = Width;
        int newHeight = Height;
        if (TileMapManager != null)
        {
            newWidth = TileMapManager.Width;
            newHeight = TileMapManager.Height;
            TileMapManager.board = this;
        }

        if (GameTilemap == null)
        {
            GameTilemap = GetComponentInChildren<Tilemap>();
        }

        if (GameTilemap == null)
        {
            Debug.LogError("BoardManager: No Tilemap found to generate the board.");
            return;
        }

        HandleUnitsAffectedByResize(newWidth, newHeight);

        Width = newWidth;
        Height = newHeight;
        GameTilemap.ClearAllTiles();
        unitGrid = new BaseUnit[Width, Height];
        RebuildUnitGridFromOccupancy();

        for (int y = 0; y < Height; ++y)
        {
            int index = y % 2;
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
              
                if  (y == 0 || y == Height - 1)
                {
                tile = WallTiles[Random.Range(0, WallTiles.Length)];
                }
                else
                {
                     tile = GroundTiles[index];
                    if (index == 1)
                    {
                        index=0;
                    }
                    else
                    {
                        index=1;
                    }
                }
              
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                GameTilemap.SetTile(cellPos, tile);
                NormalizeTileSize(cellPos, tile);
            }
        }

        CenterCamera();
    }

    private void HandleUnitsAffectedByResize(int newWidth, int newHeight)
    {
        if (occupiedTiles.Count == 0)
        {
            return;
        }

        InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
        List<Vector3Int> cellsToRemove = new List<Vector3Int>();

        foreach (KeyValuePair<Vector3Int, GameObject> entry in occupiedTiles)
        {
            Vector3Int cellPos = entry.Key;
            GameObject unitObject = entry.Value;
            if (unitObject == null)
            {
                cellsToRemove.Add(cellPos);
                continue;
            }

            if (IsCellValidAfterResize(cellPos, newWidth, newHeight))
            {
                continue;
            }

            cellsToRemove.Add(cellPos);
            EvictUnitFromBoard(unitObject, inventory);
        }

        for (int i = 0; i < cellsToRemove.Count; i++)
        {
            occupiedTiles.Remove(cellsToRemove[i]);
        }
    }

    private bool IsCellValidAfterResize(Vector3Int cellPos, int newWidth, int newHeight)
    {
        if (cellPos.x < 0 || cellPos.x >= newWidth || cellPos.y < 0 || cellPos.y >= newHeight)
        {
            return false;
        }

        return cellPos.y != 0 && cellPos.y != newHeight - 1;
    }

    private void EvictUnitFromBoard(GameObject unitObject, InventoryUI inventory)
    {
        BaseUnit baseUnit = unitObject.GetComponent<BaseUnit>();
        UnitDefinition definition = baseUnit != null && baseUnit.myData != null ? baseUnit.myData.BaseDef : null;
        if (baseUnit != null && TryGetUnitCell(unitObject, out Vector3Int cellPos))
        {
            RemoveRespawnRecord(cellPos);
        }

        bool movedToInventory = inventory != null && definition != null && inventory.AddUnit(definition);
        if (!movedToInventory)
        {
            Destroy(unitObject);
            return;
        }

        unitObject.SetActive(false);
        Destroy(unitObject);
    }

    private void RebuildUnitGridFromOccupancy()
    {
        List<Vector3Int> invalidCells = new List<Vector3Int>();
        unitList.Clear();

        foreach (KeyValuePair<Vector3Int, GameObject> entry in occupiedTiles)
        {
            Vector3Int cellPos = entry.Key;
            GameObject unitObject = entry.Value;
            if (unitObject == null)
            {
                invalidCells.Add(cellPos);
                continue;
            }

            if (cellPos.x < 0 || cellPos.x >= Width || cellPos.y < 0 || cellPos.y >= Height)
            {
                invalidCells.Add(cellPos);
                continue;
            }

            BaseUnit baseUnit = unitObject.GetComponent<BaseUnit>();
            if (baseUnit == null)
            {
                invalidCells.Add(cellPos);
                continue;
            }

            unitGrid[cellPos.x, cellPos.y] = baseUnit;
            unitList.Add(baseUnit);
            unitObject.transform.position = GameTilemap.GetCellCenterWorld(cellPos);
            unitObject.transform.SetParent(UnitsParent);
        }

        for (int i = 0; i < invalidCells.Count; i++)
        {
            occupiedTiles.Remove(invalidCells[i]);
        }
    }

    private void NormalizeTileSize(Vector3Int cellPos, Tile tile)
    {
        if (tile == null || tile.sprite == null)
        {
            return;
        }

        Vector3 spriteSize = tile.sprite.bounds.size;
        float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
        if (maxDimension <= 0f)
        {
            return;
        }

        float scaleFactor = TILE_FILL_RATIO / maxDimension;
        GameTilemap.SetTileFlags(cellPos, TileFlags.None);
        GameTilemap.SetTransformMatrix(cellPos, Matrix4x4.Scale(Vector3.one * scaleFactor));
    }

    void UpdateBoard()
    {
        

    }
    // Helper function to check if a specific cell is valid for placement
    // I called it isWalkable cause enmies will need this too
    public bool IsWalkable(Vector3Int cellPos)
    {
        if (cellPos.x < 0 || cellPos.x >= Width || cellPos.y < 0 || cellPos.y >= Height)
        {
            return false;
        }

        // Check if a tile exists there
        if (!GameTilemap.HasTile(cellPos)) return false;

        // Top and bottom rows remain blocked.
        if (cellPos.y == 0 || cellPos.y == Height - 1)
            return false;

        // check if the tiled is occupied someone already here?
        if (occupiedTiles.ContainsKey(cellPos))
        {
            // Optional: Check if the unit is dead (null) and remove it
            if (occupiedTiles[cellPos] == null)
            {
                occupiedTiles.Remove(cellPos);
                return true;
            }
            return false;
        }

        if (m_DeathBlockedCells.Contains(cellPos))
        {
            return false;
        }

        return true;
    }

    public void RegisterUnit(Vector3Int cellPos, GameObject unit)
    {
        m_DeathBlockedCells.Remove(cellPos);
        occupiedTiles[cellPos] = unit;
        if (unit.TryGetComponent(out BaseUnit baseUnit))
        {
            unitGrid[cellPos.x, cellPos.y] = baseUnit;
            unitList.Add(baseUnit);
            UpdateRespawnRecord(cellPos, baseUnit);
        }
        unit.transform.SetParent(UnitsParent);
    }
    // Helper function to find the cell position of a given unit
    public bool TryGetUnitCell(GameObject unit, out Vector3Int cellPos)
    {
        foreach (var entry in occupiedTiles)
        {
            if (entry.Value == unit)
            {
                cellPos = entry.Key;
                return true;
            }
        }

        cellPos = default;
        return false;
    }

    //Move unit from one tile to another, returns true if successful
    public bool MoveUnit(GameObject unit, Vector3Int toCell)
    {
        if (unit == null) return false;
        if (!TryGetUnitCell(unit, out Vector3Int fromCell)) return false;
        if (!IsWalkable(toCell)) return false;

        occupiedTiles.Remove(fromCell);
        occupiedTiles[toCell] = unit;

        if (unit.TryGetComponent(out BaseUnit baseUnit))
        {
            unitGrid[fromCell.x, fromCell.y] = null;
            unitGrid[toCell.x, toCell.y] = baseUnit;
        }

        unit.transform.position = GameTilemap.GetCellCenterWorld(toCell);
        return true;
    }

    public bool TryGetUnitAtCell(Vector3Int cellPos, out BaseUnit unit)
    {
        unit = null;

        if (cellPos.x < 0 || cellPos.x >= Width || cellPos.y < 0 || cellPos.y >= Height)
        {
            return false;
        }

        BaseUnit found = unitGrid[cellPos.x, cellPos.y];
        if (found == null)
        {
            return false;
        }

        unit = found;
        return true;
    }

    public bool RemoveUnit(BaseUnit unit)
    {
        if (unit == null) return false;
        if (!TryGetUnitCell(unit.gameObject, out Vector3Int cellPos)) return false;

        occupiedTiles.Remove(cellPos);
        RemoveRespawnRecord(cellPos);

        if (cellPos.x >= 0 && cellPos.x < Width && cellPos.y >= 0 && cellPos.y < Height)
        {
            unitGrid[cellPos.x, cellPos.y] = null;
        }

        unitList.Remove(unit);
        Destroy(unit.gameObject);
        return true;
    }

    public void MarkUnitDefeated(BaseUnit unit)
    {
        if (unit == null)
        {
            return;
        }

        if (!TryGetUnitCell(unit.gameObject, out Vector3Int cellPos))
        {
            return;
        }

        occupiedTiles.Remove(cellPos);

        if (cellPos.x >= 0 && cellPos.x < Width && cellPos.y >= 0 && cellPos.y < Height)
        {
            unitGrid[cellPos.x, cellPos.y] = null;
        }

        unitList.Remove(unit);
        m_DeathBlockedCells.Add(cellPos);
    }

    public void ClearDeathBlockedCells()
    {
        m_DeathBlockedCells.Clear();
    }

    public void RefreshRespawnRecord(BaseUnit unit)
    {
        if (unit == null)
        {
            return;
        }

        if (!TryGetUnitCell(unit.gameObject, out Vector3Int cellPos))
        {
            return;
        }

        UpdateRespawnRecord(cellPos, unit);
    }

    public void RestoreRespawnRoster()
    {
        if (GameTilemap == null || m_RespawnRoster.Count == 0)
        {
            return;
        }

        List<UnitRespawnRecord> records = new List<UnitRespawnRecord>(m_RespawnRoster.Values);
        for (int i = 0; i < records.Count; i++)
        {
            UnitRespawnRecord record = records[i];
            if (record == null || record.Instance == null || record.Instance.BaseDef == null)
            {
                continue;
            }

            Vector3Int cellPos = record.CellPos;
            if (!IsValidRespawnCell(cellPos))
            {
                continue;
            }

            if (TryGetUnitAtCell(cellPos, out BaseUnit existingUnit) && existingUnit != null)
            {
                continue;
            }

            if (occupiedTiles.TryGetValue(cellPos, out GameObject occupiedObject) && occupiedObject != null)
            {
                continue;
            }

            SpawnUnitFromRecord(record);
        }
    }

    private bool IsValidRespawnCell(Vector3Int cellPos)
    {
        if (cellPos.x < 0 || cellPos.x >= Width || cellPos.y < 0 || cellPos.y >= Height)
        {
            return false;
        }

        if (cellPos.y == 0 || cellPos.y == Height - 1)
        {
            return false;
        }

        return GameTilemap.HasTile(cellPos);
    }

    private void SpawnUnitFromRecord(UnitRespawnRecord record)
    {
        GameObject prefab = Resources.Load<GameObject>(record.Instance.BaseDef.PrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[BoardManager] Could not reload prefab for respawned unit '{record.Instance.BaseDef.UnitID}'.");
            return;
        }

        Vector3 spawnPos = GameTilemap.GetCellCenterWorld(record.CellPos);
        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        BaseUnit baseUnit = go.GetComponent<BaseUnit>();
        if (baseUnit == null)
        {
            Debug.LogWarning($"[BoardManager] Respawned prefab '{record.Instance.BaseDef.UnitID}' is missing BaseUnit.");
            Destroy(go);
            return;
        }

        UnitInstance clonedInstance = UnitInstance.CloneRuntimeInstance(record.Instance);
        baseUnit.Initialize(clonedInstance);
        baseUnit.ResetHealth();
        baseUnit.ResetMana();
        RegisterUnit(record.CellPos, go);
    }

    private void UpdateRespawnRecord(Vector3Int cellPos, BaseUnit unit)
    {
        if (unit == null || unit.myData == null)
        {
            return;
        }

        m_RespawnRoster[cellPos] = new UnitRespawnRecord
        {
            CellPos = cellPos,
            Instance = UnitInstance.CloneRuntimeInstance(unit.myData)
        };
    }

    private void RemoveRespawnRecord(Vector3Int cellPos)
    {
        m_RespawnRoster.Remove(cellPos);
    }

    void CenterCamera()
    {
        // 1. Get the Board's current starting position (e.g., if you moved it to 100, 100)
        float startX = transform.position.x;
        float startY = transform.position.y;

        // 2. Add half the width/height to that starting point
        Vector3 boardCenter = new Vector3(startX + (Width / 2f), startY + (Height / 2f), -10);

        Camera.main.transform.position = boardCenter;
        Camera.main.orthographicSize = (Height / 2f) + 2f;
    }
}
