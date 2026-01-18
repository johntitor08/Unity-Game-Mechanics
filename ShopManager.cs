using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public ShopItemData[] shopItems;
    private readonly System.Collections.Generic.Dictionary<string, int> stock = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        InitializeStock();
    }

    void InitializeStock()
    {
        foreach (var shopItem in shopItems)
        {
            if (!shopItem.unlimitedStock)
                stock[shopItem.item.itemID] = shopItem.stockAmount;
        }
    }

    public bool CanBuy(ShopItemData shopItem)
    {
        var profile = ProfileManager.Instance.profile;

        // Check level requirement
        if (profile.level < shopItem.requiredLevel) return false;

        // Check flag requirement
        if (shopItem.requiresFlag && !StoryFlags.Has(shopItem.requiredFlag)) return false;

        // Check currency
        if (profile.currency < shopItem.price) return false;

        // Check stock
        if (!shopItem.unlimitedStock && GetStock(shopItem.item.itemID) <= 0) return false;

        return true;
    }

    public bool BuyItem(ShopItemData shopItem)
    {
        if (!CanBuy(shopItem)) return false;

        if (ProfileManager.Instance.SpendCurrency(shopItem.price))
        {
            InventoryManager.Instance.AddItem(shopItem.item, 1);

            if (!shopItem.unlimitedStock)
            {
                stock[shopItem.item.itemID]--;
                SaveSystem.SaveGame();
            }

            return true;
        }

        return false;
    }

    public int GetStock(string itemID)
    {
        return stock.ContainsKey(itemID) ? stock[itemID] : -1; // -1 = unlimited
    }
}
