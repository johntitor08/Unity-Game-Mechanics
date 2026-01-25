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
        this.item = item;
        quantity = qty;
        sellRatio = ratio;
        icon.sprite = item.icon;
        nameText.text = item.itemName;
        quantityText.text = $"x{qty}";
        int price = Mathf.RoundToInt(item.basePrice * item.GetRarityMultiplier() * sellRatio);
        priceText.text = $"{price} Gold";
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(SellOne);
    }

    void SellOne()
    {
        if (ShopManager.Instance.SellItem(item, 1, sellRatio))
        {
            ShopUI.Instance.RefreshSellPanel();
        }
    }
}
