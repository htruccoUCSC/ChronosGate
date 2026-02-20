using UnityEngine;

public class Generator : BaseUnit
{
    [SerializeField] private CurrencyPickup currencyPickupPrefab;
    [SerializeField] private int baseCurrencyAmount = 1;
    [SerializeField] private float spawnRadius = 0.4f;

    protected override void ScanTargeting()
    {
        currentTarget = transform;
    }

    protected override void CastAbility()
    {
        if (currencyPickupPrefab == null)
        {
            Debug.LogWarning("Generator has no currency pickup prefab assigned.");
            return;
        }

        int amount = Mathf.Max(1, Mathf.RoundToInt(baseCurrencyAmount + myData.GetModifiedAbilityPower()));
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        CurrencyPickup pickup = Instantiate(currencyPickupPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        pickup.Configure(amount);
    }
}
