using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellUI : MonoBehaviour
{
    public static SellUI Instance;
    private readonly List<SellSlot> slots = new();

    [Header("Panel")]
    public GameObject sellPanel;

    [Header("Content")]
    public Transform sellContent;
    public SellSlot sellSlotPrefab;

    [Header("Header Elements")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI titleText;

    [Header("Footer Elements")]
    public Button closeButton;
    public Button switchToShopButton;

    [Header("Scroll")]
    public ScrollRect sellScrollRect;

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
            closeButton.onClick.AddListener(() =>
            {
                if (MarketUI.Instance != null)
                    MarketUI.Instance.CloseAll();
            });

        if (switchToShopButton != null)
            switchToShopButton.onClick.AddListener(() =>
            {
                if (MarketUI.Instance != null)
                    MarketUI.Instance.OpenShop();
            });

        if (sellPanel != null)
            sellPanel.SetActive(false);

        UpdateCurrency();
    }

    void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnGoldChanged;
    }

    public void Open()
    {
        if (sellPanel != null)
        {
            sellPanel.SetActive(true);
            Refresh();
        }
    }

    public void Close()
    {
        if (sellPanel != null)
            sellPanel.SetActive(false);
    }

    public void Refresh()
    {
        if (InventoryManager.Instance == null || sellPanel == null)
            return;

        int index = 0;

        foreach (var (inst, qty) in InventoryManager.Instance.GetEquipmentInstances())
        {
            EnsureSlot(index);
            slots[index].SetupEquipment(inst.baseData, inst.upgradeLevel, qty, sellPriceRatio);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        foreach (var (item, qty) in InventoryManager.Instance.GetNonEquipmentItems())
        {
            EnsureSlot(index);
            slots[index].Setup(item, qty, sellPriceRatio);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
            if (slots[i] != null) slots[i].gameObject.SetActive(false);

        ScrollToTop(sellScrollRect);
        UpdateCurrency();
    }

    void EnsureSlot(int index)
    {
        if (index >= slots.Count)
            slots.Add(Instantiate(sellSlotPrefab, sellContent));
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

    private static void ScrollToTop(ScrollRect sr)
    {
        if (sr == null)
            return;

        Canvas.ForceUpdateCanvases();
        sr.verticalNormalizedPosition = 1f;
    }
}
