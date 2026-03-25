using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentInfoPanel : MonoBehaviour
{
    public enum PanelMode { Detail, Item }
    public static EquipmentInfoPanel Instance;
    private EquipmentInstance currentInstance;
    private PanelMode currentMode;

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
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
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
            ShowPanel(currentInstance, currentMode);
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
            ShowPanel(currentInstance, currentMode);
    }

    public void ShowPanel(EquipmentInstance instance, PanelMode mode = PanelMode.Detail)
    {
        if (instance == null || instance.baseData == null || EquipmentManager.Instance == null)
            return;

        currentInstance = instance;
        currentMode = mode;
        panel.SetActive(true);
        DisplayEquipment(instance);
        EquipmentInstance slotInst = EquipmentManager.Instance.GetEquipped(instance.baseData.slot);
        bool isEquipped = slotInst != null && slotInst.baseData.itemID == instance.baseData.itemID;

        if (mode == PanelMode.Detail)
        {
            DisplayRequirements(instance.baseData);
            HideComparisonText();
            ConfigureDetailButtons(isEquipped, instance);
        }
        else
        {
            HideRequirementsText();
            DisplayComparison(instance);
            ConfigureItemButton(isEquipped, instance);
        }
    }

    public void ShowPanel(EquipmentData data, PanelMode mode = PanelMode.Detail)
    {
        if (data == null || EquipmentManager.Instance == null)
            return;

        EquipmentInstance live = EquipmentManager.Instance.GetEquipped(data.slot);

        if (live != null && live.baseData.itemID == data.itemID)
            ShowPanel(live, mode);
        else
            ShowPanel(new EquipmentInstance(data, 0), mode);
    }

    void DisplayEquipment(EquipmentInstance instance)
    {
        var data = instance.baseData;

        if (iconImage != null)
            iconImage.sprite = data.icon;

        if (nameText != null)
            nameText.text = instance.GetDisplayName();

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (statsText != null)
            statsText.text = instance.GetStatsDescription();

        if (rarityBackground != null)
        {
            Color c = data.GetRarityColor();
            c.a = 0.3f;
            rarityBackground.color = c;
        }
    }

    void DisplayRequirements(EquipmentData data)
    {
        if (requirementsText == null)
            return;

        string req = $"Level {data.requiredLevel} Required";

        if (data.requiredStatValue > 0)
            req += $"\n{data.requiredStat} {data.requiredStatValue} Required";

        bool meetsLevel = ProfileManager.Instance == null || ProfileManager.Instance.profile.level >= data.requiredLevel;
        bool meetsStat = data.requiredStatValue <= 0 || PlayerStats.Instance == null || PlayerStats.Instance.Get(data.requiredStat) >= data.requiredStatValue;
        requirementsText.text = req;
        requirementsText.color = (meetsLevel && meetsStat) ? Color.green : Color.red;
        requirementsText.gameObject.SetActive(true);
    }

    void HideRequirementsText()
    {
        if (requirementsText != null)
            requirementsText.gameObject.SetActive(false);
    }

    void DisplayComparison(EquipmentInstance incoming)
    {
        if (comparisonText == null)
            return;

        comparisonText.gameObject.SetActive(true);
        EquipmentInstance current = EquipmentManager.Instance.GetEquipped(incoming.baseData.slot);

        if (current == null)
        {
            comparisonText.text = "<color=green>No item equipped in this slot</color>";
            return;
        }

        string text = $"<b>{current.GetDisplayName()}</b>\n";
        text += CompareValue("Damage", current.GetDamageBonus(), incoming.GetDamageBonus());
        text += CompareValue("Defense", current.GetDefenseBonus(), incoming.GetDefenseBonus());
        text += CompareValue(current.baseData.primaryStat.ToString(), current.GetPrimaryBonus(), incoming.GetPrimaryBonus());
        comparisonText.text = text;
    }

    void HideComparisonText()
    {
        if (comparisonText != null)
            comparisonText.gameObject.SetActive(false);
    }

    static string CompareValue(string statName, int current, int newVal)
    {
        if (current == 0 && newVal == 0)
            return "";

        int diff = newVal - current;
        string col = diff > 0 ? "green" : (diff < 0 ? "red" : "white");
        string arrow = diff > 0 ? "↑" : (diff < 0 ? "↓" : "=");
        return $"{statName}: {current} → <color={col}>{newVal} {arrow} {Mathf.Abs(diff)}</color>\n";
    }

    void ConfigureDetailButtons(bool isEquipped, EquipmentInstance instance)
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

    void ConfigureItemButton(bool isEquipped, EquipmentInstance instance)
    {
        if (unequipButton != null)
            unequipButton.gameObject.SetActive(false);

        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(true);
            equipButton.interactable = !isEquipped && EquipmentManager.Instance.CanEquip(instance);
        }
    }

    public void Equip()
    {
        if (currentInstance == null || EquipmentManager.Instance == null || InventoryManager.Instance == null || !EquipmentManager.Instance.Equip(currentInstance))
            return;

        InventoryManager.Instance.RemoveInstance(currentInstance, 1);
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
        panel.SetActive(false);
        currentInstance = null;
    }
}
