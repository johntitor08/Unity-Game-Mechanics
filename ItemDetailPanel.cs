using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    public static ItemDetailPanel Instance;

    [Header("UI")]
    public TextMeshProUGUI title;
    public Image icon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI description;
    public Button useButton;
    private ItemData currentItem;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public void Show(ItemData item, int quantity = -1)
    {
        if (item == null) return;
        currentItem = item;
        icon.sprite = item.icon;
        title.text = item.itemName;
        description.text = item.description;

        int qty = quantity >= 0
            ? quantity
            : InventoryManager.Instance.GetQuantity(item);

        quantityText.text = $"Sahip olunan: {qty}";
        useButton.onClick.RemoveAllListeners();

        if (item.useable && qty > 0)
        {
            useButton.gameObject.SetActive(true);
            useButton.onClick.AddListener(UseItem);
        }
        else
        {
            useButton.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    void UseItem()
    {
        if (currentItem == null) return;
        if (!currentItem.useable) return;
        currentItem.onUse?.Invoke();
        InventoryManager.Instance.RemoveItem(currentItem, 1);
        int remaining = InventoryManager.Instance.GetQuantity(currentItem);

        if (remaining <= 0)
        {
            Close();
        }
        else
        {
            Show(currentItem, remaining);
        }
    }

    public void Close()
    {
        currentItem = null;
        gameObject.SetActive(false);
    }
}
