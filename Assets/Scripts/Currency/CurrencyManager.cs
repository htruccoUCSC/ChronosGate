using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int currency = 5;
    public int interestThreshold = 10;

    // Event fired when currency changes - UI subscribes to this
    public event Action<int> OnCurrencyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetCurrency() => currency;

    public void AddCurrency(int amount)
    {
        currency += amount;
        OnCurrencyChanged?.Invoke(currency);
    }

    public bool TrySpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            OnCurrencyChanged?.Invoke(currency);
            return true;
        }
        return false;
    }

    public void SubtractCurrency(int amount)
    {
        currency -= amount;
        OnCurrencyChanged?.Invoke(currency);
    }

    public void SetCurrency(int newAmount)
    {
        currency = newAmount;
        OnCurrencyChanged?.Invoke(currency);
    }

    public void GetInterest()
    {
        int addAmount = currency / interestThreshold;
        AddCurrency(addAmount);
    }
}
