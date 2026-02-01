using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentDetailPanel : MonoBehaviour
{
    public static EquipmentDetailPanel Instance;
    private EquipmentData currentEquipment;
    private EquipmentSlot currentSlot;

    [Header("Panel")]
    public GameObject panel;

    [Header("Display")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI requirementsText;
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
            equipButton.onClick.AddListener(Equip);

        if (unequipButton != null)
            unequipButton.onClick.AddListener(Unequip);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    void OnEnable()
    {
        if (currentEquipment == null) return;
        ShowDetailPanel(currentEquipment);
    }

    public void ShowDetailPanel(EquipmentData equipment)
    {
        if (EquipmentManager.Instance == null)
            return;

        EquipmentSlot slot = equipment.slot;
        EquipmentData currentlyEquipped = EquipmentManager.Instance.GetEquipped(slot);
        bool isEquipped = currentlyEquipped == equipment;

        currentEquipment = equipment;
        currentSlot = slot;

        DisplayEquipment(equipment);
        ConfigureButtons(isEquipped);
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

        if (requirementsText != null)
        {
            string req = $"Level {equipment.requiredLevel} Required";

            if (equipment.requiredStatValue > 0)
                req += $"\n{equipment.requiredStat} {equipment.requiredStatValue} Required";

            // Check if player meets requirements
            bool meetsRequirements = false;

            if (EquipmentManager.Instance != null)
                meetsRequirements = EquipmentManager.Instance.CanEquip(equipment);

            requirementsText.text = req;
            requirementsText.color = meetsRequirements ? Color.green : Color.red;
        }

        if (rarityBackground != null)
        {
            Color color = equipment.GetRarityColor();
            color.a = 0.3f;
            rarityBackground.color = color;
        }
    }

    private void ConfigureButtons(bool equipped)
    {
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(!equipped);

            if (!equipped && EquipmentManager.Instance != null)
            {
                bool canEquip = EquipmentManager.Instance.CanEquip(currentEquipment);
                equipButton.interactable = canEquip;
            }
        }

        if (unequipButton != null)
        {
            unequipButton.gameObject.SetActive(equipped);
        }
    }

    public void Equip()
    {
        if (currentEquipment != null && EquipmentManager.Instance != null)
        {
            bool success = EquipmentManager.Instance.Equip(currentEquipment);

            if (success)
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.RemoveItem(currentEquipment, 1);

                Close();
            }
        }
    }

    public void Unequip()
    {
        if (currentEquipment != null && EquipmentManager.Instance != null)
        {
            bool success = EquipmentManager.Instance.Unequip(currentSlot);

            if (success)
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.AddItem(currentEquipment, 1);

                Close();
            }
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        currentEquipment = null;
    }
}
