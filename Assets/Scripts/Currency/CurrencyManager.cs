using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public int currency = 5;
    public int maxInterest = 10;
    public int interestThreshold = 10;
    public int income = 8;

    private void OnEnable()
    {
        CurrencyPickup.Collected += HandleCurrencyCollected;
    }

    private void OnDisable()
    {
        CurrencyPickup.Collected -= HandleCurrencyCollected;
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
    }

    public void SubtractCurrency(int amount)
    {
        currency -= amount;
    }

    public void SetCurrency(int newAmount)
    {
        currency = newAmount;

    }

    public void GetInterest()
    {
        int addAmount = currency;
        if (addAmount > maxInterest)
        {
            addAmount = maxInterest;
        }
        AddCurrency(addAmount);
    }
public void newRound()
    {
        GetInterest();
        AddCurrency(income);
    }

    private void HandleCurrencyCollected(int amount)
    {
        AddCurrency(amount);
    }
}
