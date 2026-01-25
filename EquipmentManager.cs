using UnityEngine;
using System.Collections.Generic;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Equipment Sets")]
    public EquipmentSetBonus[] equipmentSets;

    private readonly Dictionary<EquipmentSlot, EquipmentData> equippedItems = new();
    private readonly Dictionary<int, int> activeSetPieces = new();
    public event Action OnEquipmentChanged;
    public event Action<EquipmentData> OnEquipmentEquipped;
    public event Action<EquipmentData> OnEquipmentUnequipped;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            activeSetPieces.Clear();
        }
        else Destroy(gameObject);
    }

    public bool CanEquip(EquipmentData equipment)
    {
        if (equipment == null) return false;

        if (ProfileManager.Instance != null && ProfileManager.Instance.profile.level < equipment.requiredLevel)
        {
            Debug.Log($"Level {equipment.requiredLevel} required!");
            return false;
        }

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
        if (!CanEquip(equipment)) return false;

        if (equippedItems.ContainsKey(equipment.slot))
            Unequip(equipment.slot, false);

        equippedItems[equipment.slot] = equipment;
        ApplyEquipmentStats(equipment, true);
        UpdateSetBonuses();
        OnEquipmentEquipped?.Invoke(equipment);
        OnEquipmentChanged?.Invoke();
        SaveSystem.SaveGame();
        return true;
    }

    public bool Unequip(EquipmentSlot slot, bool save = true)
    {
        if (!equippedItems.ContainsKey(slot)) return false;
        EquipmentData equipment = equippedItems[slot];
        ApplyEquipmentStats(equipment, false);
        equippedItems.Remove(slot);
        UpdateSetBonuses();
        OnEquipmentUnequipped?.Invoke(equipment);
        OnEquipmentChanged?.Invoke();
        if (save) SaveSystem.SaveGame();
        return true;
    }

    void ApplyEquipmentStats(EquipmentData equipment, bool apply)
    {
        if (PlayerStats.Instance == null) return;
        int mult = apply ? 1 : -1;

        if (equipment.primaryStatBonus > 0)
            PlayerStats.Instance.Modify(equipment.primaryStat, equipment.primaryStatBonus * mult, false);

        if (equipment.secondaryStatBonus > 0)
            PlayerStats.Instance.Modify(equipment.secondaryStat, equipment.secondaryStatBonus * mult, false);
    }

    public int GetTotalDamageBonus()
    {
        int total = 0;
        foreach (var eq in equippedItems.Values) total += eq.damageBonus;

        foreach (var set in equipmentSets)
        {
            int pieces = GetEquippedSetPieces(set.data.setID);
        
            foreach (var bonus in set.data.bonuses)
            {
                if (bonus.stat == StatType.Strength && pieces >= bonus.requiredPieces)
                    total += bonus.value;
            }
        }

        return total;
    }

    public int GetTotalDefenseBonus()
    {
        int total = 0;
        foreach (var eq in equippedItems.Values) total += eq.defenseBonus;

        foreach (var set in equipmentSets)
        {
            int pieces = GetEquippedSetPieces(set.data.setID);
            
            foreach (var bonus in set.data.bonuses)
            {
                if (bonus.stat == StatType.Defense && pieces >= bonus.requiredPieces)
                    total += bonus.value;
            }
        }

        return total;
    }

    public Dictionary<EquipmentSlot, EquipmentData> GetAllEquipped() => new(equippedItems);

    public EquipmentData GetEquipped(EquipmentSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }

    public int GetEquippedSetPieces(int setID)
    {
        int count = 0;
    
        foreach (var eq in equippedItems.Values)
        {
            if (eq.setData != null && eq.setData.setID == setID)
                count++;
        }
        
        return count;
    }

    void UpdateSetBonuses()
    {
        if (equipmentSets == null || PlayerStats.Instance == null) return;

        foreach (var set in equipmentSets)
        {
            int previousPieces = activeSetPieces.TryGetValue(set.data.setID, out int prev) ? prev : 0;
            int currentPieces = GetEquippedSetPieces(set.data.setID);
            if (previousPieces == currentPieces) continue;

            if (previousPieces > 0)
                set.Apply(previousPieces, false);

            if (currentPieces > 0)
                set.Apply(currentPieces, true);

            activeSetPieces[set.data.setID] = currentPieces;
        }
    }

    public List<string> GetActiveSetBonusDescriptions()
    {
        List<string> descriptions = new();
        if (equipmentSets == null) return descriptions;

        foreach (var set in equipmentSets)
        {
            int pieces = GetEquippedSetPieces(set.data.setID);
            if (pieces == 0) continue;

            if (pieces > 0)
            {
                descriptions.Add($"{set.data.setName} ({pieces}/{set.data.totalPieces})");
                descriptions.Add(set.GetDescription(pieces));
            }
        }

        return descriptions;
    }
}
