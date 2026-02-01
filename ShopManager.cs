using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    private readonly Dictionary<string, int> stock = new();

    [Header("Shop Items")]
    public ShopItemData[] shopItems;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDefaultStock();
    }

    void InitializeDefaultStock()
    {
        stock.Clear();

        foreach (var item in shopItems)
        {
            if (item == null || item.item == null) continue;

            if (!item.unlimitedStock)
                stock[item.item.itemID] = item.stockAmount;
        }
    }

    public void ApplyLoadedStock(
        List<string> ids,
        List<int> amounts
    )
    {
        if (ids == null || amounts == null) return;

        stock.Clear();

        for (int i = 0; i < ids.Count && i < amounts.Count; i++)
        {
            stock[ids[i]] = amounts[i];
        }
    }

    public bool LoadShopStock(out List<ShopStockData> savedStock)
    {
        savedStock = null;

        if (!SaveSystem.HasSaveFile())
            return false;

        SaveData data = SaveSystem.LoadShopStockData();
        if (data == null) return false;

        if (data.shopStockIDs == null || data.shopStockAmounts == null)
            return false;

        savedStock = new List<ShopStockData>();

        for (int i = 0; i < data.shopStockIDs.Count && i < data.shopStockAmounts.Count; i++)
        {
            savedStock.Add(new ShopStockData
            {
                itemID = data.shopStockIDs[i],
                amount = data.shopStockAmounts[i]
            });
        }

        return true;
    }


    public bool CanBuy(ShopItemData shopItem)
    {
        if (shopItem == null || shopItem.item == null ||
            ProfileManager.Instance == null)
            return false;

        var profile = ProfileManager.Instance.profile;

        bool meetsLevel = profile.level >= shopItem.requiredLevel;
        bool meetsFlag =
            !shopItem.requiresFlag ||
            StoryFlags.Has(shopItem.requiredFlag);

        bool hasCurrency =
            profile.currency >= shopItem.price;

        bool hasStock =
            shopItem.unlimitedStock ||
            GetStock(shopItem.item.itemID) > 0;

        return meetsLevel && meetsFlag && hasCurrency && hasStock;
    }

    public bool BuyItem(ShopItemData shopItem)
    {
        if (!CanBuy(shopItem)) return false;

        if (!shopItem.unlimitedStock &&
            !TryReduceStock(shopItem.item.itemID))
            return false;

        if (!ProfileManager.Instance
            .SpendCurrency(shopItem.price))
            return false;

        InventoryManager.Instance
            .AddItem(shopItem.item, 1);

        SaveSystem.SaveGame();
        return true;
    }

    public bool CanSell(ItemData item, int quantity)
    {
        return item != null &&
               InventoryManager.Instance
                   .GetQuantity(item) >= quantity;
    }

    public bool SellItem(
        ItemData item,
        int quantity,
        float sellRatio
    )
    {
        if (!CanSell(item, quantity)) return false;

        int price =
            Mathf.RoundToInt(item.basePrice * sellRatio)
            * quantity;

        InventoryManager.Instance
            .RemoveItem(item, quantity);

        ProfileManager.Instance.AddCurrency(price);

        SaveSystem.SaveGame();
        return true;
    }

    public int GetStock(string itemID)
    {
        return stock.TryGetValue(itemID, out int s)
            ? s
            : -1;
    }

    public bool TryReduceStock(string itemID, int amount = 1)
    {
        if (!stock.TryGetValue(itemID, out int current) ||
            current < amount)
            return false;

        stock[itemID] = current - amount;
        return true;
    }

    public void SetStock(string itemID, int amount)
    {
        stock[itemID] = amount;
    }

    public List<ShopStockData> GetStockDataForSave()
    {
        List<ShopStockData> list = new();

        foreach (var kvp in stock)
        {
            list.Add(new ShopStockData
            {
                itemID = kvp.Key,
                amount = kvp.Value
            });
        }

        return list;
    }
}


[Serializable]
public class ShopStockData
{
    public string itemID;
    public int amount;
}
