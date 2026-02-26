using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    public BoardManager board;

    // CHANGED: Now accepts a Definition, creates the Instance internally
    public bool TrySpawnFromInventory(UnitDefinition def)
    {
        // Defensive guards so we don't crash and we get actionable console errors
        if (def == null)
        {
            Debug.LogError("[UnitSpawner] TrySpawnFromInventory called with null UnitDefinition.");
            return false;
        }

        if (board == null)
        {
            Debug.LogError($"[UnitSpawner] Board reference is null. Cannot spawn '{def.UnitID}'.");
            return false;
        }

        if (board.GameTilemap == null)
        {
            Debug.LogError($"[UnitSpawner] board.GameTilemap is null. Cannot spawn '{def.UnitID}'.");
            return false;
        }

        if (Camera.main == null)
        {
            Debug.LogError($"[UnitSpawner] Camera.main is null. Cannot spawn '{def.UnitID}'.");
            return false;
        }

        if (Mouse.current == null)
        {
            Debug.LogError($"[UnitSpawner] Mouse.current is null (Input System?). Cannot spawn '{def.UnitID}'.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(def.PrefabPath))
        {
            Debug.LogError($"[UnitSpawner] '{def.UnitID}' has empty PrefabPath. Check units.json/export.");
            return false;
        }

        Vector3 rawWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        rawWorldPos.z = 0;
        Vector3Int cellPos = board.GameTilemap.WorldToCell(rawWorldPos);

        if (!board.IsWalkable(cellPos))
            return false;

        Vector3 snapPos = board.GameTilemap.GetCellCenterWorld(cellPos);

        // create unique instance for this unit
        UnitInstance newInstance = UnitInstance.CreateRuntimeInstance(def);
        if (newInstance == null)
        {
            Debug.LogError($"[UnitSpawner] Failed to create runtime UnitInstance for '{def.UnitID}'.");
            return false;
        }

        // create visuals
        GameObject prefab = Resources.Load<GameObject>(def.PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[UnitSpawner] Resources.Load failed for '{def.UnitID}'. PrefabPath='{def.PrefabPath}'. " +
                           "Prefab must be under Assets/Resources and PrefabPath must NOT include 'Assets/', 'Resources/', or '.prefab'.");
            return false;
        }

        GameObject go = Instantiate(prefab, snapPos, Quaternion.identity);
        if (go == null)
        {
            Debug.LogError($"[UnitSpawner] Instantiate returned null for '{def.UnitID}'. PrefabPath='{def.PrefabPath}'.");
            return false;
        }

        // add unit to board
        BaseUnit unit = go.GetComponent<BaseUnit>();
        if (unit == null)
        {
            Debug.LogError($"[UnitSpawner] Spawned prefab for '{def.UnitID}' is missing a BaseUnit component. " +
                           $"PrefabPath='{def.PrefabPath}'.");
            Destroy(go);
            return false;
        }

        unit.Initialize(newInstance);
        board.RegisterUnit(cellPos, go);

        return true;
    }
}