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
        if (CurrencyManager.Instance == null)
            return false;

        var costDict = new System.Collections.Generic.Dictionary<CurrencyType, int>();

        if (item.costs != null && item.costs.Length > 0)
        {
            foreach (var cost in item.costs)
                costDict[cost.type] = cost.amount;
        }
        else
        {
            costDict[item.costType] = item.cost;
        }

        var rewardDict = new System.Collections.Generic.Dictionary<CurrencyType, int>{ { item.rewardType, item.rewardAmount } };

        if (CurrencyManager.Instance.SpendMultiple(costDict))
        {
            CurrencyManager.Instance.AddMultiple(rewardDict);
            Debug.Log($"Purchased: {item.itemName}");
            return true;
        }

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
