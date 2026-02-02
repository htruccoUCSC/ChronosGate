using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class BoardManager : MonoBehaviour
{
    // I hade to change this to public get so other scripts can access the tilemap
    public Tilemap GameTilemap { get; private set; }

    public bool occupied;
    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    // dictionary to keep track of occupied tiles
    // this is way faster than looping through every object on the board
    public List<BaseUnit> unitList = new List<BaseUnit>();
    private Dictionary<Vector3Int, GameObject> occupiedTiles = new Dictionary<Vector3Int, GameObject>();

    public Transform UnitsParent;
    public Transform EnemyParent;

    // I switched this to Awake so other scripts can access the tilemap in their Start methods
    // This is a super high priority initialization that needs to be done as early as possible
    private void Awake()
    {
        GameTilemap = GetComponentInChildren<Tilemap>();
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
    void GenerateBoard()
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
              
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                }
              
                GameTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    // Helper function to check if a specific cell is valid for placement
    // I called it isWalkable cause enmies will need this too
    public bool IsWalkable(Vector3Int cellPos)
    {
        // Check if a tile exists there
        if (!GameTilemap.HasTile(cellPos)) return false;

        // Check if it's not a wall tile (assuming walls are on the border)
        // We can porbably get rid of wall tiles but I didn't want to just start deleting peoples stuff
        if (cellPos.x == 0 || cellPos.y == 0 || cellPos.x == Width - 1 || cellPos.y == Height - 1)
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

        return true;
    }

    public void RegisterUnit(Vector3Int cellPos, GameObject unit)
    {
        occupiedTiles[cellPos] = unit;
        unitList.Add(GetComponent<BaseUnit>());
        unit.transform.SetParent(UnitsParent);
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