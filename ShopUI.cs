using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;
    private readonly List<ShopSlot> slots = new();

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
    public Button switchToSellButton;

    [Header("Scroll Settings")]
    public ScrollRect shopScrollRect;

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
            closeButton.onClick.AddListener(CloseAll);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(Refresh);

        if (switchToSellButton != null)
            switchToSellButton.onClick.AddListener(() =>
            {
                if (MarketUI.Instance != null)
                    MarketUI.Instance.OpenSell();
            });

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (marketClosedPanel != null)
            marketClosedPanel.SetActive(false);

        UpdateCurrency();
        UpdateMarketStatus();
    }

    void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnGoldChanged;
    }

    public void Open()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            Refresh();
        }

        UpdateMarketStatus();
    }

    public void Close()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (marketClosedPanel != null)
            marketClosedPanel.SetActive(false);
    }

    public void CloseAll()
    {
        if (MarketUI.Instance != null)
            MarketUI.Instance.CloseAll();
    }

    public void ShowMarketClosed()
    {
        if (marketClosedPanel != null)
            marketClosedPanel.SetActive(true);
    }

    public void Refresh()
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

    public void RefreshShop() => Refresh();

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
