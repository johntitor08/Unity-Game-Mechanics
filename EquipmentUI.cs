using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI Instance;

    [Header("Panels")]
    public GameObject equipmentPanel;

    [Header("Equipment Slots")]
    public EquipmentSlotUI weaponSlot;
    public EquipmentSlotUI armorSlot;
    public EquipmentSlotUI helmetSlot;
    public EquipmentSlotUI accessorySlot;
    public EquipmentSlotUI shieldSlot;
    public EquipmentSlotUI bootsSlot;

    [Header("Stats Display")]
    public TextMeshProUGUI totalDamageText;
    public TextMeshProUGUI totalDefenseText;
    public Transform setBonusesParent;
    public TextMeshProUGUI setBonusTextPrefab;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.E;

    private Dictionary<EquipmentSlot, EquipmentSlotUI> slotUIMap;
    private readonly List<TextMeshProUGUI> setBonusTexts = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Initialize slot map
        slotUIMap = new Dictionary<EquipmentSlot, EquipmentSlotUI>
        {
            { EquipmentSlot.Weapon, weaponSlot },
            { EquipmentSlot.Armor, armorSlot },
            { EquipmentSlot.Helmet, helmetSlot },
            { EquipmentSlot.Accessory, accessorySlot },
            { EquipmentSlot.Shield, shieldSlot },
            { EquipmentSlot.Boots, bootsSlot }
        };

        // Setup each slot
        foreach (var kvp in slotUIMap)
        {
            if (kvp.Value != null)
                kvp.Value.Setup(kvp.Key);
        }

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
        }

        equipmentPanel.SetActive(false);
        RefreshUI();
    }

    void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            equipmentPanel.SetActive(!equipmentPanel.activeSelf);
            if (equipmentPanel.activeSelf)
                RefreshUI();
        }
    }

    void RefreshUI()
    {
        // Refresh all slots
        foreach (var slotUI in slotUIMap.Values)
        {
            if (slotUI != null)
                slotUI.Refresh();
        }

        // Update total stats
        if (totalDamageText != null)
            totalDamageText.text = "Total Damage: +" + EquipmentManager.Instance.GetTotalDamageBonus();

        if (totalDefenseText != null)
            totalDefenseText.text = "Total Defense: +" + EquipmentManager.Instance.GetTotalDefenseBonus();

        // Update set bonuses
        UpdateSetBonuses();
    }

    void UpdateSetBonuses()
    {
        // Clear existing texts
        foreach (var text in setBonusTexts)
        {
            if (text != null)
                Destroy(text.gameObject);
        }
        setBonusTexts.Clear();

        if (setBonusesParent == null || setBonusTextPrefab == null) return;

        // Get active set bonuses
        var bonuses = EquipmentManager.Instance.GetActiveSetBonusDescriptions();

        foreach (var bonus in bonuses)
        {
            var text = Instantiate(setBonusTextPrefab, setBonusesParent);
            text.text = bonus;
            setBonusTexts.Add(text);
        }
    }
}
