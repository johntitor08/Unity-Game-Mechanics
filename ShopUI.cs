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
    public ScrollRect shopScrollRect;
    public ScrollRect sellScrollRect;

    [Header("Sell Panel")]
    public GameObject sellPanel;
    public Transform sellContent;
    public SellSlot sellSlotPrefab;

    [Header("Sell Settings")]
    [Range(0f, 1f)]
    public float sellPriceRatio = 0.5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += OnGoldChanged;

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshShop);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (sellPanel != null)
            sellPanel.SetActive(false);

        UpdateCurrency();
        UpdateMarketStatus();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnGoldChanged;
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
        if (ShopManager.Instance == null)
            return;

        foreach (var slot in slots)
            if (slot != null) slot.gameObject.SetActive(false);

        int index = 0;

        foreach (var shopItem in ShopManager.Instance.shopItems)
        {
            if (index >= slots.Count)
                slots.Add(Instantiate(shopSlotPrefab, shopContent));

            slots[index].Setup(shopItem);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
            if (slots[i] != null) slots[i].gameObject.SetActive(false);

        ScrollToTop(shopScrollRect);
        UpdateCurrency();
    }

    public void RefreshSellPanel()
    {
        if (InventoryManager.Instance == null || sellPanel == null)
            return;

        int index = 0;

        foreach (var pair in InventoryManager.Instance.GetItems())
        {
            if (index >= sellSlots.Count)
                sellSlots.Add(Instantiate(sellSlotPrefab, sellContent));

            if (ItemDatabase.Instance == null)
                continue;

            ItemData item = ItemDatabase.Instance.GetByID(pair.Key);

            if (item != null)
            {
                sellSlots[index].Setup(item, pair.Value, sellPriceRatio);
                sellSlots[index].gameObject.SetActive(true);
                index++;
            }
        }

        for (int i = index; i < sellSlots.Count; i++)
            if (sellSlots[i] != null) sellSlots[i].gameObject.SetActive(false);

        ScrollToTop(sellScrollRect);
    }

    void OnGoldChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        if (type == CurrencyType.Gold)
            UpdateCurrency();
    }

    void UpdateCurrency()
    {
        if (currencyText == null)
            return;

        int gold = CurrencyManager.Instance != null ? CurrencyManager.Instance.Get(CurrencyType.Gold) : 0;
        currencyText.text = $"{gold} Gold";
    }

    public void UpdateMarketStatus()
    {
        if (marketStatusText == null)
            return;

        bool isOpen = MarketController.Instance == null || MarketController.Instance.IsOpen();
        marketStatusText.text = isOpen ? "Market Open" : "Market Closed";
        marketStatusText.color = isOpen ? Color.green : Color.red;
    }

    private static void ScrollToTop(ScrollRect sr)
    {
        if (sr == null)
            return;

        Canvas.ForceUpdateCanvases();
        sr.verticalNormalizedPosition = 1f;
    }
}
