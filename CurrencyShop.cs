using UnityEngine;

[System.Serializable]
public class CurrencyShopItem
{
    public string itemName;
    public string description;
    public Sprite icon;
    public CurrencyType costType;
    public int cost;
    public MultiCurrencyCost[] costs;
    public CurrencyType rewardType;
    public int rewardAmount;
}

public class CurrencyShop : MonoBehaviour
{
    [Header("Shop Items")]
    public CurrencyShopItem[] shopItems;

    public bool PurchaseItem(CurrencyShopItem item)
    {
        if (CurrencyManager.Instance == null) return false;

        // Multi-currency cost dictionary
        var costDict = new System.Collections.Generic.Dictionary<CurrencyType, int>
        {
            { item.costType, item.cost }
        };

        // Multi-currency reward dictionary
        var rewardDict = new System.Collections.Generic.Dictionary<CurrencyType, int>
        {
            { item.rewardType, item.rewardAmount }
        };

        // Try to spend all costs at once
        if (CurrencyManager.Instance.SpendMultiple(costDict))
        {
            // Add rewards
            CurrencyManager.Instance.AddMultiple(rewardDict);
            Debug.Log($"Purchased: {item.itemName}");
            return true;
        }

        // Not enough currency
        Debug.Log($"Cannot purchase {item.itemName}: insufficient funds.");
        return false;
    }
}

[System.Serializable]
public class MultiCurrencyCost
{
    public CurrencyType type;
    public int amount;
}
