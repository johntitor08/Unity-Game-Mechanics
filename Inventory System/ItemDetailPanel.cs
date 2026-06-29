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
    public Button actionButton;
    public Button closeButton;
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

        if (actionButton != null)
            actionButton.interactable = false;

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
            quantityText.text = $"Owned: {qty}";

        if (upgradeFusionButton != null)
            upgradeFusionButton.SetItem(item is EquipmentData eq ? eq : null);

        ConfigureButtons(item, qty);
        UIPanelAnimator.Show(itemDetailPanel);
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

        if (actionButton == null)
            return;

        actionButton.gameObject.SetActive(true);
        actionButton.onClick.RemoveAllListeners();
        bool isReadable = item.readable && !item.IsEquipment();

        if (isReadable)
        {
            SetActionButtonLabel("Read");
            actionButton.interactable = true;
            actionButton.onClick.AddListener(ReadItem);
        }
        else
        {
            SetActionButtonLabel(item.IsEquipment() ? "Equip" : "Use");
            actionButton.interactable = item.useable && quantity > 0;
            actionButton.onClick.AddListener(UseItem);
        }
    }

    private void SetActionButtonLabel(string label)
    {
        if (actionButton == null)
            return;

        var text = actionButton.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
            text.text = label;
    }

    public void ReadItem()
    {
        if (currentItem == null)
            return;

        if (ReadingPanel.Instance != null)
            ReadingPanel.Instance.Show(currentItem.itemName, currentItem.readText, currentItem.icon);
        else
            Debug.LogWarning("[ItemDetailPanel] ReadingPanel.Instance is null.");
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

        if (actionButton != null)
            actionButton.interactable = false;

        ItemData item = currentItem;
        item.onUse?.Invoke();

        if (item.IsEquipment())
        {
            if (item is EquipmentData eq)
                EquipEquipment(eq);
            else
                Debug.LogError($"{item.itemID} IsEquipment() true ama EquipmentData değil");

            RefreshQuantity();
            return;
        }

        if (item is StatModifierItem statMod)
            statMod.Use();

        if (item is ConsumableBuffItem buffItem)
            buffItem.Use();

        if (!string.IsNullOrEmpty(item.questObjectiveOnUse) && QuestManager.Instance != null)
            QuestManager.Instance.NotifyObjectInteracted(item.questObjectiveOnUse, 1);

        if (item.consumeOnUse)
        {
            bool removed = InventoryManager.Instance.RemoveItem(item, 1);

            if (!removed)
            {
                isProcessingUse = false;

                if (actionButton != null)
                    actionButton.interactable = true;

                return;
            }
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.NotifyItemUsed(item);

        RefreshQuantity();
    }

    void RefreshQuantity()
    {
        if (currentItem == null || InventoryManager.Instance == null)
            return;

        int qty = ResolveQuantity(currentItem, -1, currentUpgradeLevel);

        if (quantityText != null)
            quantityText.text = $"Owned: {qty}";

        isProcessingUse = false;

        if (qty <= 0)
        {
            Close();
            return;
        }

        if (actionButton != null && currentItem.useable)
            actionButton.interactable = true;
    }

    public void Close()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.interactable = false;
        }

        if (upgradeFusionButton != null)
            upgradeFusionButton.SetItem(null);

        currentItem = null;
        currentUpgradeLevel = 0;
        isProcessingUse = false;
        UIPanelAnimator.Hide(itemDetailPanel);
    }
}
