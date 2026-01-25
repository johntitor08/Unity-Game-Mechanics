using UnityEngine;
using System.Collections.Generic;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    private readonly Dictionary<CurrencyType, Currency> currencyDict = new();
    public event Action<CurrencyType, int, int> OnCurrencyChanged;
    public event Action<CurrencyType, int> OnCurrencyAdded;
    public event Action<CurrencyType, int> OnCurrencySpent;
    public event Action<CurrencyType> OnInsufficientFunds;

    [Serializable]
    public class Currency
    {
        public CurrencyType type;
        public int amount = 0;
        public int maxAmount = 999_999;
        public Sprite icon;
        public Color displayColor = Color.yellow;

        public Currency(CurrencyType type, int startAmount = 0)
        {
            this.type = type;
            this.amount = startAmount;
        }
    }

    [Header("Currency Configuration")]
    public List<Currency> currencies = new()
    {
        new Currency(CurrencyType.Gold, 100),
        new Currency(CurrencyType.Gems, 0),
        new Currency(CurrencyType.Tokens, 0),
        new Currency(CurrencyType.Credits, 0)
    };

    [Header("Sound Effects")]
    public AudioClip gainSound;
    public AudioClip spendSound;
    public AudioClip insufficientFundsSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeCurrencies();
    }

    void OnEnable()
    {
        if (Instance == this)
            InitializeCurrencies();
    }

    private void InitializeCurrencies()
    {
        currencyDict.Clear();

        foreach (var currency in currencies)
        {
            if (!currencyDict.ContainsKey(currency.type))
                currencyDict.Add(currency.type, currency);
        }
    }

    public int Get(CurrencyType type)
    {
        return currencyDict.TryGetValue(type, out var currency)
            ? currency.amount
            : 0;
    }

    public bool Has(CurrencyType type, int amount)
    {
        return Get(type) >= amount;
    }

    public Currency GetCurrencyInfo(CurrencyType type)
    {
        return currencyDict.TryGetValue(type, out var currency) ? currency : null;
    }

    public Dictionary<CurrencyType, int> GetAllCurrencies()
    {
        Dictionary<CurrencyType, int> result = new();

        foreach (var c in currencies)
            result[c.type] = c.amount;

        return result;
    }

    public void AddMultiple(Dictionary<CurrencyType, int> amounts, bool showNotification = true)
    {
        if (amounts == null || amounts.Count == 0) return;
        Dictionary<CurrencyType, int> changes = new();

        foreach (var kvp in amounts)
        {
            if (!currencyDict.TryGetValue(kvp.Key, out var currency)) continue;
            if (kvp.Value <= 0) continue;
            int old = currency.amount;
            currency.amount = Mathf.Min(currency.amount + kvp.Value, currency.maxAmount);
            int delta = currency.amount - old;

            if (delta > 0)
            {
                changes[kvp.Key] = delta;
                OnCurrencyAdded?.Invoke(kvp.Key, delta);
                OnCurrencyChanged?.Invoke(kvp.Key, old, currency.amount);
            }
        }

        FinalizeTransaction(changes, true, showNotification);
    }

    public bool SpendMultiple(Dictionary<CurrencyType, int> amounts, bool showNotification = true)
    {
        if (amounts == null || amounts.Count == 0) return false;

        foreach (var kvp in amounts)
        {
            if (!Has(kvp.Key, kvp.Value))
            {
                OnInsufficientFunds?.Invoke(kvp.Key);
                PlaySound(insufficientFundsSound);
                return false;
            }
        }

        Dictionary<CurrencyType, int> changes = new();

        foreach (var kvp in amounts)
        {
            var currency = currencyDict[kvp.Key];
            int old = currency.amount;
            currency.amount -= kvp.Value;
            changes[kvp.Key] = -kvp.Value;
            OnCurrencySpent?.Invoke(kvp.Key, kvp.Value);
            OnCurrencyChanged?.Invoke(kvp.Key, old, currency.amount);
        }

        FinalizeTransaction(changes, false, showNotification);
        return true;
    }

    public void Add(CurrencyType type, int amount, bool showNotification = true)
    {
        AddMultiple(new Dictionary<CurrencyType, int> { { type, amount } }, showNotification);
    }

    public bool Spend(CurrencyType type, int amount, bool showNotification = true)
    {
        return SpendMultiple(new Dictionary<CurrencyType, int> { { type, amount } }, showNotification);
    }

    public void Set(CurrencyType type, int amount)
    {
        if (!currencyDict.TryGetValue(type, out var currency)) return;
        int old = currency.amount;
        currency.amount = Mathf.Clamp(amount, 0, currency.maxAmount);

        if (old != currency.amount)
        {
            OnCurrencyChanged?.Invoke(type, old, currency.amount);
            SaveSystem.SaveGame();
        }
    }

    public void ResetAllCurrencies()
    {
        foreach (var currency in currencies)
            Set(currency.type, 0);
    }

    public void GrantReward(MultiCurrencyReward reward)
    {
        reward?.GrantAll(true);
    }

    private void FinalizeTransaction(
        Dictionary<CurrencyType, int> changes,
        bool isGain,
        bool showNotification)
    {
        if (changes.Count == 0) return;

        if (showNotification && CurrencyNotificationUI.Instance != null)
            CurrencyNotificationUI.Instance.Show(changes);

        PlaySound(isGain ? gainSound : spendSound);
        SaveSystem.SaveGame();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
    }

}
