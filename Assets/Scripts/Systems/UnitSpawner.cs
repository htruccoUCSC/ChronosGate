using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    public BoardManager board;

    // CHANGED: Now accepts a Definition, creates the Instance internally
    public bool TrySpawnFromInventory(UnitDefinition def)
    {
        Vector3 rawWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        rawWorldPos.z = 0;
        Vector3Int cellPos = board.GameTilemap.WorldToCell(rawWorldPos);

        if (board.IsWalkable(cellPos))
        {
            Vector3 snapPos = board.GameTilemap.GetCellCenterWorld(cellPos);

            // create unique instance for this unit
            UnitInstance newInstance = UnitInstance.CreateRuntimeInstance(def);

            // create visuals
            GameObject prefab = Resources.Load<GameObject>(def.PrefabPath);
            GameObject go = Instantiate(prefab, snapPos, Quaternion.identity);
            
            // add unit to board
            go.GetComponent<BaseUnit>().Initialize(newInstance);
            board.RegisterUnit(cellPos, go);

            return true;
        }

        return false;
    }
}