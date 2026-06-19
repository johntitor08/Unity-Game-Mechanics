using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    private readonly Dictionary<string, int> stock = new();
    private float _priceMultiplier = 1f;

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
            if (item == null || item.item == null)
                continue;

            if (!item.unlimitedStock)
                stock[item.item.itemID] = item.stockAmount;
        }
    }

    public void ApplyPriceMultiplier(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        _priceMultiplier *= multiplier;
        Debug.Log($"[ShopManager] Price multiplier updated: {_priceMultiplier:F3}");
        SaveSystem.SaveGame();
    }

    public float GetPriceMultiplier() => _priceMultiplier;

    public float GetPriceMultiplierForSave() => _priceMultiplier;

    public void LoadPriceMultiplier(float multiplier)
    {
        _priceMultiplier = multiplier > 0f ? multiplier : 1f;
    }

    public bool CanBuy(ShopItemData shopItem)
    {
        if (shopItem == null || shopItem.item == null || ProfileManager.Instance == null)
            return false;

        var profile = ProfileManager.Instance.profile;
        bool meetsLevel = profile.level >= shopItem.requiredLevel;
        bool meetsFlag = !shopItem.requiresFlag || StoryFlags.Has(shopItem.requiredFlag);
        int finalPrice = GetFinalPrice(shopItem.price);
        bool hasCurrency = CurrencyManager.Instance != null && CurrencyManager.Instance.Has(CurrencyType.Gold, finalPrice);
        bool hasStock = shopItem.unlimitedStock || GetStock(shopItem.item.itemID) > 0;
        return meetsLevel && meetsFlag && hasCurrency && hasStock;
    }

    public bool BuyItem(ShopItemData shopItem)
    {
        if (shopItem == null || shopItem.item == null)
            return false;

        int finalPrice = GetFinalPrice(shopItem.price);

        if (!CanBuy(shopItem) || !CurrencyManager.Instance.Spend(CurrencyType.Gold, finalPrice))
            return false;

        if (!shopItem.unlimitedStock && !TryReduceStock(shopItem.item.itemID))
        {
            CurrencyManager.Instance.Add(CurrencyType.Gold, finalPrice, false);
            return false;
        }

        InventoryManager.Instance.AddItem(shopItem.item, 1);
        SaveSystem.SaveGame();
        return true;
    }

    public bool CanSell(ItemData item, int quantity)
    {
        return item != null && InventoryManager.Instance.GetQuantity(item) >= quantity;
    }

    public bool SellItem(ItemData item, int quantity, float sellRatio)
    {
        if (!CanSell(item, quantity))
            return false;

        int price = Mathf.RoundToInt(item.basePrice * item.GetRarityMultiplier() * sellRatio) * quantity;
        InventoryManager.Instance.RemoveItem(item, quantity);
        CurrencyManager.Instance.Add(CurrencyType.Gold, price);
        SaveSystem.SaveGame();
        return true;
    }

    public int GetFinalPrice(int basePrice) => Mathf.RoundToInt(basePrice * _priceMultiplier);

    public int GetStock(string itemID) => stock.TryGetValue(itemID, out int s) ? s : -1;

    public bool TryReduceStock(string itemID, int amount = 1)
    {
        if (!stock.TryGetValue(itemID, out int current) || current < amount)
            return false;

        stock[itemID] = current - amount;
        return true;
    }

    public void SetStock(string itemID, int amount) => stock[itemID] = amount;

    public void ApplyLoadedStock(List<string> ids, List<int> amounts)
    {
        if (ids == null || amounts == null)
            return;

        stock.Clear();

        for (int i = 0; i < ids.Count && i < amounts.Count; i++)
            stock[ids[i]] = amounts[i];
    }

    public List<ShopStockData> GetStockDataForSave()
    {
        var list = new List<ShopStockData>();

        foreach (var kvp in stock)
            list.Add(new ShopStockData { itemID = kvp.Key, amount = kvp.Value });

        return list;
    }
}

[Serializable]
public class ShopStockData
{
    public string itemID;
    public int amount;
}
