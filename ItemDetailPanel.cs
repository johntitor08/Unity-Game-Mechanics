using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    public static ItemDetailPanel Instance;
    private ItemData currentItem;
    private bool isProcessingUse;

    [Header("UI")]
    public TextMeshProUGUI title;
    public Image icon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI description;
    public Button useButton;
    public Button equipButton;
    public Button closeButton;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshQuantity;
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshQuantity;
    }

    public void ShowItemDetail(ItemData item, int quantity)
    {
        currentItem = item;
        isProcessingUse = false;

        if (title != null)
            title.text = item.itemName;

        if (description != null)
            description.text = item.description;

        if (icon != null)
            icon.sprite = item.icon;

        int qty = quantity >= 0 ? quantity : InventoryManager.Instance.GetQuantity(item);

        if (quantityText != null)
            quantityText.text = $"Sahip olunan: {qty}";

        ConfigureButtons(item, qty);
        gameObject.SetActive(true);
    }

    private void ConfigureButtons(ItemData item, int quantity)
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (equipButton != null && useButton != null)
        {
            if (item.IsEquipment())
            {
                equipButton.gameObject.SetActive(true);
                equipButton.interactable = item.useable && quantity > 0;
                useButton.gameObject.SetActive(false);
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(UseItem);
            }
            else
            {
                useButton.gameObject.SetActive(true);
                useButton.interactable = item.useable && quantity > 0;
                equipButton.gameObject.SetActive(false);
                useButton.onClick.RemoveAllListeners();
                useButton.onClick.AddListener(UseItem);
            }
        }
    }

    private void EquipEquipment(EquipmentData equipment)
    {
        if (EquipmentManager.Instance.CanEquip(equipment))
        {
            InventoryManager.Instance.RemoveItem(equipment, 1);
            EquipmentManager.Instance.Equip(equipment);
        }
        else
        {
            Debug.Log("Cannot equip this item!");
        }
    }

    public void UseItem()
    {
        if (isProcessingUse || currentItem == null || !currentItem.useable)
            return;

        isProcessingUse = true;

        if (useButton != null)
            useButton.interactable = false;

        currentItem.onUse?.Invoke();

        // Apply stat effect if configured
        if (currentItem.statAmount > 0 && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Modify(currentItem.affectedStat, currentItem.statAmount);
        }

        if (currentItem.IsEquipment())
        {
            EquipEquipment(currentItem as EquipmentData);
            RefreshQuantity();
            return;
        }

        bool removed = InventoryManager.Instance.RemoveItem(currentItem, 1);

        if (!removed)
        {
            isProcessingUse = false;
            if (useButton != null)
                useButton.interactable = true;
        }
    }

    void RefreshQuantity()
    {
        if (currentItem == null || InventoryManager.Instance == null) return;
        int qty = InventoryManager.Instance.GetQuantity(currentItem);

        if (quantityText != null)
            quantityText.text = $"Sahip olunan: {qty}";

        if (qty <= 0)
        {
            Close();
        }
        else
        {
            isProcessingUse = false;

            if (useButton != null && currentItem.useable)
                useButton.interactable = true;
        }
    }

    public void Close()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.interactable = false;
        }

        currentItem = null;
        isProcessingUse = false;
        gameObject.SetActive(false);
    }
}
