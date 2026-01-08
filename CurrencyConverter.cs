using UnityEngine;

public class CurrencyConverter : MonoBehaviour
{
    [Header("Conversion Rates")]
    public CurrencyType fromType = CurrencyType.Gems;
    public CurrencyType toType = CurrencyType.Gold;
    public int conversionRate = 100; // 1 Gem = 100 Gold

    public bool Convert(int fromAmount)
    {
        if (CurrencyManager.Instance == null) return false;

        // Check if has enough
        if (!CurrencyManager.Instance.Has(fromType, fromAmount))
        {
            return false;
        }

        int toAmount = fromAmount * conversionRate;

        // Process conversion
        if (CurrencyManager.Instance.Spend(fromType, fromAmount, false))
        {
            CurrencyManager.Instance.Add(toType, toAmount, false);

            // Show notification
            if (CurrencyNotificationUI.Instance != null)
            {
                CurrencyNotificationUI.Instance.Show(
                    $"Converted {fromAmount} {fromType} → {toAmount} {toType}",
                    Color.cyan);
            }

            Debug.Log($"Converted {fromAmount} {fromType} to {toAmount} {toType}");
            return true;
        }

        return false;
    }
}
