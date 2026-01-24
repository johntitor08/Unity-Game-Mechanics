using UnityEngine;
using System.Collections.Generic;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

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
    public AudioClip coinSound;
    public AudioClip purchaseSound;
    public AudioClip insufficientFundsSound;

    private readonly Dictionary<CurrencyType, Currency> currencyDict = new();
    public event Action<CurrencyType, int, int> OnCurrencyChanged;
    public event Action<CurrencyType, int> OnCurrencyAdded;
    public event Action<CurrencyType, int> OnCurrencySpent;
    public event Action<CurrencyType> OnInsufficientFunds;

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

    private void InitializeCurrencies()
    {
        currencyDict.Clear();

        foreach (var currency in currencies)
        {
            currencyDict[currency.type] = currency;
        }
    }

    public int Get(CurrencyType type)
    {
        if (!currencyDict.ContainsKey(type))
        {
            Debug.LogWarning($"Currency type {type} not found!");
            return 0;
        }

        return currencyDict[type].amount;
    }

    public bool Has(CurrencyType type, int amount) => Get(type) >= amount;

    public Currency GetCurrencyInfo(CurrencyType type) => currencyDict.ContainsKey(type) ? currencyDict[type] : null;

    public Dictionary<CurrencyType, int> GetAllCurrencies()
    {
        var result = new Dictionary<CurrencyType, int>();

        foreach (var currency in currencies)
        {
            result[currency.type] = currency.amount;
        }

        return result;
    }

    public void AddMultiple(Dictionary<CurrencyType, int> amounts, bool showNotification = true)
    {
        if (amounts == null || amounts.Count == 0) return;
        var actualChanges = new Dictionary<CurrencyType, int>();

        foreach (var kvp in amounts)
        {
            if (!currencyDict.ContainsKey(kvp.Key) || kvp.Value <= 0) continue;
            var currency = currencyDict[kvp.Key];
            int oldAmount = currency.amount;
            currency.amount = Mathf.Min(currency.amount + kvp.Value, currency.maxAmount);
            int actualAdded = currency.amount - oldAmount;

            if (actualAdded > 0)
            {
                actualChanges[kvp.Key] = actualAdded;
                OnCurrencyAdded?.Invoke(kvp.Key, actualAdded);
                OnCurrencyChanged?.Invoke(kvp.Key, oldAmount, currency.amount);
            }
        }

        if (actualChanges.Count > 0)
        {
            if (showNotification && CurrencyNotificationUI.Instance != null)
                CurrencyNotificationUI.Instance.Show(actualChanges);

            foreach (var kvp in actualChanges)
            {
                if (showNotification)
                    ShowCurrencyNotification(kvp.Key, kvp.Value, true);
            }

            PlaySound(coinSound);
            SaveSystem.SaveGame();
        }
    }

    public bool SpendMultiple(Dictionary<CurrencyType, int> amounts, bool showNotification = true)
    {
        if (amounts == null || amounts.Count == 0) return false;

        // Ensure all currencies are available
        foreach (var kvp in amounts)
        {
            if (!Has(kvp.Key, kvp.Value))
            {
                OnInsufficientFunds?.Invoke(kvp.Key);
                if (showNotification) ShowInsufficientFundsNotification(kvp.Key, kvp.Value);
                PlaySound(insufficientFundsSound);
                Debug.Log($"Insufficient {kvp.Key} for multi-currency transaction!");
                return false;
            }
        }

        var actualChanges = new Dictionary<CurrencyType, int>();

        // Deduct
        foreach (var kvp in amounts)
        {
            var currency = currencyDict[kvp.Key];
            int oldAmount = currency.amount;
            currency.amount -= kvp.Value;
            actualChanges[kvp.Key] = -kvp.Value;
            OnCurrencySpent?.Invoke(kvp.Key, kvp.Value);
            OnCurrencyChanged?.Invoke(kvp.Key, oldAmount, currency.amount);
        }

        if (showNotification && CurrencyNotificationUI.Instance != null)
            CurrencyNotificationUI.Instance.Show(actualChanges);

        foreach (var kvp in actualChanges)
        {
            if (showNotification)
                ShowCurrencyNotification(kvp.Key, Mathf.Abs(kvp.Value), false);
        }

        PlaySound(purchaseSound);
        SaveSystem.SaveGame();
        return true;
    }

    public void Set(CurrencyType type, int amount)
    {
        if (!currencyDict.ContainsKey(type))
        {
            Debug.LogWarning($"Currency type {type} not found!");
            return;
        }

        var currency = currencyDict[type];
        int oldAmount = currency.amount;
        currency.amount = Mathf.Clamp(amount, 0, currency.maxAmount);

        if (oldAmount != currency.amount)
        {
            OnCurrencyChanged?.Invoke(type, oldAmount, currency.amount);
            SaveSystem.SaveGame();
        }
    }

    private void ShowCurrencyNotification(CurrencyType type, int amount, bool isGain)
    {
        if (CurrencyNotificationUI.Instance != null)
        {
            // Each currency gets its own notification
            var changes = new Dictionary<CurrencyType, int>
            {
                { type, isGain ? amount : -amount }
            };

            CurrencyNotificationUI.Instance.Show(changes);
        }
    }

    private void ShowInsufficientFundsNotification(CurrencyType type, int required)
    {
        if (CurrencyNotificationUI.Instance != null)
        {
            CurrencyNotificationUI.Instance.Show(new Dictionary<CurrencyType, int>
            {
                { type, -required }
            });
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
    }

    public void AddAllCurrencies(int amount)
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            Add(type, amount, false);
    }

    public void ResetAllCurrencies()
    {
        foreach (var currency in currencies)
            Set(currency.type, 0);
    }

    public void GrantReward(MultiCurrencyReward reward)
    {
        if (reward == null) return;
        reward.GrantAll(true); // show notifications
    }

    // Simple Add for convenience
    public void Add(CurrencyType type, int amount, bool showNotification = true)
    {
        AddMultiple(new Dictionary<CurrencyType, int> { { type, amount } }, showNotification);
    }

    // Simple Spend for convenience
    public bool Spend(CurrencyType type, int amount, bool showNotification = true)
    {
        return SpendMultiple(new Dictionary<CurrencyType, int> { { type, amount } }, showNotification);
    }
}
