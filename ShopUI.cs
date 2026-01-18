using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

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

    private System.Collections.Generic.List<ShopSlot> slots = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Subscribe to events
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnCurrencyChanged += UpdateCurrency;

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged += UpdateMarketStatus;

        // Setup buttons
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshShop);

        // Initial setup
        shopPanel?.SetActive(false);
        RefreshShop();
        UpdateCurrency();
        UpdateMarketStatus(TimePhaseManager.Instance?.currentPhase ?? TimePhase.Morning);
    }

    public void OpenShop()
    {
        // Check if market is open
        if (IsMarketClosed())
        {
            if (marketClosedPanel != null)
                marketClosedPanel.SetActive(true);
            return;
        }

        shopPanel?.SetActive(true);
        RefreshShop();
        UpdateCurrency();
    }

    public void CloseShop()
    {
        shopPanel?.SetActive(false);
        marketClosedPanel?.SetActive(false);
    }

    bool IsMarketClosed()
    {
        if (TimePhaseManager.Instance == null) return false;

        TimePhase phase = TimePhaseManager.Instance.currentPhase;
        // You can customize this based on your MarketController settings
        return phase == TimePhase.Evening || phase == TimePhase.Night;
    }

    public void RefreshShop()
    {
        if (ShopManager.Instance == null) return;

        int index = 0;
        foreach (var shopItem in ShopManager.Instance.shopItems)
        {
            if (index >= slots.Count)
            {
                var slot = Instantiate(shopSlotPrefab, shopContent);
                slots.Add(slot);
            }

            slots[index].Setup(shopItem);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        // Hide unused slots
        for (int i = index; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);

        // Scroll to top
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    void UpdateCurrency()
    {
        if (ProfileManager.Instance != null && currencyText != null)
        {
            int currency = ProfileManager.Instance.profile.currency;
            currencyText.text = $"{currency} Gold";
        }
    }

    void UpdateMarketStatus(TimePhase phase)
    {
        if (marketStatusText == null) return;

        bool isOpen = phase == TimePhase.Morning || phase == TimePhase.Noon;
        string status = isOpen ? "Market Open" : "Market Closed";
        string timeString = phase.ToString();

        marketStatusText.text = $"{status} - {timeString}";
        marketStatusText.color = isOpen ? Color.green : Color.red;
    }
}
