using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private BoardManager m_Board;
    [SerializeField] private float m_DestroyDelay = 0.6f;
    
    public bool TryPlaceFromInventory(ItemDefinition item)
    {
        if (item == null) return false;

        if (Camera.main == null || Mouse.current == null) return false;

        Vector3 rawWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        rawWorldPos.z = 0;
        Vector3Int cellPos = m_Board.GameTilemap.WorldToCell(rawWorldPos);

        if (m_Board.IsWalkable(cellPos))
        {
            Vector3 snapPos = m_Board.GameTilemap.GetCellCenterWorld(cellPos);

            GameObject prefab = Resources.Load<GameObject>(item.PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[ItemSpawner] Could not load prefab at path '{item.PrefabPath}'.");
                return false;
            }

            GameObject placedItem = Instantiate(prefab, snapPos, Quaternion.identity);
            placedItem.transform.SetParent(m_Board.transform);
            Destroy(placedItem, m_DestroyDelay);

            return true;
        }

        return false;
    }

}
