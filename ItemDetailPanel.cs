using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    public static ItemDetailPanel Instance;
    private ItemData currentItem;
    private int currentUpgradeLevel;
    private bool isProcessingUse;

    [Header("UI")]
    public GameObject itemDetailPanel;
    public TextMeshProUGUI title;
    public Image icon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI description;
    public Button useButton;
    public Button equipButton;
    public Button closeButton;
    public TextMeshProUGUI upgradeLevelText;
    public UpgradeFusionButton upgradeFusionButton;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    public void ShowItemDetail(ItemData item, int quantity, int upgradeLevel = 0)
    {
        currentItem = item;
        currentUpgradeLevel = upgradeLevel;
        isProcessingUse = false;

        if (useButton != null)
            useButton.interactable = false;

        if (equipButton != null)
            equipButton.interactable = false;

        if (title != null)
        {
            string upgradeStr = upgradeLevel > 0 ? $" <color=#FFD700>+{upgradeLevel}</color>" : "";
            title.text = $"{item.itemName}{upgradeStr}";
        }

        if (description != null)
            description.text = item.description;

        if (icon != null)
            icon.sprite = item.icon;

        int qty = ResolveQuantity(item, quantity, upgradeLevel);

        if (quantityText != null)
            quantityText.text = $"Sahip olunan: {qty}";

        if (upgradeLevelText != null)
        {
            bool showUpgrade = upgradeLevel > 0 && item is EquipmentData;
            upgradeLevelText.gameObject.SetActive(showUpgrade);
            upgradeLevelText.text = $"+{upgradeLevel}";
        }

        if (upgradeFusionButton != null)
            upgradeFusionButton.SetItem(item is EquipmentData eq ? eq : null);

        ConfigureButtons(item, qty);
        itemDetailPanel.SetActive(true);
    }

    public void RefreshPanel()
    {
        if (currentItem == null || itemDetailPanel == null || !itemDetailPanel.activeSelf)
            return;

        ShowItemDetail(currentItem, -1, currentUpgradeLevel);
    }

    private int ResolveQuantity(ItemData item, int quantity, int upgradeLevel)
    {
        if (quantity >= 0)
            return quantity;

        if (item is EquipmentData eqData)
            return upgradeLevel > 0 ? InventoryManager.Instance.GetUpgradedQuantity(eqData, upgradeLevel) : InventoryManager.Instance.GetQuantity(eqData);

        return InventoryManager.Instance.GetQuantity(item);
    }

    private void ConfigureButtons(ItemData item, int quantity)
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (equipButton == null || useButton == null)
            return;

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

    private void EquipEquipment(EquipmentData equipment)
    {
        var em = EquipmentManager.Instance;

        if (em == null)
            return;

        if (!em.Equip(new EquipmentInstance(equipment, currentUpgradeLevel)))
            Debug.Log("Cannot equip this item!");
    }

    public void UseItem()
    {
        if (isProcessingUse || currentItem == null || !currentItem.useable)
            return;

        isProcessingUse = true;

        if (useButton != null)
            useButton.interactable = false;

        if (equipButton != null)
            equipButton.interactable = false;

        currentItem.onUse?.Invoke();

        if (currentItem.IsEquipment())
        {
            if (currentItem is EquipmentData eq)
                EquipEquipment(eq);
            else
                Debug.LogError($"{currentItem.itemID} IsEquipment() true ama EquipmentData değil");

            isProcessingUse = false;
            RefreshQuantity();
            return;
        }

        if (currentItem is StatModifierItem statMod)
            statMod.Use();

        bool removed = InventoryManager.Instance.RemoveItem(currentItem, 1);

        if (!removed)
        {
            isProcessingUse = false;

            if (useButton != null)
                useButton.interactable = true;

            return;
        }

        RefreshQuantity();
    }

    void RefreshQuantity()
    {
        if (currentItem == null || InventoryManager.Instance == null)
            return;

        int qty = ResolveQuantity(currentItem, -1, currentUpgradeLevel);

        if (quantityText != null)
            quantityText.text = $"Sahip olunan: {qty}";

        isProcessingUse = false;

        if (qty <= 0)
        {
            Close();
            return;
        }

        if (useButton != null && currentItem.useable)
            useButton.interactable = true;

        if (equipButton != null && currentItem.useable)
            equipButton.interactable = true;
    }

    public void Close()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.interactable = false;
        }

        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.interactable = false;
        }

        if (upgradeFusionButton != null)
            upgradeFusionButton.SetItem(null);

        currentItem = null;
        currentUpgradeLevel = 0;
        isProcessingUse = false;
        itemDetailPanel.SetActive(false);
    }
}
