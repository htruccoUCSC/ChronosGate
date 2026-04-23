using UnityEngine;

public class Generator : BaseUnit
{
    // [SerializeField] private CurrencyPickup currencyPickupPrefab;
    protected override void ScanTargeting()
    {
        currentTarget = transform;
    }

    protected override void CastAbility()
    {
        int amount = Mathf.Max(1, Mathf.RoundToInt(myData.GetModifiedAbilityPower())/50);
        CoinDropSpawner.Spawn(transform.position, amount);
    }
}
