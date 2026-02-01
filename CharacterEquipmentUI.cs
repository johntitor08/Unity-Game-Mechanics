using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterEquipmentUI : MonoBehaviour
{
    [Header("Equipment Slots")]
    public EquipmentSlotUI helmetSlot;
    public EquipmentSlotUI weaponSlot;
    public EquipmentSlotUI armorSlot;
    public EquipmentSlotUI bootsSlot;
    public EquipmentSlotUI accessorySlot;

    [Header("Stats Display")]
    public TextMeshProUGUI totalDamageText;
    public TextMeshProUGUI totalDefenseText;
    public TextMeshProUGUI setBonus;

    [Header("Character Preview")]
    public Image characterPreview;

    void OnEnable()
    {
        RefreshAll();

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += RefreshAll;
    }

    void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshAll;
    }

    public void RefreshAll()
    {
        RefreshStats();
        RefreshSetBonuses();
    }

    private void RefreshStats()
    {
        if (EquipmentManager.Instance == null) return;
        int totalDamage = EquipmentManager.Instance.GetTotalDamageBonus();
        int totalDefense = EquipmentManager.Instance.GetTotalDefenseBonus();

        if (totalDamageText != null)
            totalDamageText.text = $"Total Damage: +{totalDamage}";

        if (totalDefenseText != null)
            totalDefenseText.text = $"Total Defense: +{totalDefense}";
    }

    private void RefreshSetBonuses()
    {
        if (setBonus == null || EquipmentManager.Instance == null) return;
        List<string> bonuses = EquipmentManager.Instance.GetActiveSetBonusDescriptions();

        if (bonuses.Count == 0)
        {
            setBonus.text = "<color=#888888>No set bonuses active</color>";
        }
        else
        {
            setBonus.text = "<b>Set Bonuses:</b>\n" + string.Join("\n", bonuses);
        }
    }
}
