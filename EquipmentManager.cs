using UnityEngine;
using System.Collections.Generic;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Equipment Sets")]
    public EquipmentSetBonus[] equipmentSets;

    private Dictionary<EquipmentSlot, EquipmentData> equippedItems = new();

    public event Action OnEquipmentChanged;
    public event Action<EquipmentData> OnEquipmentEquipped;
    public event Action<EquipmentData> OnEquipmentUnequipped;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanEquip(EquipmentData equipment)
    {
        if (equipment == null) return false;

        // Check level requirement
        if (ProfileManager.Instance != null)
        {
            if (ProfileManager.Instance.profile.level < equipment.requiredLevel)
            {
                Debug.Log($"Level {equipment.requiredLevel} required!");
                return false;
            }
        }

        // Check stat requirement
        if (equipment.requiredStatValue > 0 && PlayerStats.Instance != null)
        {
            int currentStat = PlayerStats.Instance.Get(equipment.requiredStat);
            if (currentStat < equipment.requiredStatValue)
            {
                Debug.Log($"{equipment.requiredStat} {equipment.requiredStatValue} required!");
                return false;
            }
        }

        return true;
    }

    public bool Equip(EquipmentData equipment)
    {
        if (!CanEquip(equipment))
            return false;

        // Unequip current item in slot
        if (equippedItems.ContainsKey(equipment.slot))
        {
            Unequip(equipment.slot, false);
        }

        // Equip new item
        equippedItems[equipment.slot] = equipment;
        ApplyEquipmentStats(equipment, true);

        OnEquipmentEquipped?.Invoke(equipment);
        OnEquipmentChanged?.Invoke();

        Debug.Log($"Equipped: {equipment.itemName}");
        SaveSystem.SaveGame();
        return true;
    }

    public bool Unequip(EquipmentSlot slot, bool save = true)
    {
        if (!equippedItems.ContainsKey(slot))
            return false;

        EquipmentData equipment = equippedItems[slot];
        ApplyEquipmentStats(equipment, false);
        equippedItems.Remove(slot);

        OnEquipmentUnequipped?.Invoke(equipment);
        OnEquipmentChanged?.Invoke();

        Debug.Log($"Unequipped: {equipment.itemName}");

        if (save)
            SaveSystem.SaveGame();

        return true;
    }

    void ApplyEquipmentStats(EquipmentData equipment, bool apply)
    {
        if (PlayerStats.Instance == null) return;

        int multiplier = apply ? 1 : -1;

        // Apply primary stat
        if (equipment.primaryStatBonus > 0)
        {
            PlayerStats.Instance.Modify(equipment.primaryStat,
                equipment.primaryStatBonus * multiplier, false);
        }

        // Apply secondary stat
        if (equipment.secondaryStatBonus > 0)
        {
            PlayerStats.Instance.Modify(equipment.secondaryStat,
                equipment.secondaryStatBonus * multiplier, false);
        }

        // Save after all stat changes
        if (apply)
            SaveSystem.SaveGame();
    }

    public EquipmentData GetEquipped(EquipmentSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }

    public bool IsEquipped(EquipmentData equipment)
    {
        return equippedItems.ContainsKey(equipment.slot) &&
               equippedItems[equipment.slot] == equipment;
    }

    public int GetTotalDamageBonus()
    {
        int total = 0;
        foreach (var equipment in equippedItems.Values)
        {
            total += equipment.damageBonus;
        }
        return total;
    }

    public int GetTotalDefenseBonus()
    {
        int total = 0;
        foreach (var equipment in equippedItems.Values)
        {
            total += equipment.defenseBonus;
        }

        // Add set bonuses
        total += GetActiveSetBonuses();

        return total;
    }

    public Dictionary<EquipmentSlot, EquipmentData> GetAllEquipped()
    {
        return new Dictionary<EquipmentSlot, EquipmentData>(equippedItems);
    }

    public int GetEquippedSetPieces(int setID)
    {
        int count = 0;
        foreach (var equipment in equippedItems.Values)
        {
            if (equipment.setID == setID && setID > 0)
                count++;
        }
        return count;
    }

    public int GetActiveSetBonuses()
    {
        int totalBonus = 0;

        if (equipmentSets == null) return 0;

        foreach (var set in equipmentSets)
        {
            int equipped = GetEquippedSetPieces(set.setID);
            totalBonus += set.GetBonusForPieces(equipped);
        }

        return totalBonus;
    }

    public List<string> GetActiveSetBonusDescriptions()
    {
        List<string> descriptions = new();

        if (equipmentSets == null) return descriptions;

        foreach (var set in equipmentSets)
        {
            int equipped = GetEquippedSetPieces(set.setID);
            if (equipped >= 2)
            {
                descriptions.Add($"{set.setName} ({equipped}/{set.totalPieces})");
                descriptions.Add(set.GetActiveBonusDescription(equipped));
            }
        }

        return descriptions;
    }
}
