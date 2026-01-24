using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CurrencyConversionRate
{
    public CurrencyType fromType;
    public CurrencyType toType;
    public float rate = 1f; // 1 fromType = rate * toType
}

public class CurrencyConverter : MonoBehaviour
{
    [Header("Conversion Settings")]
    public List<CurrencyConversionRate> conversionRates = new();

    private Dictionary<(CurrencyType, CurrencyType), float> rateDict;

    void Awake()
    {
        // Build lookup dictionary for fast conversion
        rateDict = new Dictionary<(CurrencyType, CurrencyType), float>();

        foreach (var rate in conversionRates)
        {
            rateDict[(rate.fromType, rate.toType)] = rate.rate;
        }
    }

    public bool Convert(CurrencyType from, CurrencyType to, int fromAmount)
    {
        if (CurrencyManager.Instance == null || fromAmount <= 0) return false;

        if (!CurrencyManager.Instance.Has(from, fromAmount))
            return false;

        if (!rateDict.TryGetValue((from, to), out float conversionRate))
        {
            Debug.LogWarning($"No conversion rate from {from} to {to}!");
            return false;
        }

        int toAmount = Mathf.FloorToInt(fromAmount * conversionRate);
        var spendDict = new Dictionary<CurrencyType, int> { { from, fromAmount } };
        var addDict = new Dictionary<CurrencyType, int> { { to, toAmount } };

        if (CurrencyManager.Instance.SpendMultiple(spendDict))
        {
            CurrencyManager.Instance.AddMultiple(addDict);
            Debug.Log($"Converted {fromAmount} {from} → {toAmount} {to}");
            return true;
        }

        return false;
    }

    public bool ConvertMultiple(Dictionary<CurrencyType, int> fromAmounts, CurrencyType to, out int totalConverted)
    {
        totalConverted = 0;
        if (CurrencyManager.Instance == null || fromAmounts == null || fromAmounts.Count == 0)
            return false;

        // Check if all currencies have enough
        foreach (var kvp in fromAmounts)
        {
            if (!CurrencyManager.Instance.Has(kvp.Key, kvp.Value))
                return false;
        }

        var spendDict = new Dictionary<CurrencyType, int>();
        var addDict = new Dictionary<CurrencyType, int>();

        // Calculate conversion amounts
        foreach (var kvp in fromAmounts)
        {
            if (!rateDict.TryGetValue((kvp.Key, to), out float rate))
            {
                Debug.LogWarning($"No conversion rate from {kvp.Key} to {to}!");
                return false;
            }

            int converted = Mathf.FloorToInt(kvp.Value * rate);
            spendDict[kvp.Key] = kvp.Value;
            totalConverted += converted;
        }

        addDict[to] = totalConverted;

        // Use multi-currency Spend/Add for notifications
        if (CurrencyManager.Instance.SpendMultiple(spendDict))
        {
            CurrencyManager.Instance.AddMultiple(addDict);
            Debug.Log($"Converted multiple currencies → {totalConverted} {to}");
            return true;
        }

        return false;
    }
}
