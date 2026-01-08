using UnityEngine;
using System.Collections.Generic;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [System.Serializable]
    public class Currency
    {
        public CurrencyType type;
        public int amount = 0;
        public int maxAmount = 999999;
        public Sprite icon;
        public Color displayColor = Color.yellow;

        public Currency(CurrencyType type, int startAmount = 0)
        {
            this.type = type;
            this.amount = startAmount;
        }
    }

    [Header("Currency Configuration")]
    public List<Currency> currencies = new List<Currency>
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

    private Dictionary<CurrencyType, Currency> currencyDict = new();

    public event Action<CurrencyType, int, int> OnCurrencyChanged; // (type, oldAmount, newAmount)
    public event Action<CurrencyType, int> OnCurrencyAdded; // (type, amount)
    public event Action<CurrencyType, int> OnCurrencySpent; // (type, amount)
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

    void InitializeCurrencies()
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

    public bool Has(CurrencyType type, int amount)
    {
        return Get(type) >= amount;
    }

    public void Add(CurrencyType type, int amount, bool showNotification = true)
    {
        if (amount <= 0) return;

        if (!currencyDict.ContainsKey(type))
        {
            Debug.LogWarning($"Currency type {type} not found!");
            return;
        }

        Currency currency = currencyDict[type];
        int oldAmount = currency.amount;
        currency.amount = Mathf.Min(currency.amount + amount, currency.maxAmount);
        int actualAdded = currency.amount - oldAmount;

        if (actualAdded > 0)
        {
            OnCurrencyAdded?.Invoke(type, actualAdded);
            OnCurrencyChanged?.Invoke(type, oldAmount, currency.amount);

            if (showNotification)
            {
                ShowCurrencyNotification(type, actualAdded, true);
            }

            PlaySound(coinSound);
            SaveSystem.SaveGame();

            Debug.Log($"Added {actualAdded} {type}. Total: {currency.amount}");
        }
    }

    public bool Spend(CurrencyType type, int amount, bool showNotification = true)
    {
        if (amount <= 0) return false;

        if (!currencyDict.ContainsKey(type))
        {
            Debug.LogWarning($"Currency type {type} not found!");
            return false;
        }

        Currency currency = currencyDict[type];

        if (currency.amount < amount)
        {
            OnInsufficientFunds?.Invoke(type);
            PlaySound(insufficientFundsSound);

            if (showNotification)
            {
                ShowInsufficientFundsNotification(type, amount);
            }

            Debug.Log($"Insufficient {type}! Need {amount}, have {currency.amount}");
            return false;
        }

        int oldAmount = currency.amount;
        currency.amount -= amount;

        OnCurrencySpent?.Invoke(type, amount);
        OnCurrencyChanged?.Invoke(type, oldAmount, currency.amount);

        if (showNotification)
        {
            ShowCurrencyNotification(type, amount, false);
        }

        PlaySound(purchaseSound);
        SaveSystem.SaveGame();

        Debug.Log($"Spent {amount} {type}. Remaining: {currency.amount}");
        return true;
    }

    public void Set(CurrencyType type, int amount)
    {
        if (!currencyDict.ContainsKey(type))
        {
            Debug.LogWarning($"Currency type {type} not found!");
            return;
        }

        Currency currency = currencyDict[type];
        int oldAmount = currency.amount;
        currency.amount = Mathf.Clamp(amount, 0, currency.maxAmount);

        if (oldAmount != currency.amount)
        {
            OnCurrencyChanged?.Invoke(type, oldAmount, currency.amount);
            SaveSystem.SaveGame();
        }
    }

    public Currency GetCurrencyInfo(CurrencyType type)
    {
        return currencyDict.ContainsKey(type) ? currencyDict[type] : null;
    }

    public Dictionary<CurrencyType, int> GetAllCurrencies()
    {
        Dictionary<CurrencyType, int> result = new();
        foreach (var currency in currencies)
        {
            result[currency.type] = currency.amount;
        }
        return result;
    }

    void ShowCurrencyNotification(CurrencyType type, int amount, bool isGain)
    {
        if (CurrencyNotificationUI.Instance != null)
        {
            string prefix = isGain ? "+" : "-";
            CurrencyNotificationUI.Instance.Show($"{prefix}{amount} {type}",
                currencyDict[type].displayColor);
        }
    }

    void ShowInsufficientFundsNotification(CurrencyType type, int required)
    {
        if (CurrencyNotificationUI.Instance != null)
        {
            CurrencyNotificationUI.Instance.Show(
                $"Not enough {type}! (Need {required})",
                Color.red);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
        }
    }

    // Admin/Debug methods
    public void AddAllCurrencies(int amount)
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            Add(type, amount, false);
        }
    }

    public void ResetAllCurrencies()
    {
        foreach (var currency in currencies)
        {
            Set(currency.type, 0);
        }
    }
}
