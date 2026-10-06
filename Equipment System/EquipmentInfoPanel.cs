using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentInfoPanel : MonoBehaviour
{
    public static EquipmentInfoPanel Instance;
    private EquipmentInstance currentInstance;

    [Header("Panel")]
    public GameObject panel;

    [Header("Display")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI comparisonText;
    public Image rarityBackground;

    [Header("Actions")]
    public Button equipButton;
    public Button unequipButton;
    public Button closeButton;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (panel != null)
            panel.SetActive(false);

        FitToBox(statsText);
        FitToBox(comparisonText);
    }

    static void FitToBox(TextMeshProUGUI text)
    {
        if (text == null || text.enableAutoSizing)
            return;

        text.fontSizeMax = text.fontSize;
        text.fontSizeMin = Mathf.Min(16f, text.fontSize);
        text.enableAutoSizing = true;
        text.overflowMode = TextOverflowModes.Truncate;
    }

    void Start()
    {
        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(Equip);
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.AddListener(Unequip);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnDataChanged;

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += OnDataChanged;

        if (currentInstance != null)
            ShowPanel(currentInstance);
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnDataChanged;

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= OnDataChanged;
    }

    void OnDataChanged()
    {
        if (currentInstance != null && panel != null && panel.activeSelf)
            ShowPanel(currentInstance);
    }

    public void ShowPanel(EquipmentInstance instance)
    {
        if (instance == null || instance.baseData == null || EquipmentManager.Instance == null)
            return;

        currentInstance = instance;
        UIPanelAnimator.Show(panel);
        DisplayEquipment(instance);
        EquipmentInstance slotInst = EquipmentManager.Instance.GetEquipped(instance.baseData.slot);
        bool isEquipped = slotInst != null && slotInst.baseData.itemID == instance.baseData.itemID && slotInst.upgradeLevel == instance.upgradeLevel;
        ConfigureButtons(isEquipped, instance);
        DisplayRequirements(instance.baseData);

        if (isEquipped)
            DisplayDescriptionInComparison(instance.baseData);
        else
            DisplayComparison(instance);
    }

    public void ShowPanel(EquipmentData data)
    {
        if (data == null || EquipmentManager.Instance == null)
            return;

        EquipmentInstance live = EquipmentManager.Instance.GetEquipped(data.slot);

        if (live != null && live.baseData.itemID == data.itemID)
            ShowPanel(live);
        else
            ShowPanel(new EquipmentInstance(data, 0));
    }

    void DisplayEquipment(EquipmentInstance instance)
    {
        var data = instance.baseData;

        if (iconImage != null)
            iconImage.sprite = data.icon;

        if (nameText != null)
            nameText.text = instance.GetDisplayName();

        if (descriptionText != null)
            descriptionText.text = data.DisplayDescription;

        if (statsText != null)
            statsText.text = instance.GetStatsDescription();

        if (rarityBackground != null)
        {
            Color c = data.GetRarityColor();
            c.a = 0.85f;
            rarityBackground.color = c;
        }
    }

    void DisplayRequirements(EquipmentData data)
    {
        if (requirementsText == null)
            return;

        string req = Loc.T($"Level {data.requiredLevel} Required", $"Seviye {data.requiredLevel} gerekli");

        if (data.requiredStatValue > 0)
            req += "\n" + Loc.T($"{data.requiredStat.Display()} {data.requiredStatValue} Required", $"{data.requiredStat.Display()} {data.requiredStatValue} gerekli");

        bool meetsLevel = ProfileManager.Instance == null || ProfileManager.Instance.profile.level >= data.requiredLevel;
        bool meetsStat = data.requiredStatValue <= 0 || PlayerStats.Instance == null || PlayerStats.Instance.Get(data.requiredStat) >= data.requiredStatValue;
        requirementsText.text = req;
        requirementsText.color = (meetsLevel && meetsStat) ? UIPalette.Good : UIPalette.Bad;
        requirementsText.gameObject.SetActive(true);
    }

    void DisplayComparison(EquipmentInstance incoming)
    {
        if (comparisonText == null)
            return;

        comparisonText.gameObject.SetActive(true);
        EquipmentInstance current = EquipmentManager.Instance.GetEquipped(incoming.baseData.slot);

        if (current == null)
        {
            comparisonText.text = $"<color={UIPalette.Hex(UIPalette.Muted)}>{Loc.T("No item equipped in this slot", "Bu yuvada kuşanılmış eşya yok")}</color>";
            return;
        }

        string text = $"<b>{current.GetDisplayName()}</b>\n";
        var stats = new System.Collections.Generic.List<StatType>();

        foreach (var (stat, _) in current.GetStatTotals())
            if (!stats.Contains(stat))
                stats.Add(stat);

        foreach (var (stat, _) in incoming.GetStatTotals())
            if (!stats.Contains(stat))
                stats.Add(stat);

        foreach (StatType stat in stats)
            text += CompareValue(stat.Display(), current.GetStatTotal(stat), incoming.GetStatTotal(stat));

        comparisonText.text = text;
    }

    void DisplayDescriptionInComparison(EquipmentData data)
    {
        if (comparisonText == null)
            return;

        string desc = data.DisplayDescription;
        comparisonText.gameObject.SetActive(!string.IsNullOrEmpty(desc));
        comparisonText.text = $"<i><color={UIPalette.Hex(UIPalette.Muted)}>{desc}</color></i>";
    }

    static string CompareValue(string statName, int current, int newVal)
    {
        if (current == 0 && newVal == 0)
            return "";

        int diff = newVal - current;
        string col = UIPalette.Hex(diff > 0 ? UIPalette.Good : diff < 0 ? UIPalette.Bad : UIPalette.Cream);
        string arrow = diff > 0 ? "↑" : (diff < 0 ? "↓" : "=");
        return $"{statName}: {current} → <color={col}>{newVal} {arrow} {Mathf.Abs(diff)}</color>\n";
    }

    void ConfigureButtons(bool isEquipped, EquipmentInstance instance)
    {
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(!isEquipped);

            if (!isEquipped)
                equipButton.interactable = EquipmentManager.Instance.CanEquip(instance);
        }

        if (unequipButton != null)
            unequipButton.gameObject.SetActive(isEquipped);
    }

    public void Equip()
    {
        if (currentInstance == null || EquipmentManager.Instance == null || !EquipmentManager.Instance.Equip(currentInstance))
            return;

        Close();
    }

    public void Unequip()
    {
        if (currentInstance == null || EquipmentManager.Instance == null)
            return;

        EquipmentManager.Instance.Unequip(currentInstance.baseData.slot, returnToInventory: true);
        Close();
    }

    public void Close()
    {
        UIPanelAnimator.Hide(panel);
        currentInstance = null;
    }
}
