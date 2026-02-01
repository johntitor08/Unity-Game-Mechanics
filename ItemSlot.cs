using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI quantityText;
    public Image rarityBorder;

    [Header("Tooltip")]
    public ItemTooltip tooltip;

    private ItemData item;
    private int quantity;

    public void Setup(string itemID, int qty = -1)
    {
        item = ItemDatabase.Instance.GetByID(itemID);

        if (item == null)
        {
            Clear();
            return;
        }

        if (qty >= 0)
        {
            quantity = qty;
        }
        else
        {
            InventoryManager.Instance.GetItems().TryGetValue(itemID, out quantity);
        }

        icon.sprite = item.icon;
        icon.enabled = true;
        title.text = item.itemName;
        ApplyRarityUI(item);

        if (item.stackable && quantity > 1)
        {
            quantityText.gameObject.SetActive(true);
            quantityText.text = $"x{quantity}";
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    void ApplyRarityUI(ItemData item)
    {
        Color rarityColor = item.GetRarityColor();

        if (rarityBorder != null)
            rarityBorder.color = rarityColor;

        if (title != null)
            title.color = rarityColor;
    }

    public void OnClick()
    {
        if (item == null) return;
        ItemDetailPanel.Instance.ShowItemDetail(item, quantity);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && tooltip != null)
            tooltip.Show(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.Hide();
    }

    void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
        title.text = "";
        quantityText.gameObject.SetActive(false);
        item = null;
        quantity = 0;
    }
}
