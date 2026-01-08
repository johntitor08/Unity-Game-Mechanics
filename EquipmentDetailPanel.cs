using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentDetailPanel : MonoBehaviour
{
    public static EquipmentDetailPanel Instance;

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
    public Button unequipButton;
    public Button closeButton;

    private EquipmentData currentEquipment;
    private EquipmentSlot currentSlot;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    void Start()
    {
        if (unequipButton != null)
            unequipButton.onClick.AddListener(Unequip);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void Show(EquipmentData equipment, EquipmentSlot slot)
    {
        currentEquipment = equipment;
        currentSlot = slot;

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
            requirementsText.text = req;
        }

        if (rarityBackground != null)
        {
            Color color = equipment.GetRarityColor();
            color.a = 0.3f;
            rarityBackground.color = color;
        }

        panel.SetActive(true);
    }

    public void Unequip()
    {
        if (currentEquipment != null)
        {
            EquipmentManager.Instance.Unequip(currentSlot);
            Close();
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        currentEquipment = null;
    }
}
