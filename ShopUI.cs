using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;
    private readonly List<ShopSlot> slots = new();
    private readonly List<SellSlot> sellSlots = new();

    [Header("Main Panels")]
    public GameObject shopPanel;
    public GameObject marketClosedPanel;

    [Header("Shop Content")]
    public Transform shopContent;
    public ShopSlot shopSlotPrefab;

    [Header("Header Elements")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI shopTitleText;

    [Header("Footer Elements")]
    public TextMeshProUGUI marketStatusText;
    public Button closeButton;
    public Button refreshButton;

    [Header("Scroll Settings")]
    public ScrollRect scrollRect;

    [Header("Sell Panel")]
    public GameObject sellPanel;
    public Transform sellContent;
    public SellSlot sellSlotPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnCurrencyChanged += UpdateCurrency;
            ProfileManager.Instance.OnProfileChanged += UpdateCurrency;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshShop);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (sellPanel != null)
            sellPanel.SetActive(false);

        OpenShop();
        UpdateCurrency(ProfileManager.Instance != null ? ProfileManager.Instance.profile : null);
        UpdateMarketStatus();
    }

    void OnDestroy()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnCurrencyChanged -= UpdateCurrency;
            ProfileManager.Instance.OnProfileChanged -= UpdateCurrency;
        }
    }

    public void OpenShop()
    {
        if (MarketController.Instance != null && !MarketController.Instance.IsOpen())
        {
            if (marketClosedPanel != null)
                marketClosedPanel.SetActive(true);

            return;
        }

        if (sellPanel != null)
            sellPanel.SetActive(false);

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            RefreshShop();
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (marketClosedPanel != null)
            marketClosedPanel.SetActive(false);
    }

    public void OpenSell()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (sellPanel != null)
        {
            sellPanel.SetActive(true);
            RefreshSellPanel();
        }
    }

    public void RefreshShop()
    {
        if (ShopManager.Instance == null) return;

        foreach (ShopSlot slot in slots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }

        int index = 0;

        foreach (var shopItem in ShopManager.Instance.shopItems)
        {
            if (index >= slots.Count)
            {
                ShopSlot newSlot = Instantiate(shopSlotPrefab, shopContent);
                slots.Add(newSlot);
            }

            slots[index].Setup(shopItem);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
        {
            if (slots[i] != null)
                slots[i].gameObject.SetActive(false);
        }

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        UpdateCurrency(ProfileManager.Instance != null ? ProfileManager.Instance.profile : null);
    }

    public void RefreshSellPanel()
    {
        if (InventoryManager.Instance == null || sellPanel == null) return;
        int index = 0;

        foreach (var pair in InventoryManager.Instance.GetItems())
        {
            if (index >= sellSlots.Count)
            {
                SellSlot newSlot = Instantiate(sellSlotPrefab, sellContent);
                sellSlots.Add(newSlot);
            }

            ItemData item = ItemDatabase.Instance.GetByID(pair.Key);

            if (item != null)
            {
                int qty = pair.Value;
                sellSlots[index].Setup(item, qty, 0.5f);
                sellSlots[index].gameObject.SetActive(true);
                index++;
            }
        }

        for (int i = index; i < sellSlots.Count; i++)
        {
            if (sellSlots[i] != null)
                sellSlots[i].gameObject.SetActive(false);
        }

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    void UpdateCurrency(PlayerProfile profile)
    {
        if (profile == null || currencyText == null) return;
        currencyText.text = $"{profile.currency} Gold";
    }

    public void UpdateMarketStatus()
    {
        if (marketStatusText == null) return;
        bool isOpen = true;

        if (MarketController.Instance != null)
        {
            isOpen = MarketController.Instance.IsOpen();
        }

        marketStatusText.text = isOpen ? "Market Open" : "Market Closed";
        marketStatusText.color = isOpen ? Color.green : Color.red;
    }
}
