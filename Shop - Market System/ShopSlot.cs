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
    private static readonly Color PurchasedTextColor = new(0.934f, 0.897f, 0.824f);
    private static string BuyLabel => Loc.T("BUY", "SATIN AL");

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
            itemNameText.text = shopItem.item.DisplayName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = shopItem.item.DisplayDescription;

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

        if (rarityText != null)
            rarityText.text = item.rarity.Display();

        if (rarityBadge != null)
            rarityBadge.color = item.GetRarityColor();
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
                lines.Add($"{StatType.Damage.Display()}: +{equip.damageBonus}");

            if (equip.defenseBonus > 0)
                lines.Add($"{StatType.Defense.Display()}: +{equip.defenseBonus}");

            if (equip.primaryStatBonus > 0)
                lines.Add($"{equip.primaryStat.Display()}: +{equip.primaryStatBonus}");

            if (equip.secondaryStatBonus > 0)
                lines.Add($"{equip.secondaryStat.Display()}: +{equip.secondaryStatBonus}");
        }
        else if (item is StatModifierItem statMod)
        {
            string sign = statMod.modifyAmount >= 0 ? "+" : "";
            string label = statMod.modifyMaxStat ? $"{Loc.T("Max", "Maks.")} {statMod.targetStat.Display()}" : statMod.targetStat.Display();
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
        if (buyButton != null)
        {
            btnRect = buyButton.GetComponent<RectTransform>();
            originalSize = btnRect != null ? btnRect.sizeDelta : Vector2.zero;
        }

        if (buyButtonText != null)
        {
            originalText = BuyLabel;
            originalColor = buyButtonText.color;
        }
    }

    void SetupProperties(ItemData item)
    {
        if (propertiesText == null)
            return;

        if (item is EquipmentData equip)
        {
            string props = $"{Loc.T("Slot", "Yuva")}: {equip.slot.Display()}";

            if (equip.setData != null && !string.IsNullOrEmpty(equip.setData.setName))
                props += $"\n{Loc.T("Set", "Set")}: {equip.setData.DisplaySetName}";

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
            unmet.Add(Loc.T($"Requires Level {shopItem.requiredLevel}", $"Seviye {shopItem.requiredLevel} gerekli"));

        if (shopItem.requiresFlag && !StoryFlags.Has(shopItem.requiredFlag))
            unmet.Add(Loc.T("Story Progress Required", "Hikâyede ilerleme gerekli"));

        int stockAmount = ShopManager.Instance.GetStock(shopItem.item.itemID);

        if (!shopItem.unlimitedStock && stockAmount <= 0)
            unmet.Add(Loc.T("Out of Stock", "Stokta yok"));

        if (unmet.Count > 0)
        {
            requirementText.gameObject.SetActive(true);
            requirementText.text = string.Join("\n", unmet);
            requirementText.color = UIPalette.Bad;
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
        buyButtonText.text = canBuy ? BuyLabel : Loc.T("LOCKED", "KİLİTLİ");

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
        string stock = Loc.T("Stock", "Stok");
        stockText.text = stockAmount == -1 ? $"{stock}: ∞" : $"{stock}: {stockAmount}";
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

        buyButtonText.text = Loc.T("Purchased!", "Satın Alındı!");
        buyButtonText.color = PurchasedTextColor;
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
        Color flash = Color.Lerp(original, UIPalette.Bad, 0.45f);

        for (int i = 0; i < 3; i++)
        {
            background.color = flash;
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
