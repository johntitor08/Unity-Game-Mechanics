using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI quantityText;

    ItemData item;

    public void Setup(string id, int qty)
    {
        item = InventoryManager.Instance.GetItem(id);
        icon.sprite = item.icon;
        title.text = item.itemName;
        quantityText.text = "x" + qty;
    }

    public void OnClick()
    {
        ItemDetailPanel.Instance.Show(item);
    }
}
