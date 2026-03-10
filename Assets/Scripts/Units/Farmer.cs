using UnityEngine;

public class Farmer : BaseUnit
{
    [SerializeField] private CurrencyPickup m_CurrencyPickupPrefab;
    [SerializeField] private int m_BaseCurrencyAmount = 1;
    [SerializeField] private float m_CurrencySpawnRadius = 0.4f;
    [SerializeField] private ItemDefinition m_OnionItemDefinition;
    [SerializeField] private int m_pickupSortingOrder = 1000;

    protected override void ScanTargeting()
    {
        // Farmer doesn't attack enemies, targets itself for currency generation
        currentTarget = transform;
    }

    protected override void CastAbility()
    {
        // Generate currency
        SpawnCurrency();

        // Give player an Onion item
        GivePlayerOnionItem();
    }

    private void SpawnCurrency()
    {
        if (m_CurrencyPickupPrefab == null)
        {
            Debug.LogWarning("Farmer has no currency pickup prefab assigned.");
            return;
        }

        int amount = Mathf.Max(1, Mathf.RoundToInt(m_BaseCurrencyAmount + myData.GetModifiedAbilityPower()));
        Vector2 offset = Random.insideUnitCircle * m_CurrencySpawnRadius;
        CurrencyPickup pickup = Instantiate(m_CurrencyPickupPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        pickup.Configure(amount);

        // Set sorting order to render on top of all units
        SpriteRenderer pickupRenderer = pickup.GetComponent<SpriteRenderer>();
        if (pickupRenderer == null)
        {
            pickupRenderer = pickup.GetComponentInChildren<SpriteRenderer>();
        }

        if (pickupRenderer != null)
        {
            pickupRenderer.sortingOrder = m_pickupSortingOrder;
        }
    }

    private void GivePlayerOnionItem()
    {
        if (m_OnionItemDefinition == null)
        {
            Debug.LogWarning("Farmer has no Onion item definition assigned.");
            return;
        }

        // Find the item inventory and add the item
        ItemInventoryUI itemInventory = FindFirstObjectByType<ItemInventoryUI>();
        if (itemInventory != null)
        {
            if (itemInventory.AddItem(m_OnionItemDefinition))
            {
                Debug.Log("Onion item added to inventory!");
            }
            else
            {
                Debug.LogWarning("Inventory is full, could not add Onion item.");
            }
        }
        else
        {
            Debug.LogWarning("ItemInventoryUI not found in scene.");
        }
    }
}
