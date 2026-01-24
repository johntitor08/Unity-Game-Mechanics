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

        // Listen to equipment changes
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;

        // Listen to combat updates
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStateChanged += RefreshCombatStats;

        equipmentPanel.SetActive(false);
        RefreshUI();
    }

    void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStateChanged -= RefreshCombatStats;
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

    // Refresh UI outside of combat
    public void RefreshUI()
    {
        if (EquipmentManager.Instance == null) return;
        RefreshSlots();
        RefreshStats();
        RefreshSetBonuses();
    }

    // Refresh UI stats during combat
    public void RefreshCombatStats()
    {
        if (EquipmentManager.Instance == null) return;
        RefreshStats();
        RefreshSetBonuses();
    }

    void RefreshSlots()
    {
        foreach (var slotUI in slotUIMap.Values)
        {
            if (slotUI != null)
                slotUI.Refresh();
        }
    }

    void RefreshStats()
    {
        if (totalDamageText != null)
            totalDamageText.text = "Total Damage: +" + EquipmentManager.Instance.GetTotalDamageBonus();

        if (totalDefenseText != null)
            totalDefenseText.text = "Total Defense: +" + EquipmentManager.Instance.GetTotalDefenseBonus();
    }

    void RefreshSetBonuses()
    {
        // Clear existing texts
        foreach (var text in setBonusTexts)
        {
            if (text != null)
                Destroy(text.gameObject);
        }

        setBonusTexts.Clear();
        if (setBonusesParent == null || setBonusTextPrefab == null) return;
        var bonuses = EquipmentManager.Instance.GetActiveSetBonusDescriptions();

        foreach (var bonus in bonuses)
        {
            var text = Instantiate(setBonusTextPrefab, setBonusesParent);
            text.text = bonus;
            setBonusTexts.Add(text);
        }
    }
}
