using UnityEngine;

public class Farmer : BaseUnit
{
    // [SerializeField] private CurrencyPickup m_CurrencyPickupPrefab;
    [SerializeField] private int m_BaseCurrencyAmount = 1;
    [SerializeField] private ItemDefinition m_OnionItemDefinition;

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
        int amount = Mathf.Max(1, Mathf.RoundToInt(m_BaseCurrencyAmount + myData.GetModifiedAbilityPower())/40);
        CoinDropSpawner.Spawn(transform.position, amount);
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
