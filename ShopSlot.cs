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
    private readonly List<TextMeshProUGUI> spawnedStatTexts = new();
    private RectTransform btnRect;
    private Vector2 originalSize;
    private string originalText;
    private Color originalColor;

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
    public TextMeshProUGUI statTextPrefab;
    public Transform statsParent;
    public int maxStatLines = 2;

    [Header("Interaction")]
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;
    public Button closeButton;

    [Header("Locked State")]
    public GameObject lockedOverlay;

    [Header("Visual Feedback")]
    public Color normalColor = Color.black;
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

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
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

        if (isPurchasing)
            ResetVisuals();
    }

    public void Setup(ShopItemData shopItem)
    {
        if (shopItem == null || ShopManager.Instance == null)
            return;

        currentItem = shopItem;
        isPurchasing = false;

        if (itemIcon != null)
            itemIcon.sprite = shopItem.item.icon;

        if (itemNameText != null)
            itemNameText.text = shopItem.item.itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = shopItem.item.description;

        if (priceText != null)
            priceText.text = $"{shopItem.price}";

        GetProperties();
        UpdateStockDisplay();
        SetupRarity(shopItem.item);
        SetupRequirements(shopItem);
        SetupStats(shopItem.item);
        SetupProperties(shopItem.item);
        RefreshVisuals(ProfileManager.Instance != null ? ProfileManager.Instance.profile : null);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
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

        if (rarityBadge != null)
            rarityBadge.color = color;
    }

    void SetupStats(ItemData item)
    {
        foreach (var t in spawnedStatTexts)
            if (t != null)
                Destroy(t.gameObject);

        spawnedStatTexts.Clear();

        if (statsContainer == null)
            return;

        var lines = new List<string>();

        if (item is EquipmentData equip)
        {
            if (equip.damageBonus > 0)
                lines.Add($"Damage: +{equip.damageBonus}");

            if (equip.defenseBonus > 0)
                lines.Add($"Defense: +{equip.defenseBonus}");

            if (equip.primaryStatBonus > 0)
                lines.Add($"{equip.primaryStat}: +{equip.primaryStatBonus}");

            if (equip.secondaryStatBonus > 0)
                lines.Add($"{equip.secondaryStat}: +{equip.secondaryStatBonus}");
        }
        else if (item is StatModifierItem statMod)
        {
            string sign = statMod.modifyAmount >= 0 ? "+" : "";
            string label = statMod.modifyMaxStat ? $"Max {statMod.targetStat}" : statMod.targetStat.ToString();
            lines.Add($"{label}: {sign}{statMod.modifyAmount}");
        }

        int count = Mathf.Min(lines.Count, maxStatLines);

        if (count == 0)
        {
            statsContainer.SetActive(false);
            return;
        }

        statsContainer.SetActive(true);
        Transform parent = statsParent != null ? statsParent : statsContainer.transform;

        for (int i = 0; i < count; i++)
        {
            TextMeshProUGUI textObj;

            if (statTextPrefab != null)
            {
                textObj = Instantiate(statTextPrefab, parent);
            }
            else
            {
                var go = new GameObject($"StatText_{i}", typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
                textObj = go.GetComponent<TextMeshProUGUI>();
            }

            textObj.text = lines[i];
            spawnedStatTexts.Add(textObj);
        }
    }

    void GetProperties()
    {
        btnRect = buyButton.GetComponent<RectTransform>();
        originalSize = btnRect != null ? btnRect.sizeDelta : Vector2.zero;
        originalText = buyButtonText.text;
        originalColor = buyButtonText.color;
    }

    void SetupProperties(ItemData item)
    {
        if (propertiesText == null)
            return;

        if (item is EquipmentData equip)
        {
            string props = $"Slot: {equip.slot}";

            if (equip.setData != null && !string.IsNullOrEmpty(equip.setData.setName))
                props += $"\nSet: {equip.setData.setName}";

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
        if (requirementText == null || ProfileManager.Instance == null || ShopManager.Instance == null)
            return;

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

    void UpdateVisuals(bool canBuy)
    {
        if (buyButton == null || buyButtonText == null || background == null || ProfileManager.Instance == null)
            return;

        buyButton.interactable = canBuy && !isPurchasing;
        buyButtonText.text = canBuy ? "BUY" : "LOCKED";

        if (!canBuy)
            background.color = lockedColor;
        else
            background.color = normalColor;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!canBuy);
    }

    void UpdateStockDisplay()
    {
        if (currentItem == null || stockText == null || ShopManager.Instance == null)
            return;

        int stockAmount = ShopManager.Instance.GetStock(currentItem.item.itemID);
        stockText.text = stockAmount == -1 ? "Stock: ∞" : $"Stock: {stockAmount}";
    }

    void RefreshVisuals(PlayerProfile profile)
    {
        if (currentItem == null || ShopManager.Instance == null || profile == null)
            return;

        bool canBuy = ShopManager.Instance.CanBuy(currentItem);
        SetupRequirements(currentItem);
        UpdateVisuals(canBuy);
    }

    void ResetVisuals()
    {
        isPurchasing = false;

        if (buyButtonText != null)
        {
            buyButtonText.text = originalText;
            buyButtonText.color = originalColor;
        }

        if (btnRect != null)
            btnRect.sizeDelta = originalSize;

        if (currentItem != null)
            RefreshVisuals(ProfileManager.Instance != null ? ProfileManager.Instance.profile : null);
    }

    public void Close()
    {
        StopAllCoroutines();
        isPurchasing = false;
        gameObject.SetActive(false);
    }

    void OnBuyClicked()
    {
        if (isPurchasing || ShopManager.Instance == null || currentItem == null)
            return;

        if (!ShopManager.Instance.BuyItem(currentItem))
        {
            StartCoroutine(FailureFeedback());
            return;
        }

        isPurchasing = true;
        StartCoroutine(PurchaseFeedback());
    }

    IEnumerator PurchaseFeedback()
    {
        if (!gameObject.activeInHierarchy || buyButtonText == null || btnRect == null)
        {
            isPurchasing = false;
            yield break;
        }

        btnRect.sizeDelta = new Vector2(originalSize.x + 50f, originalSize.y);
        buyButtonText.text = "PURCHASED!";
        buyButtonText.color = Color.green;
        buyButton.interactable = false;
        yield return waitForSeconds0_5;

        if (!gameObject.activeInHierarchy)
        {
            isPurchasing = false;
            yield break;
        }

        btnRect.sizeDelta = originalSize;
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
        if (!gameObject.activeInHierarchy || background == null)
        {
            isPurchasing = false;
            yield break;
        }

        Color original = background.color;

        for (int i = 0; i < 3; i++)
        {
            background.color = Color.red * 0.5f;
            yield return waitForSeconds0_1;

            if (!gameObject.activeInHierarchy)
            {
                isPurchasing = false;
                yield break;
            }

            background.color = original;
            yield return waitForSeconds0_1;
        }

        isPurchasing = false;
    }
}
