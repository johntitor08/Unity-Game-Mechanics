using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    public static ItemDetailPanel Instance;

    public TextMeshProUGUI title;
    public Image icon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI description;
    public Button useButton;

    ItemData currentItem;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.SetActive(false);
    }

    public void Show(ItemData item)
    {
        currentItem = item;

        icon.sprite = item.icon;
        title.text = item.itemName;
        description.text = item.description;

        int qty = InventoryManager.Instance.GetQuantity(item);
        quantityText.text = "Sahip olunan: " + qty;

        useButton.gameObject.SetActive(item.useable);
        gameObject.SetActive(true);
    }

    public void UseItem()
    {
        if (currentItem == null) return;

        currentItem.onUse?.Invoke();
        InventoryManager.Instance.RemoveItem(currentItem, 1);

        if (InventoryManager.Instance.GetQuantity(currentItem) <= 0)
            gameObject.SetActive(false);
        else
            Show(currentItem);
    }
}
