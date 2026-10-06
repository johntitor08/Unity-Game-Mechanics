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
        SetClosedState(false);

        if (shopPanel != null)
        {
            UIPanelAnimator.Show(shopPanel);
            Refresh();
        }

        UpdateMarketStatus();
    }

    public void Close()
    {
        UIPanelAnimator.Hide(shopPanel);
        UIPanelAnimator.Hide(marketClosedPanel);
    }

    public void CloseAll()
    {
        if (MarketUI.Instance != null)
            MarketUI.Instance.CloseAll();
    }

    public void ShowMarketClosed()
    {
        SetClosedState(true);

        if (shopPanel != null)
            UIPanelAnimator.Show(shopPanel);

        UIPanelAnimator.Show(marketClosedPanel);
        SetClosedMessage();
        UpdateMarketStatus();
    }

    void SetClosedMessage()
    {
        if (marketClosedPanel == null)
            return;

        string muted = UIPalette.Hex(UIPalette.Muted);
        string en = $"Market Closed\n<size=55%><color={muted}>The stalls open again in the morning.</color></size>";
        string tr = $"Pazar Kapalı\n<size=55%><color={muted}>Tezgâhlar sabah yeniden açılır.</color></size>";
        var localized = marketClosedPanel.GetComponentInChildren<LocalizedText>(true);

        if (localized != null)
        {
            localized.en = en;
            localized.tr = tr;
            localized.Apply();
            return;
        }

        var text = marketClosedPanel.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
            text.text = Loc.T(en, tr);
    }

    void SetClosedState(bool closed)
    {
        if (shopScrollRect != null)
            shopScrollRect.gameObject.SetActive(!closed);

        if (refreshButton != null)
            refreshButton.gameObject.SetActive(!closed);

        if (switchToSellButton != null)
            switchToSellButton.gameObject.SetActive(!closed);

        if (!closed && marketClosedPanel != null)
            marketClosedPanel.SetActive(false);
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
        currencyText.text = $"{gold} {Loc.T("Gold", "Altın")}";
    }

    public void UpdateMarketStatus()
    {
        if (marketStatusText == null)
            return;

        bool isOpen = MarketController.Instance == null || MarketController.Instance.IsOpen();
        marketStatusText.text = isOpen ? Loc.T("Market Open", "Pazar Açık") : Loc.T("Market Closed", "Pazar Kapalı");
        marketStatusText.color = isOpen ? UIPalette.Good : UIPalette.Bad;
    }

    private static void ScrollToTop(ScrollRect sr)
    {
        if (sr == null)
            return;

        Canvas.ForceUpdateCanvases();
        sr.verticalNormalizedPosition = 1f;
    }
}
