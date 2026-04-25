using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private BoardManager m_Board;
    [SerializeField] private float m_DestroyDelay = 0.6f;
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private Color m_PreviewColor = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] private float m_ItemScale = 1.5f;

    private Tile m_PreviewTile;
    private Vector3Int m_LastCenterCell;
    private bool m_HasPreview;
    private bool m_IsPreviewActive;
    private ItemDefinition m_PreviewItem;

    private void Awake()
    {
        if (m_PreviewTilemap == null && m_Board != null)
        {
            GameObject previewObj = new GameObject("ItemPreviewTilemap");
            previewObj.transform.SetParent(m_Board.transform, false);
            m_PreviewTilemap = previewObj.AddComponent<Tilemap>();
            TilemapRenderer renderer = previewObj.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 100;
        }
    }

    private void Update()
    {
        UpdatePreview();
    }
    
    public bool TryPlaceFromInventory(ItemDefinition item)
    {
        if (item == null) return false;

        if (Camera.main == null || Mouse.current == null) return false;

        return TryPlaceAtScreenPosition(item, Mouse.current.position.ReadValue());
    }

    public bool TryPlaceAtScreenPosition(ItemDefinition item, Vector2 screenPosition)
    {
        if (item == null) return false;
        if (m_Board == null || m_Board.GameTilemap == null || Camera.main == null) return false;

        Vector3 rawWorldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        rawWorldPos.z = 0;
        Vector3Int cellPos = m_Board.GameTilemap.WorldToCell(rawWorldPos);
        if (!m_Board.GameTilemap.HasTile(cellPos)) return false;

        Vector3 snapPos = m_Board.GameTilemap.GetCellCenterWorld(cellPos);

        GameObject prefab = Resources.Load<GameObject>(item.PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[ItemSpawner] Could not load prefab at path '{item.PrefabPath}'.");
            return false;
        }

        GameObject placedItem = Instantiate(prefab, snapPos, Quaternion.identity);
        placedItem.transform.SetParent(m_Board.transform);
        placedItem.transform.localScale = Vector3.one * m_ItemScale;

        StartCoroutine(DelayedItemEffect(item, snapPos, placedItem));

        return true;
    }

    public void SetPreviewItem(ItemDefinition item)
    {
        m_PreviewItem = item;
        m_IsPreviewActive = item != null
            && item.EffectType == ItemEffectType.AreaDamage
            && item.DamageValue > 0;

        if (!m_IsPreviewActive)
        {
            ClearPreview();
        }
    }

    private void UpdatePreview()
    {
        if (!m_IsPreviewActive || m_PreviewItem == null)
        {
            ClearPreview();
            return;
        }

        if (m_PreviewTilemap == null || m_Board == null || m_Board.GameTilemap == null)
        {
            ClearPreview();
            return;
        }

        if (Camera.main == null || Mouse.current == null)
        {
            ClearPreview();
            return;
        }

        Vector3 rawWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        rawWorldPos.z = 0;
        Vector3Int cellPos = m_Board.GameTilemap.WorldToCell(rawWorldPos);

        if (!m_Board.GameTilemap.HasTile(cellPos))
        {
            ClearPreview();
            return;
        }

        if (m_HasPreview && cellPos == m_LastCenterCell) return;

        ClearPreview();
        EnsurePreviewTile();

        int radius = GetPreviewRadiusInTiles(m_PreviewItem);
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector3Int previewCell = new Vector3Int(cellPos.x + x, cellPos.y + y, 0);
                if (!m_Board.GameTilemap.HasTile(previewCell)) continue;
                m_PreviewTilemap.SetTile(previewCell, m_PreviewTile);
                m_PreviewTilemap.SetTileFlags(previewCell, TileFlags.None);
                m_PreviewTilemap.SetColor(previewCell, m_PreviewColor);
            }
        }

        m_LastCenterCell = cellPos;
        m_HasPreview = true;
    }

    private void ClearPreview()
    {
        if (!m_HasPreview || m_PreviewTilemap == null) return;

        int radius = m_PreviewItem != null ? GetPreviewRadiusInTiles(m_PreviewItem) : 1;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector3Int previewCell = new Vector3Int(m_LastCenterCell.x + x, m_LastCenterCell.y + y, 0);
                m_PreviewTilemap.SetTile(previewCell, null);
            }
        }

        m_HasPreview = false;
    }

    private void EnsurePreviewTile()
    {
        if (m_PreviewTile != null) return;

        m_PreviewTile = ScriptableObject.CreateInstance<Tile>();
        m_PreviewTile.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            Texture2D.whiteTexture.width);
    }

    private void ApplyItemEffect(ItemDefinition item, Vector3 worldCenter)
    {
        if (item == null) return;

        switch (item.EffectType)
        {
            case ItemEffectType.AreaDamage:
                ApplyAreaDamage(item, worldCenter);
                break;
            case ItemEffectType.TowerLevelUp:
                ApplyTowerLevelUp(item, worldCenter);
                break;
            case ItemEffectType.TowerPoolRestock:
                ApplyTowerPoolRestock(item, worldCenter);
                break;
            case ItemEffectType.None:
            default:
                break;
        }
    }

    private void ApplyAreaDamage(ItemDefinition item, Vector3 worldCenter)
    {
        if (item.DamageValue <= 0) return;

        Vector3 cellSize = m_Board.GameTilemap.cellSize;
        int areaSize = Mathf.Max(1, item.AreaSizeInTiles);
        Vector2 boxSize = new Vector2(cellSize.x * areaSize, cellSize.y * areaSize);

        int maskValue = m_TargetMask.value;
        Collider2D[] hits = maskValue != 0
            ? Physics2D.OverlapBoxAll(worldCenter, boxSize, 0f, m_TargetMask)
            : Physics2D.OverlapBoxAll(worldCenter, boxSize, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            if (hits[i].TryGetComponent(out TargetDummyTest dummy))
            {
                dummy.TakeDamage(item.DamageValue, null);
                continue;
            }

            BaseEnemy enemy = hits[i].GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(null, item.DamageValue);
                continue;
            }

            hits[i].gameObject.SendMessage("TakeDamage", item.DamageValue, SendMessageOptions.DontRequireReceiver);
        }
    }

    private int GetPreviewRadiusInTiles(ItemDefinition item)
    {
        int areaSize = item != null ? Mathf.Max(1, item.AreaSizeInTiles) : 3;
        return Mathf.FloorToInt((areaSize - 1) * 0.5f);
    }

    private void ApplyTowerLevelUp(ItemDefinition item, Vector3 worldCenter)
    {
        BaseUnit closestTower = FindClosestTower(worldCenter);

        // Apply merge upgrade (level up) to the closest tower
        if (closestTower != null)
        {
            // Create a duplicate unit definition to trigger the merge upgrade
            closestTower.TryApplyMergeUpgrade(closestTower.myData.BaseDef);
            Debug.Log($"Leveled up tower: {closestTower.myData.Name} to level {closestTower.myData.Level}");
        }
        else
        {
            Debug.LogWarning("Could not find a tower to level up.");
        }
    }

    private void ApplyTowerPoolRestock(ItemDefinition item, Vector3 worldCenter)
    {
        BaseUnit closestTower = FindClosestTower(worldCenter);
        if (closestTower == null || closestTower.myData == null || closestTower.myData.BaseDef == null)
        {
            Debug.LogWarning("Could not find a tower to restock in the pool.");
            return;
        }

        UnitDefinition towerDefinition = closestTower.myData.BaseDef;

        ShopManager shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager != null)
        {
            shopManager.AddTowerPoolCopies(towerDefinition, 1);
            return;
        }

        ShopManagerOld shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        if (shopManagerOld != null)
        {
            shopManagerOld.AddTowerPoolCopies(towerDefinition, 1);
            return;
        }

        Debug.LogWarning("Could not restock tower pool because no shop manager was found.");
    }

    private static BaseUnit FindClosestTower(Vector3 worldCenter)
    {
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        if (allUnits == null || allUnits.Length == 0)
        {
            return null;
        }

        BaseUnit closestTower = null;
        float closestDistance = Mathf.Infinity;

        foreach (BaseUnit unit in allUnits)
        {
            if (unit == null || unit.myData == null)
            {
                continue;
            }

            float distance = Vector3.Distance(unit.transform.position, worldCenter);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = unit;
            }
        }

        return closestTower;
    }

    private IEnumerator DelayedItemEffect(ItemDefinition item, Vector3 worldCenter, GameObject placedItem)
    {
        if (m_DestroyDelay > 0f)
        {
            yield return new WaitForSeconds(m_DestroyDelay);
        }

        ApplyItemEffect(item, worldCenter);

        if (placedItem != null)
        {
            Destroy(placedItem);
        }
    }

}
