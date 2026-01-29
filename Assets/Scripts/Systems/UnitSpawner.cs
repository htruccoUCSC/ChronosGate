using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    public DatabaseLoader database;
    private string selectedID = "archer_01"; // Hardcoded for testing

    void Update()
    {
        // Left Click to Spawn
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SpawnUnit();
        }
    }

    void SpawnUnit()
    {
        // 1. Get Mouse Position
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        // 2. Snap to Grid (Optional)
        worldPos.x = Mathf.Round(worldPos.x);
        worldPos.y = Mathf.Round(worldPos.y);

        // 3. Get Data & Spawn
        if (database.UnitLookup.TryGetValue(selectedID, out UnitData data))
        {
            GameObject prefab = Resources.Load<GameObject>(data.PrefabPath);
            if (prefab != null)
            {
                GameObject newUnit = Instantiate(prefab, worldPos, Quaternion.identity);

                // We get BaseUnit because ArcherUnit inherits from it
                newUnit.GetComponent<BaseUnit>().Initialize(data);

                Debug.Log($"Spawned {data.Name}");
            }
        }
    }
}