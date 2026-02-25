using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    public BoardManager board;

    // CHANGED: Now accepts a Definition, creates the Instance internally
    public bool TrySpawnFromInventory(UnitDefinition def)
    {
        if (def == null)
        {
            Debug.LogError("TrySpawnFromInventory: UnitDefinition is null!");
            return false;
        }

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
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab at path: {def.PrefabPath}");
                return false;
            }

            GameObject go = Instantiate(prefab, snapPos, Quaternion.identity);
            
            // add unit to board
            BaseUnit unitComponent = go.GetComponent<BaseUnit>();
            if (unitComponent == null)
            {
                Debug.LogError($"Spawned unit '{go.name}' from prefab '{def.PrefabPath}' does not have a BaseUnit component! Check that the prefab has the correct unit script attached.");
                Destroy(go);
                return false;
            }
            
            unitComponent.Initialize(newInstance);
            board.RegisterUnit(cellPos, go);

            return true;
        }

        return false;
    }
}