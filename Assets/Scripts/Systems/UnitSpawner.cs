using UnityEngine;
using UnityEngine.InputSystem;

// example unit spanwer that spawns archers on click
// this can be adapted later to be part of our reroll ui
public class UnitSpawner : MonoBehaviour
{
    public DatabaseLoader database;
    public string currentSelectedID = "archer_01";

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (database.UnitLookup.TryGetValue(currentSelectedID, out UnitDefinition def))
            {
                // 1. create unique runtime instance
                UnitInstance newInstance = UnitInstance.CreateRuntimeInstance(def);

                // 2. create the prefab
                GameObject prefab = Resources.Load<GameObject>(def.PrefabPath);
                GameObject go = Instantiate(prefab, GetMouseWorldPos(), Quaternion.identity);

                // 3. link the two together
                go.GetComponent<BaseUnit>().Initialize(newInstance);
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0);
    }
}