using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private int currency = 0;
    public int startingCurrency = 10;
    public int interestThreshold = 10;
    public int maxInterest = 10;
    public int income = 8;

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
        currency = startingCurrency;
    }

    private void OnEnable()
    {
        CurrencyPickup.Collected += HandleCurrencyCollected;
    }

    private void OnDisable()
    {
        CurrencyPickup.Collected -= HandleCurrencyCollected;
    }

    public int GetCurrency() => currency;

    public void AddCurrency(int amount)
    {
        currency += amount;
        OnCurrencyChanged?.Invoke(currency);
    }

    public void AddCurrency(int amount, Vector3 worldOrigin)
    {
        AddCurrency(amount);

        if (amount > 0)
        {
            CurrencyUI.Instance?.PlayCurrencyGainAnimation(worldOrigin, amount);
        }
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
