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
        ProfileManager.Instance.OnCurrencyChanged += UpdateCurrency;
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (refreshButton != null) refreshButton.onClick.AddListener(RefreshShop);
        shopPanel.SetActive(false);
        RefreshShop();
        UpdateCurrency();
        UpdateMarketStatus();
    }

    public void OpenShop()
    {
        if (!MarketController.Instance.IsOpen())
        {
            marketClosedPanel.SetActive(true);
            return;
        }

        shopPanel.SetActive(true);
        RefreshShop();
        UpdateCurrency();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        marketClosedPanel.SetActive(false);
    }

    public void OpenSell()
    {
        shopPanel.SetActive(false);
        sellPanel.SetActive(true);
        RefreshSellPanel();
    }

    public void RefreshShop()
    {
        if (ShopManager.Instance == null) return;
        int index = 0;

        foreach (ShopSlot slot in slots)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (var shopItem in ShopManager.Instance.shopItems)
        {
            if (index >= slots.Count)
                slots.Add(Instantiate(shopSlotPrefab, shopContent));

            slots[index].Setup(shopItem);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);

        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void RefreshSellPanel()
    {
        int index = 0;

        foreach (var pair in InventoryManager.Instance.GetItems())
        {
            if (index >= sellSlots.Count)
                sellSlots.Add(Instantiate(sellSlotPrefab, sellContent));

            ItemData item = ItemDatabase.Instance.GetByID(pair.Key);
            int qty = pair.Value;

            sellSlots[index].Setup(item, qty, 0.5f);
            sellSlots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < sellSlots.Count; i++)
            sellSlots[i].gameObject.SetActive(false);
    }

    void UpdateCurrency()
    {
        currencyText.text = $"{ProfileManager.Instance.profile.currency} Gold";
    }

    public void UpdateMarketStatus()
    {
        bool isOpen = true;
        
        if (MarketController.Instance != null)
            isOpen = MarketController.Instance.IsOpen();

        marketStatusText.text = isOpen ? "Market Open" : "Market Closed";
        marketStatusText.color = isOpen ? Color.green : Color.red;
    }
}
