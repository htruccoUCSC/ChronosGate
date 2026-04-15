using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private int currency = 0;
    public int startingCurrency = 3;
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

        // InterestMult scales the interest payout (e.g. 0.5 = half interest).
        // Only applied when interest is not fully suppressed by NoInterest.
        RoundModifierContext ctx = RoundModifierContext.Instance;
        if (ctx != null && ctx.InterestMult != 1f)
            addAmount = Mathf.RoundToInt(addAmount * ctx.InterestMult);

        AddCurrency(addAmount);
    }

    public void newRound()
    {
        // NoInterest completely suppresses interest for this round (e.g. Recession modifier).
        // InterestMult fine-tunes the payout when interest is not fully blocked.
        RoundModifierContext ctx = RoundModifierContext.Instance;
        if (ctx == null || !ctx.NoInterest)
            GetInterest();

        // IncomeMult scales the flat gold income awarded each wave.
        float incomeMult = ctx != null ? ctx.IncomeMult : 1f;
        AddCurrency(Mathf.RoundToInt(income * incomeMult));
    }

    private void HandleCurrencyCollected(int amount)
    {
        AddCurrency(amount);
    }
}
