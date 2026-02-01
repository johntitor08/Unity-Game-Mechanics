using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentItemPanel : MonoBehaviour
{
    public static EquipmentItemPanel Instance;
    private EquipmentData currentEquipment;

    [Header("Panel")]
    public GameObject panel;

    [Header("Display")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI comparisonText;
    public Image rarityBackground;

    [Header("Actions")]
    public Button equipButton;
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
            equipButton.onClick.AddListener(EquipItem);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    void OnEnable()
    {
        if (currentEquipment == null) return;
        ShowItemPanel(currentEquipment);
    }

    public void ShowItemPanel(EquipmentData equipment)
    {
        if (equipment == null) return;
        currentEquipment = equipment;
        DisplayEquipment(equipment);
        ShowComparison(equipment);
        ConfigureEquipButton(equipment);
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

    private void ShowComparison(EquipmentData newEquipment)
    {
        if (comparisonText == null || EquipmentManager.Instance == null) return;
        EquipmentData currentEquipped = EquipmentManager.Instance.GetEquipped(newEquipment.slot);

        if (currentEquipped == null)
        {
            comparisonText.text = "<color=green>No item equipped in this slot</color>";
            return;
        }

        string comparison = $"<b>Currently Equipped: {currentEquipped.itemName}</b>\n\n";
        comparison += CompareValue("Damage", currentEquipped.damageBonus, newEquipment.damageBonus);
        comparison += CompareValue("Defense", currentEquipped.defenseBonus, newEquipment.defenseBonus);
        comparison += CompareValue(currentEquipped.primaryStat.ToString(),
                                   currentEquipped.primaryStatBonus,
                                   newEquipment.primaryStatBonus);

        comparisonText.text = comparison;
    }

    private string CompareValue(string statName, int current, int newValue)
    {
        if (current == 0 && newValue == 0) return "";
        int difference = newValue - current;
        string color = difference > 0 ? "green" : (difference < 0 ? "red" : "white");
        string arrow = difference > 0 ? "↑" : (difference < 0 ? "↓" : "=");
        return $"{statName}: {current} → <color={color}>{newValue} {arrow}{Mathf.Abs(difference)}</color>\n";
    }

    private void ConfigureEquipButton(EquipmentData equipment)
    {
        if (equipButton == null || EquipmentManager.Instance == null) return;
        bool isEquipped = EquipmentManager.Instance.GetEquipped(equipment.slot) == equipment;
        bool canEquip = EquipmentManager.Instance.CanEquip(equipment);
        equipButton.interactable = canEquip && !isEquipped;
    }

    public void EquipItem()
    {
        if (currentEquipment == null || EquipmentManager.Instance == null) return;
        bool success = EquipmentManager.Instance.Equip(currentEquipment);

        if (success)
        {
            if (InventoryManager.Instance == null) return;
            InventoryManager.Instance.RemoveItem(currentEquipment, 1);
            Close();
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        currentEquipment = null;
    }
}
