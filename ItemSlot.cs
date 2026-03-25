using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI quantityText;
    public Image rarityBorder;
    public TextMeshProUGUI upgradeLevelText;
    public TextMeshProUGUI upgradeButtonText;
    public Button upgradeButton;

    [Header("Tooltip")]
    public ItemTooltip tooltip;

    private ItemData currentItem;
    private int currentUpgradeLevel;
    private int currentQuantity;

    void Awake()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUpgradeButton;

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUpgradeButton;
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUpgradeButton;

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUpgradeButton;
    }

    public void Setup(string itemID, int qty = 1, int upgradeLevel = 0)
    {
        currentItem = ItemDatabase.Instance.GetByID(itemID);

        if (currentItem == null)
        {
            Clear();
            return;
        }

        currentUpgradeLevel = upgradeLevel;
        currentQuantity = qty;
        icon.sprite = currentItem.icon;
        icon.enabled = true;
        string displayName = upgradeLevel > 0 ? $"{currentItem.itemName} <color=#FFD700>+{upgradeLevel}</color>" : currentItem.itemName;
        title.text = displayName;
        ApplyRarityUI(currentItem);

        if (currentItem.stackable && qty > 1)
        {
            quantityText.gameObject.SetActive(true);
            quantityText.text = $"x{qty}";
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }

        if (upgradeLevelText != null)
        {
            upgradeLevelText.gameObject.SetActive(upgradeLevel > 0);
            upgradeLevelText.text = $"+{upgradeLevel}";
        }

        RefreshUpgradeButton();
    }

    void RefreshUpgradeButton()
    {
        if (upgradeButton == null || currentItem is not EquipmentData equipData)
            return;

        bool canUpgrade = FusionManager.Instance != null && FusionManager.Instance.CanUpgradeFuse(equipData);
        upgradeButton.gameObject.SetActive(canUpgrade);

        if (!canUpgrade || upgradeButtonText == null)
            return;

        int bestLevel = currentUpgradeLevel;
        var em = EquipmentManager.Instance;

        if (em != null)
        {
            var equipped = em.GetEquipped(equipData.slot);

            if (equipped != null && equipped.baseData.itemID == equipData.itemID)
                bestLevel = Mathf.Max(bestLevel, equipped.upgradeLevel);
        }

        if (InventoryManager.Instance != null)
        {
            foreach (var (inst, _) in InventoryManager.Instance.GetEquipmentInstances())
                if (inst.baseData.itemID == equipData.itemID)
                    bestLevel = Mathf.Max(bestLevel, inst.upgradeLevel);
        }

        upgradeButtonText.text = $"+{bestLevel + 1}";
    }

    void OnUpgradeClicked()
    {
        if (currentItem is not EquipmentData equipData || FusionManager.Instance == null)
            return;

        FusionManager.Instance.UpgradeFuse(equipData);
    }

    void ApplyRarityUI(ItemData item)
    {
        Color c = item.GetRarityColor();

        if (rarityBorder != null)
            rarityBorder.color = c;

        if (title != null)
            title.color = c;
    }

    public void OnClick()
    {
        if (currentItem == null)
            return;

        ItemDetailPanel.Instance.ShowItemDetail(currentItem, currentQuantity, currentUpgradeLevel);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && tooltip != null)
            tooltip.Show(currentItem, currentUpgradeLevel);
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

        if (upgradeLevelText != null)
            upgradeLevelText.gameObject.SetActive(false);

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);

        currentItem = null;
        currentUpgradeLevel = 0;
        currentQuantity = 0;
    }
}
