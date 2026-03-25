using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI Instance { get; private set; }
    private Dictionary<EquipmentSlot, EquipmentSlotUI> slotUIMap;
    private readonly List<TextMeshProUGUI> setBonusTexts = new();

    [Header("Panels")]
    public GameObject equipmentPanel;
    public GameObject equipmentInfoPanel;

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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        InitializeSlots();
        SubscribeToEvents();
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            TogglePanel();
    }

    void OnDestroy() => UnsubscribeFromEvents();

    void InitializeSlots()
    {
        slotUIMap = new Dictionary<EquipmentSlot, EquipmentSlotUI>
        {
            {
                EquipmentSlot.Weapon, weaponSlot
            },
            {
                EquipmentSlot.Armor, armorSlot
            },
            {
                EquipmentSlot.Helmet, helmetSlot
            },
            {
                EquipmentSlot.Accessory, accessorySlot
            },
            {
                EquipmentSlot.Shield, shieldSlot
            },
            {
                EquipmentSlot.Boots, bootsSlot
            },
        };

        foreach (var (slot, ui) in slotUIMap)
        {
            if (ui != null)
                ui.Setup(slot);
            else
                Debug.LogWarning($"EquipmentSlotUI for {slot} is not assigned!");
        }

        foreach (var ui in slotUIMap.Values)
        {
            if (ui == null)
                continue;

            ui.OnItemClicked += OpenItemPanel;
            ui.OnDetailClicked += OpenDetailPanel;
        }
    }

    void SubscribeToEvents()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStateChanged += RefreshCombatStats;
    }

    void UnsubscribeFromEvents()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStateChanged -= RefreshCombatStats;
    }

    public void TogglePanel()
    {
        if (equipmentPanel == null)
            return;

        bool newState = !equipmentPanel.activeSelf;
        equipmentPanel.SetActive(newState);

        if (newState)
            RefreshUI();
    }

    public void ShowPanel()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(true);
        }

        RefreshUI();
    }

    public void HidePanel()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
    }

    public bool IsPanelVisible() => equipmentPanel != null && equipmentPanel.activeSelf;

    public void OpenItemPanel(EquipmentInstance instance)
    {
        if (equipmentInfoPanel == null || instance == null)
            return;

        equipmentInfoPanel.SetActive(true);
        EquipmentInfoPanel.Instance.ShowPanel(instance, EquipmentInfoPanel.PanelMode.Item);
    }

    public void OpenDetailPanel(EquipmentInstance instance)
    {
        if (equipmentInfoPanel == null || instance == null)
            return;

        equipmentInfoPanel.SetActive(true);
        EquipmentInfoPanel.Instance.ShowPanel(instance, EquipmentInfoPanel.PanelMode.Detail);
    }

    public void OpenItemPanel(EquipmentData data)
    {
        if (data == null)
            return;

        OpenItemPanel(new EquipmentInstance(data));
    }

    public void OpenDetailPanel(EquipmentData data)
    {
        if (data == null)
            return;

        OpenDetailPanel(new EquipmentInstance(data));
    }

    public void RefreshUI()
    {
        if (EquipmentManager.Instance == null)
            return;

        RefreshSlots();
        RefreshStats();
        RefreshSetBonuses();
    }

    public void RefreshCombatStats()
    {
        if (equipmentPanel != null && !equipmentPanel.activeSelf)
            return;

        RefreshStats();
        RefreshSetBonuses();
    }

    void RefreshSlots()
    {
        foreach (var ui in slotUIMap.Values)
            if (ui != null)
                ui.Refresh();
    }

    void RefreshStats()
    {
        if (EquipmentManager.Instance == null)
            return;

        if (totalDamageText != null)
            totalDamageText.text = $"Total Damage: +{EquipmentManager.Instance.GetTotalDamageBonus()}";

        if (totalDefenseText != null)
            totalDefenseText.text = $"Total Defense: +{EquipmentManager.Instance.GetTotalDefenseBonus()}";
    }

    void RefreshSetBonuses()
    {
        ClearSetBonusTexts();

        if (setBonusesParent == null || setBonusTextPrefab == null || EquipmentManager.Instance == null)
            return;

        var bonuses = EquipmentManager.Instance.GetActiveSetBonusDescriptions();

        if (bonuses.Count == 0)
            CreateSetBonusText("<color=#888888>No set bonuses active</color>");
        else
            foreach (var b in bonuses) CreateSetBonusText(b);
    }

    void ClearSetBonusTexts()
    {
        foreach (var t in setBonusTexts)
            if (t != null)
                Destroy(t.gameObject);

        setBonusTexts.Clear();
    }

    void CreateSetBonusText(string text)
    {
        var t = Instantiate(setBonusTextPrefab, setBonusesParent);
        t.text = text;
        setBonusTexts.Add(t);
    }

    public EquipmentSlotUI GetSlotUI(EquipmentSlot slot) => slotUIMap.TryGetValue(slot, out var ui) ? ui : null;
}
