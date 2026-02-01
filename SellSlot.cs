using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;
    public Button sellButton;
    private ItemData item;
    private int quantity;
    private float sellRatio;

    public void Setup(ItemData item, int qty, float ratio)
    {
        if (item == null) return;
        this.item = item;
        quantity = qty;
        sellRatio = ratio;
        icon.sprite = item.icon;
        icon.enabled = true;
        nameText.text = item.itemName;
        quantityText.text = $"x{quantity}";
        int price = Mathf.RoundToInt(item.basePrice * item.GetRarityMultiplier() * sellRatio);
        priceText.text = $"{price} Gold";
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(SellOne);
        sellButton.interactable = InventoryManager.Instance.GetQuantity(item) > 0;
    }

    void SellOne()
    {
        if (item == null) return;

        if (ShopManager.Instance.SellItem(item, 1, sellRatio))
        {
            quantity = InventoryManager.Instance.GetQuantity(item);
            quantityText.text = $"x{quantity}";
            sellButton.interactable = quantity > 0;
            ShopUI.Instance.RefreshSellPanel();
        }
    }
}
