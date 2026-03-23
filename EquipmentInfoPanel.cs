using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentInfoPanel : MonoBehaviour
{
    public enum PanelMode { Detail, Item }
    public static EquipmentInfoPanel Instance;
    private EquipmentData currentEquipment;
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
        if (currentEquipment != null)
            ShowPanel(currentEquipment, currentMode);
    }

    public void ShowPanel(EquipmentData equipment, PanelMode mode = PanelMode.Detail)
    {
        if (equipment == null || EquipmentManager.Instance == null)
            return;

        currentEquipment = equipment;
        currentMode = mode;
        panel.SetActive(true);
        DisplayEquipment(equipment);

        bool isEquipped = EquipmentManager.Instance.GetEquipped(equipment.slot) == equipment;

        if (mode == PanelMode.Detail)
        {
            DisplayRequirements(equipment);
            HideComparisonText();
            ConfigureDetailButtons(isEquipped, equipment);
        }
        else
        {
            HideRequirementsText();
            DisplayComparison(equipment);
            ConfigureItemButton(isEquipped, equipment);
        }
    }

    private void DisplayEquipment(EquipmentData equipment)
    {
        if (iconImage != null)
            iconImage.sprite = equipment.icon;

        if (nameText != null)
        {
            nameText.text = equipment.itemName;
            nameText.color = equipment.GetRarityColor();
        }

        if (descriptionText != null)
            descriptionText.text = equipment.description;

        if (statsText != null)
            statsText.text = equipment.GetStatsDescription();

        if (rarityBackground != null)
        {
            Color color = equipment.GetRarityColor();
            color.a = 0.3f;
            rarityBackground.color = color;
        }
    }

    private void DisplayRequirements(EquipmentData equipment)
    {
        if (requirementsText == null)
            return;

        string req = $"Level {equipment.requiredLevel} Required";

        if (equipment.requiredStatValue > 0)
            req += $"\n{equipment.requiredStat} {equipment.requiredStatValue} Required";

        bool meetsRequirements = EquipmentManager.Instance.CanEquip(equipment);
        requirementsText.text = req;
        requirementsText.color = meetsRequirements ? Color.green : Color.red;
        requirementsText.gameObject.SetActive(true);
    }

    private void HideRequirementsText()
    {
        if (requirementsText != null)
            requirementsText.gameObject.SetActive(false);
    }

    private void DisplayComparison(EquipmentData newEquipment)
    {
        if (comparisonText == null)
            return;

        comparisonText.gameObject.SetActive(true);
        EquipmentData currentEquipped = EquipmentManager.Instance.GetEquipped(newEquipment.slot);

        if (currentEquipped == null)
        {
            comparisonText.text = "<color=green>No item equipped in this slot</color>";
            return;
        }

        string comparison = $"<b>Currently Equipped: {currentEquipped.itemName}</b>\n\n";
        comparison += CompareValue("Damage", currentEquipped.damageBonus, newEquipment.damageBonus);
        comparison += CompareValue("Defense", currentEquipped.defenseBonus, newEquipment.defenseBonus);
        comparison += CompareValue(currentEquipped.primaryStat.ToString(), currentEquipped.primaryStatBonus, newEquipment.primaryStatBonus);
        comparisonText.text = comparison;
    }

    private void HideComparisonText()
    {
        if (comparisonText != null)
            comparisonText.gameObject.SetActive(false);
    }

    private static string CompareValue(string statName, int current, int newValue)
    {
        if (current == 0 && newValue == 0)
            return "";

        int difference = newValue - current;
        string color = difference > 0 ? "green" : (difference < 0 ? "red" : "white");
        string arrow = difference > 0 ? "↑" : (difference < 0 ? "↓" : "=");
        return $"{statName}: {current} → <color={color}>{newValue} {arrow}{Mathf.Abs(difference)}</color>\n";
    }

    private void ConfigureDetailButtons(bool isEquipped, EquipmentData equipment)
    {
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(!isEquipped);

            if (!isEquipped)
                equipButton.interactable = EquipmentManager.Instance.CanEquip(equipment);
        }

        if (unequipButton != null)
            unequipButton.gameObject.SetActive(isEquipped);
    }

    private void ConfigureItemButton(bool isEquipped, EquipmentData equipment)
    {
        if (unequipButton != null)
            unequipButton.gameObject.SetActive(false);

        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(true);
            equipButton.interactable = !isEquipped && EquipmentManager.Instance.CanEquip(equipment);
        }
    }

    public void Equip()
    {
        if (currentEquipment == null || EquipmentManager.Instance == null)
            return;

        if (!EquipmentManager.Instance.Equip(currentEquipment))
            return;

        InventoryManager.Instance.RemoveItem(currentEquipment, 1);
        Close();
    }

    public void Unequip()
    {
        if (currentEquipment == null || EquipmentManager.Instance == null)
            return;

        if (!EquipmentManager.Instance.Unequip(currentEquipment.slot))
            return;

        InventoryManager.Instance.AddItem(currentEquipment, 1);
        Close();
    }

    public void Close()
    {
        panel.SetActive(false);
        currentEquipment = null;
    }
}
