using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance;

    [Header("Currency Displays")]
    public List<CurrencyDisplay> currencyDisplays = new();

    [System.Serializable]
    public class CurrencyDisplay
    {
        public CurrencyType type;
        public GameObject container;
        public Image icon;
        public TextMeshProUGUI amountText;
        public bool animateOnChange = true;
    }

    private readonly Dictionary<CurrencyType, CurrencyDisplay> displayDict = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var display in currencyDisplays)
            displayDict[display.type] = display;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;

        RefreshAll();
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        UpdateDisplay(type, newAmount);

        if (displayDict.ContainsKey(type) && displayDict[type].animateOnChange)
            StartCoroutine(AnimateCountUp(displayDict[type].amountText, oldAmount, newAmount, 0.5f));
    }

    public void UpdateMultiple(Dictionary<CurrencyType, int> newAmounts)
    {
        foreach (var kvp in newAmounts)
        {
            UpdateDisplay(kvp.Key, kvp.Value);
        }
    }

    void UpdateDisplay(CurrencyType type, int amount)
    {
        if (!displayDict.ContainsKey(type)) return;
        var display = displayDict[type];

        if (display.amountText != null)
            display.amountText.text = FormatCurrency(amount);

        if (display.icon != null)
        {
            var currencyInfo = CurrencyManager.Instance.GetCurrencyInfo(type);
            if (currencyInfo != null && currencyInfo.icon != null)
                display.icon.sprite = currencyInfo.icon;
        }
    }

    System.Collections.IEnumerator AnimateCountUp(TextMeshProUGUI text, int start, int end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            int current = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            text.text = FormatCurrency(current);
            yield return null;
        }

        text.text = FormatCurrency(end);
    }

    string FormatCurrency(int amount)
    {
        if (amount >= 1000000)
            return $"{amount / 1000000f:F1}M";
        else if (amount >= 1000)
            return $"{amount / 1000f:F1}K";
        else
            return amount.ToString();
    }

    void RefreshAll()
    {
        foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
        {
            int amount = CurrencyManager.Instance.Get(type);
            UpdateDisplay(type, amount);
        }
    }
}
