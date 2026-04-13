using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance;
    private readonly Dictionary<CurrencyType, Coroutine> animationCoroutines = new();
    private readonly Dictionary<CurrencyType, CurrencyDisplay> displayDict = new();
    public GameObject panel;
    private bool _subscribed;
    private bool gameStarted = false;

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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (var display in currencyDisplays)
            displayDict[display.type] = display;
    }

    void Start()
    {
        Subscribe();
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.C))
            panel.SetActive(!panel.activeSelf);
    }

    void OnEnable()
    {
        if (_subscribed)
            Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (CurrencyManager.Instance == null)
            return;

        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        _subscribed = true;
        RefreshAll();
    }

    private void Unsubscribe()
    {
        if (CurrencyManager.Instance == null)
            return;

        CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    public void OnGameStarted()
    {
        gameStarted = true;
    }

    void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        UpdateDisplay(type, newAmount);

        if (displayDict.TryGetValue(type, out var display) && display.animateOnChange)
        {
            if (animationCoroutines.TryGetValue(type, out var existing) && existing != null)
                StopCoroutine(existing);

            animationCoroutines[type] = StartCoroutine(
                AnimateCountUp(display.amountText, oldAmount, newAmount, 0.5f)
            );
        }
    }

    public void UpdateMultiple(Dictionary<CurrencyType, int> newAmounts)
    {
        foreach (var kvp in newAmounts)
            UpdateDisplay(kvp.Key, kvp.Value);
    }

    void UpdateDisplay(CurrencyType type, int amount)
    {
        if (!displayDict.TryGetValue(type, out var display))
            return;

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
        if (start == end)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            text.text = FormatCurrency(Mathf.RoundToInt(Mathf.Lerp(start, end, t)));
            yield return null;
        }

        text.text = FormatCurrency(end);
    }

    string FormatCurrency(int amount)
    {
        if (amount >= 1_000_000)
            return $"{amount / 1_000_000f:F1}M";
        else if (amount >= 1_000)
            return $"{amount / 1_000f:F1}K";
        else
            return amount.ToString();
    }

    void RefreshAll()
    {
        foreach (var type in displayDict.Keys)
        {
            int amount = CurrencyManager.Instance.Get(type);
            UpdateDisplay(type, amount);
        }
    }
}
