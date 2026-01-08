using UnityEngine;

[System.Serializable]
public class CurrencyShopItem
{
    public string itemName;
    public string description;
    public Sprite icon;

    public CurrencyType costType;
    public int cost;

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

        // Check if can afford
        if (!CurrencyManager.Instance.Has(item.costType, item.cost))
        {
            return false;
        }

        // Process purchase
        if (CurrencyManager.Instance.Spend(item.costType, item.cost))
        {
            CurrencyManager.Instance.Add(item.rewardType, item.rewardAmount);
            Debug.Log($"Purchased: {item.itemName}");
            return true;
        }

        return false;
    }
}
