using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    public DatabaseLoader database;
    public BoardManager board; // Reference to the new script

    public string currentSelectedID = "archer_01";

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SpawnUnitOnTile();
        }
    }

    void SpawnUnitOnTile()
    {
        // get raw mouse position in world space
        Vector3 rawWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // get the cell position from the tilemap
        Vector3Int cellPos = board.GameTilemap.WorldToCell(rawWorldPos);

        // chceck if the cell is walkable
        if (board.IsWalkable(cellPos))
        {
            // get the exact center position of the cell from the tilemap
            Vector3 snapPos = board.GameTilemap.GetCellCenterWorld(cellPos);

            if (database.UnitLookup.TryGetValue(currentSelectedID, out UnitDefinition def))
            {
                // Create Data
                UnitInstance newInstance = UnitInstance.CreateRuntimeInstance(def);

                // Create Visual
                GameObject prefab = Resources.Load<GameObject>(def.PrefabPath);
                GameObject go = Instantiate(prefab, snapPos, Quaternion.identity);

                // Initialize
                go.GetComponent<BaseUnit>().Initialize(newInstance);

                board.RegisterUnit(cellPos, go);

                Debug.Log($"Spawned unit at Grid: {cellPos}");
            }
        }
        else
        {
            Debug.Log("Cannot place unit here (Wall or Empty).");
        }
    }
}