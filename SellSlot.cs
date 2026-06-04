using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellSlot : MonoBehaviour
{
    [Header("Visual")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;
    public Image rarityBorder;
    public TextMeshProUGUI rarityText;
    public Button sellButton;

    [Header("Rarity Colors")]
    public Color commonColor = new(0.6f, 0.6f, 0.6f);
    public Color rareColor = new(0.2f, 0.5f, 1f);
    public Color epicColor = new(0.6f, 0.2f, 1f);
    public Color legendaryColor = new(1f, 0.6f, 0f);

    private ItemData item;
    private EquipmentData equipData;
    private int upgradeLevel;
    private int quantity;
    private float sellRatio;
    private bool isEquipment;

    public void Setup(ItemData newItem, int qty, float ratio)
    {
        item = newItem;
        equipData = null;
        upgradeLevel = 0;
        quantity = qty;
        sellRatio = ratio;
        isEquipment = false;
        ApplyVisuals();
    }

    public void SetupEquipment(EquipmentData data, int lvl, int qty, float ratio)
    {
        item = data;
        equipData = data;
        upgradeLevel = lvl;
        quantity = qty;
        sellRatio = ratio;
        isEquipment = true;
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        if (item == null)
            return;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (nameText != null)
            nameText.text = (isEquipment && upgradeLevel > 0) ? $"{item.itemName} +{upgradeLevel}" : item.itemName;

        if (descText != null)
            descText.text = item.description;

        if (quantityText != null)
            quantityText.text = $"x{quantity}";

        if (priceText != null)
        {
            int price = Mathf.RoundToInt(item.basePrice * item.GetRarityMultiplier() * sellRatio);
            priceText.text = $"{price}";
        }

        SetupRarity(item);

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellOne);
            RefreshButton();
        }
    }

    void SetupRarity(ItemData item)
    {
        if (item == null)
            return;

        Rarity rarityEnum = item.rarity;
        Color color;

        if (item is EquipmentData equip)
        {
            rarityEnum = equip.rarity;
            color = equip.GetRarityColor();
        }
        else
        {
            color = rarityEnum switch
            {
                Rarity.Common => commonColor,
                Rarity.Rare => rareColor,
                Rarity.Epic => epicColor,
                Rarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        if (rarityText != null)
            rarityText.text = rarityEnum.ToString();

        if (rarityBorder != null)
            rarityBorder.color = color;
    }

    void RefreshButton()
    {
        if (sellButton == null)
            return;

        sellButton.interactable = GetCurrentQuantity() > 0;
    }

    int GetCurrentQuantity()
    {
        if (InventoryManager.Instance == null)
            return 0;

        if (isEquipment && equipData != null)
            return InventoryManager.Instance.GetUpgradedQuantity(equipData, upgradeLevel);

        return item != null ? InventoryManager.Instance.GetQuantity(item) : 0;
    }

    void SellOne()
    {
        if (item == null || ShopManager.Instance == null)
            return;

        bool sold;

        if (isEquipment && equipData != null)
        {
            if (InventoryManager.Instance.GetUpgradedQuantity(equipData, upgradeLevel) <= 0)
                return;

            int price = Mathf.RoundToInt(item.basePrice * item.GetRarityMultiplier() * sellRatio);
            InventoryManager.Instance.RemoveUpgradedItem(equipData, upgradeLevel, 1);

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.Add(CurrencyType.Gold, price);

            SaveSystem.SaveGame();
            sold = true;
        }
        else
        {
            sold = ShopManager.Instance.SellItem(item, 1, sellRatio);
        }

        if (!sold)
            return;

        quantity = GetCurrentQuantity();

        if (quantityText != null)
            quantityText.text = $"x{quantity}";

        RefreshButton();

        if (SellUI.Instance != null)
            SellUI.Instance.Refresh();
    }
}
