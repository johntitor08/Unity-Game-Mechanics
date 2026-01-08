using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    public Button buyButton;

    private ShopItemData shopItem;

    public void Setup(ShopItemData item)
    {
        shopItem = item;

        itemIcon.sprite = item.item.icon;
        itemName.text = item.item.itemName;
        priceText.text = item.price + " Gold";

        int stock = ShopManager.Instance.GetStock(item.item.itemID);
        stockText.text = stock == -1 ? "∞" : "Stock: " + stock;

        bool canBuy = ShopManager.Instance.CanBuy(item);
        buyButton.interactable = canBuy;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        if (ShopManager.Instance.BuyItem(shopItem))
        {
            ShopUI.Instance.RefreshShop();
        }
    }
}
