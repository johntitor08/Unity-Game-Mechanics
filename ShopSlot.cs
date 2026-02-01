using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShopSlot : MonoBehaviour
{
    private static readonly WaitForSeconds waitForSeconds0_1 = new(0.1f);
    private static readonly WaitForSeconds waitForSeconds0_5 = new(0.5f);
    private ShopItemData currentItem;
    private bool isPurchasing;

    [Header("Visual Elements")]
    public Image itemIcon;
    public Image background;
    public Image rarityBadge;

    [Header("Text Elements")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI requirementText;
    public TextMeshProUGUI propertiesText;

    [Header("Stats Display")]
    public GameObject statsContainer;
    public TextMeshProUGUI[] statTexts;

    [Header("Interaction")]
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    [Header("Locked State")]
    public GameObject lockedOverlay;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color canAffordColor = Color.green;
    public Color cannotAffordColor = Color.red;
    public Color lockedColor = Color.gray;

    [Header("Rarity Colors")]
    public Color commonColor = new(0.6f, 0.6f, 0.6f);
    public Color rareColor = new(0.2f, 0.5f, 1f);
    public Color epicColor = new(0.6f, 0.2f, 1f);
    public Color legendaryColor = new(1f, 0.6f, 0f);

    void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnEnable()
    {
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnCurrencyChanged += RefreshVisuals;
    }

    void OnDisable()
    {
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnCurrencyChanged -= RefreshVisuals;
    }

    public void Setup(ShopItemData shopItem)
    {
        if (shopItem == null || ShopManager.Instance == null) return;
        currentItem = shopItem;
        isPurchasing = false;
        if (itemIcon != null) itemIcon.sprite = shopItem.item.icon;
        if (itemNameText != null) itemNameText.text = shopItem.item.itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = shopItem.item.description;
        if (priceText != null) priceText.text = $"{shopItem.price}";
        UpdateStockDisplay();
        SetupRarity(shopItem.item);
        SetupRequirements(shopItem);
        SetupStats(shopItem.item);
        SetupProperties(shopItem.item);
        RefreshVisuals(ProfileManager.Instance != null ? ProfileManager.Instance.profile : null);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    void UpdateStockDisplay()
    {
        if (currentItem == null || stockText == null || ShopManager.Instance == null) return;
        int stockAmount = ShopManager.Instance.GetStock(currentItem.item.itemID);
        stockText.text = stockAmount == -1 ? "∞" : $"Stock: {stockAmount}";
    }

    void RefreshVisuals(PlayerProfile profile)
    {
        if (currentItem == null || ShopManager.Instance == null || profile == null) return;
        bool canBuy = ShopManager.Instance.CanBuy(currentItem);
        UpdateVisuals(currentItem, canBuy);
    }

    void SetupRarity(ItemData item)
    {
        if (item == null) return;
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

        if (rarityText != null) rarityText.text = rarityEnum.ToString();
        if (rarityBadge != null) rarityBadge.color = color;
    }

    void SetupStats(ItemData item)
    {
        if (statsContainer == null || statTexts == null) return;

        if (item is EquipmentData equip)
        {
            statsContainer.SetActive(true);
            int shown = 0;

            if (equip.damageBonus > 0 && shown < statTexts.Length)
                statTexts[shown++].text = $"Damage: +{equip.damageBonus}";

            if (equip.defenseBonus > 0 && shown < statTexts.Length)
                statTexts[shown++].text = $"Defense: +{equip.defenseBonus}";

            if (equip.primaryStatBonus > 0 && shown < statTexts.Length)
                statTexts[shown++].text = $"{equip.primaryStat}: +{equip.primaryStatBonus}";

            if (equip.secondaryStatBonus > 0 && shown < statTexts.Length)
                statTexts[shown++].text = $"{equip.secondaryStat}: +{equip.secondaryStatBonus}";

            for (int i = shown; i < statTexts.Length; i++)
                statTexts[i].text = "";
        }
        else
        {
            statsContainer.SetActive(false);
        }
    }

    void SetupProperties(ItemData item)
    {
        if (propertiesText == null) return;

        if (item is EquipmentData equip)
        {
            string props = $"Slot: {equip.slot}";
            
            if (!string.IsNullOrEmpty(equip.setName))
                props += $"\nSet: {equip.setName}";

            propertiesText.gameObject.SetActive(true);
            propertiesText.text = props;
        }
        else
        {
            propertiesText.gameObject.SetActive(false);
        }
    }

    void SetupRequirements(ShopItemData shopItem)
    {
        if (requirementText == null || ProfileManager.Instance == null || ShopManager.Instance == null) return;
        List<string> unmet = new();
        var profile = ProfileManager.Instance.profile;

        if (profile.level < shopItem.requiredLevel)
            unmet.Add($"Requires Level {shopItem.requiredLevel}");

        if (shopItem.requiresFlag && !StoryFlags.Has(shopItem.requiredFlag))
            unmet.Add("Story Progress Required");

        int stockAmount = ShopManager.Instance.GetStock(shopItem.item.itemID);
        if (!shopItem.unlimitedStock && stockAmount <= 0)
            unmet.Add("Out of Stock");

        if (unmet.Count > 0)
        {
            requirementText.gameObject.SetActive(true);
            requirementText.text = string.Join("\n", unmet);
            requirementText.color = Color.red;
        }
        else
        {
            requirementText.gameObject.SetActive(false);
        }
    }

    void UpdateVisuals(ShopItemData shopItem, bool canBuy)
    {
        if (buyButton == null || buyButtonText == null || background == null || ProfileManager.Instance == null) return;
        buyButton.interactable = canBuy && !isPurchasing;
        buyButtonText.text = canBuy ? "BUY" : "LOCKED";

        if (!canBuy)
            background.color = lockedColor;
        else if (ProfileManager.Instance.profile.currency >= shopItem.price)
            background.color = canAffordColor * 0.3f;
        else
            background.color = cannotAffordColor * 0.3f;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!canBuy);
    }

    void OnBuyClicked()
    {
        if (isPurchasing || ShopManager.Instance == null || currentItem == null) return;
        isPurchasing = true;

        if (ShopManager.Instance.BuyItem(currentItem))
        {
            StartCoroutine(PurchaseFeedback());
        }
        else
        {
            isPurchasing = false;
            StartCoroutine(FailureFeedback());
        }
    }

    IEnumerator PurchaseFeedback()
    {
        if (buyButtonText == null)
        {
            isPurchasing = false;
            yield break;
        }

        string originalText = buyButtonText.text;
        Color originalColor = buyButtonText.color;
        buyButtonText.text = "PURCHASED!";
        buyButtonText.color = Color.green;
        buyButton.interactable = false;
        yield return waitForSeconds0_5;
        buyButtonText.text = originalText;
        buyButtonText.color = originalColor;
        UpdateStockDisplay();
        RefreshVisuals(ProfileManager.Instance != null ? ProfileManager.Instance.profile : null);
        isPurchasing = false;

        if (ShopUI.Instance != null)
            ShopUI.Instance.RefreshShop();
    }

    IEnumerator FailureFeedback()
    {
        if (background == null) yield break;
        Color original = background.color;

        for (int i = 0; i < 3; i++)
        {
            background.color = Color.red * 0.5f;
            yield return waitForSeconds0_1;
            background.color = original;
            yield return waitForSeconds0_1;
        }
    }
}
