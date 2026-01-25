using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public ShopItemData[] shopItems;
    private readonly Dictionary<string, int> stock = new();

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

        InitializeStock();
    }

    void InitializeStock()
    {
        foreach (var item in shopItems)
        {
            if (!item.unlimitedStock)
                stock[item.item.itemID] = item.stockAmount;
        }
    }

    public bool CanBuy(ShopItemData shopItem)
    {
        var profile = ProfileManager.Instance.profile;
        bool meetsLevel = profile.level >= shopItem.requiredLevel;
        bool meetsFlag = !shopItem.requiresFlag || StoryFlags.Has(shopItem.requiredFlag);
        bool hasCurrency = profile.currency >= shopItem.price;
        bool hasStock = shopItem.unlimitedStock || GetStock(shopItem.item.itemID) > 0;
        return meetsLevel && meetsFlag && hasCurrency && hasStock;
    }

    public bool BuyItem(ShopItemData shopItem)
    {
        if (!CanBuy(shopItem)) return false;
        if (!ProfileManager.Instance.SpendCurrency(shopItem.price)) return false;
        InventoryManager.Instance.AddItem(shopItem.item, 1);

        if (!shopItem.unlimitedStock)
        {
            TryReduceStock(shopItem.item.itemID);
            SaveSystem.SaveGame();
        }

        return true;
    }

    public bool CanSell(ItemData item, int quantity)
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.GetQuantity(item) >= quantity;
    }

    public bool SellItem(ItemData item, int quantity, float sellRatio)
    {
        if (!CanSell(item, quantity)) return false;
        int price = Mathf.RoundToInt(item.basePrice * sellRatio) * quantity;
        InventoryManager.Instance.RemoveItem(item, quantity);
        ProfileManager.Instance.AddCurrency(price);
        SaveSystem.SaveGame();
        return true;
    }

    public int GetStock(string itemID)
    {
        return stock.ContainsKey(itemID) ? stock[itemID] : -1;
    }

    public bool TryReduceStock(string itemID, int amount = 1)
    {
        if (!stock.ContainsKey(itemID)) return true;

        if (stock[itemID] >= amount)
        {
            stock[itemID] -= amount;
            return true;
        }

        return false;
    }
}
