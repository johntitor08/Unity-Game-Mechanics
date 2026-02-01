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
    public GameObject equipmentDetailPanel;
    public GameObject equipmentItemPanel;

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
        InitializeSingleton();
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
        HandleInput();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSlots()
    {
        slotUIMap = new Dictionary<EquipmentSlot, EquipmentSlotUI>
        {
            { EquipmentSlot.Weapon, weaponSlot },
            { EquipmentSlot.Armor, armorSlot },
            { EquipmentSlot.Helmet, helmetSlot },
            { EquipmentSlot.Accessory, accessorySlot },
            { EquipmentSlot.Shield, shieldSlot },
            { EquipmentSlot.Boots, bootsSlot }
        };

        foreach (var slot in slotUIMap)
        {
            if (slot.Value != null)
            {
                slot.Value.Setup(slot.Key);
            }
            else
            {
                Debug.LogWarning($"EquipmentSlotUI for {slot.Key} is not assigned!");
            }
        }

        foreach (var slot in slotUIMap.Values)
        {
            slot.OnItemClicked += OpenItemPanel;
            slot.OnDetailClicked += OpenDetailPanel;
        }
    }

    private void SubscribeToEvents()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged += RefreshCombatStats;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged -= RefreshCombatStats;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (equipmentPanel == null) return;
        bool newState = !equipmentPanel.activeSelf;
        equipmentPanel.SetActive(newState);

        if (newState)
        {
            RefreshUI();
        }
    }

    public void ShowPanel()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(true);
            RefreshUI();
        }
    }

    public void HidePanel()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
    }

    public void OpenItemPanel(EquipmentData equipment)
    {
        if (equipmentItemPanel == null || equipment == null)
            return;

        equipmentItemPanel.SetActive(true);
        EquipmentItemPanel.Instance.ShowItemPanel(equipment);
    }

    public void OpenDetailPanel(EquipmentData equipment)
    {
        if (equipmentDetailPanel == null || equipment == null)
            return;

        equipmentDetailPanel.SetActive(true);
        EquipmentDetailPanel.Instance.ShowDetailPanel(equipment);
    }

    public void RefreshUI()
    {
        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning("EquipmentManager.Instance is null!");
            return;
        }

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

    private void RefreshSlots()
    {
        foreach (var slotUI in slotUIMap.Values)
        {
            if (slotUI != null)
            {
                slotUI.Refresh();
            }
        }
    }

    private void RefreshStats()
    {
        if (EquipmentManager.Instance == null) return;
        int totalDamage = EquipmentManager.Instance.GetTotalDamageBonus();
        int totalDefense = EquipmentManager.Instance.GetTotalDefenseBonus();

        if (totalDamageText != null)
        {
            totalDamageText.text = $"Total Damage: +{totalDamage}";
        }

        if (totalDefenseText != null)
        {
            totalDefenseText.text = $"Total Defense: +{totalDefense}";
        }
    }

    private void RefreshSetBonuses()
    {
        ClearSetBonusTexts();

        if (setBonusesParent == null || setBonusTextPrefab == null || EquipmentManager.Instance == null)
        {
            return;
        }

        List<string> bonuses = EquipmentManager.Instance.GetActiveSetBonusDescriptions();

        if (bonuses.Count == 0)
        {
            CreateSetBonusText("<color=#888888>No set bonuses active</color>");
        }
        else
        {
            foreach (var bonus in bonuses)
            {
                CreateSetBonusText(bonus);
            }
        }
    }

    private void ClearSetBonusTexts()
    {
        foreach (var text in setBonusTexts)
        {
            if (text != null)
            {
                Destroy(text.gameObject);
            }
        }

        setBonusTexts.Clear();
    }

    private void CreateSetBonusText(string bonusText)
    {
        TextMeshProUGUI text = Instantiate(setBonusTextPrefab, setBonusesParent);
        text.text = bonusText;
        setBonusTexts.Add(text);
    }

    public EquipmentSlotUI GetSlotUI(EquipmentSlot slot)
    {
        return slotUIMap.TryGetValue(slot, out var slotUI) ? slotUI : null;
    }

    public bool IsPanelVisible()
    {
        return equipmentPanel != null && equipmentPanel.activeSelf;
    }
}
