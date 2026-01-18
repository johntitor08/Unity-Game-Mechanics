using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
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

    [Header("Stats Display")]
    public GameObject statsContainer;
    public TextMeshProUGUI[] statTexts; // For displaying item stats

    [Header("Interaction")]
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    [Header("Locked State")]
    public GameObject lockedOverlay;
    public TextMeshProUGUI lockReasonText;

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

    private ShopItemData currentItem;

    void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
    }

    public void Setup(ShopItemData shopItem)
    {
        currentItem = shopItem;

        // Basic info
        if (itemIcon != null && shopItem.item.icon != null)
            itemIcon.sprite = shopItem.item.icon;

        if (itemNameText != null)
            itemNameText.text = shopItem.item.itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = shopItem.item.description;

        if (priceText != null)
            priceText.text = $"{shopItem.price}";

        // Stock
        int stock = ShopManager.Instance.GetStock(shopItem.item.itemID);
        if (stockText != null)
            stockText.text = stock == -1 ? "∞" : $"Stock: {stock}";

        // Rarity
        SetupRarity(shopItem.item);

        // Requirements
        SetupRequirements(shopItem);

        // Check if can buy
        bool canBuy = ShopManager.Instance.CanBuy(shopItem);
        bool hasEnoughGold = ProfileManager.Instance.profile.currency >= shopItem.price;

        UpdateVisuals(canBuy, hasEnoughGold);
    }

    void SetupRarity(ItemData item)
    {
        // Assuming ItemData has a rarity field
        // If not, you can skip this or add it to ItemData
        if (rarityBadge != null && rarityText != null)
        {
            // Example: determine rarity based on item name or add rarity field
            string rarity = "Common"; // Default
            Color rarityColor = commonColor;

            // You can add rarity logic here
            // For now, just showing the setup

            rarityText.text = rarity;
            rarityBadge.color = rarityColor;
        }
    }

    void SetupRequirements(ShopItemData shopItem)
    {
        if (requirementText == null) return;

        string requirement = "";

        if (shopItem.requiredLevel > 1)
            requirement = $"Requires Level {shopItem.requiredLevel}";

        if (shopItem.requiresFlag)
            requirement += (requirement.Length > 0 ? "\n" : "") + "Special unlock required";

        requirementText.text = requirement;
        requirementText.gameObject.SetActive(requirement.Length > 0);
    }

    void UpdateVisuals(bool canBuy, bool hasEnoughGold)
    {
        // Update button
        if (buyButton != null)
        {
            buyButton.interactable = canBuy;

            if (buyButtonText != null)
            {
                if (!canBuy)
                    buyButtonText.text = hasEnoughGold ? "LOCKED" : "TOO EXPENSIVE";
                else
                    buyButtonText.text = "BUY";
            }
        }

        // Update background color
        if (background != null)
        {
            if (!canBuy)
                background.color = lockedColor;
            else if (hasEnoughGold)
                background.color = canAffordColor * 0.3f; // Subtle tint
            else
                background.color = cannotAffordColor * 0.3f;
        }

        // Show/hide locked overlay
        if (lockedOverlay != null)
        {
            bool isLocked = !canBuy && hasEnoughGold; // Locked due to requirements
            lockedOverlay.SetActive(isLocked);

            if (isLocked && lockReasonText != null)
            {
                if (ProfileManager.Instance.profile.level < currentItem.requiredLevel)
                    lockReasonText.text = $"Level {currentItem.requiredLevel} Required";
                else if (currentItem.requiresFlag)
                    lockReasonText.text = "Story Progress Required";
            }
        }

        // Update price text color
        if (priceText != null)
            priceText.color = hasEnoughGold ? Color.white : Color.red;
    }

    void OnBuyClicked()
    {
        if (ShopManager.Instance.BuyItem(currentItem))
        {
            // Success feedback
            StartCoroutine(PurchaseFeedback());
            ShopUI.Instance.RefreshShop();
        }
        else
        {
            // Failure feedback
            StartCoroutine(FailureFeedback());
        }
    }

    System.Collections.IEnumerator PurchaseFeedback()
    {
        if (buyButtonText != null)
        {
            string originalText = buyButtonText.text;
            buyButtonText.text = "PURCHASED!";
            buyButtonText.color = Color.green;

            yield return _waitForSeconds0_5;

            buyButtonText.text = originalText;
            buyButtonText.color = Color.white;
        }
    }

    System.Collections.IEnumerator FailureFeedback()
    {
        if (background != null)
        {
            Color originalColor = background.color;

            for (int i = 0; i < 3; i++)
            {
                background.color = Color.red * 0.5f;
                yield return _waitForSeconds0_1;
                background.color = originalColor;
                yield return _waitForSeconds0_1;
            }
        }
    }
}
