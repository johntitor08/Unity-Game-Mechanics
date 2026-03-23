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

    public void Setup(ItemData newItem, int qty, float ratio)
    {
        if (newItem == null)
            return;

        item = newItem;
        quantity = qty;
        sellRatio = ratio;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (nameText != null)
            nameText.text = item.itemName;

        if (quantityText != null)
            quantityText.text = $"x{quantity}";

        if (priceText != null)
        {
            int price = Mathf.RoundToInt(item.basePrice * item.GetRarityMultiplier() * sellRatio);
            priceText.text = $"{price} Gold";
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellOne);
            sellButton.interactable = InventoryManager.Instance != null && InventoryManager.Instance.GetQuantity(item) > 0;
        }
    }

    void SellOne()
    {
        if (item == null || ShopManager.Instance == null)
            return;

        if (ShopManager.Instance.SellItem(item, 1, sellRatio))
        {
            quantity = InventoryManager.Instance != null ? InventoryManager.Instance.GetQuantity(item) : 0;

            if (quantityText != null)
                quantityText.text = $"x{quantity}";

            if (sellButton != null)
                sellButton.interactable = quantity > 0;

            if (ShopUI.Instance != null)
                ShopUI.Instance.RefreshSellPanel();
        }
    }
}
