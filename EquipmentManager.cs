using UnityEngine;
using System.Collections.Generic;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;
    private readonly Dictionary<EquipmentSlot, EquipmentInstance> equippedItems = new();
    private readonly Dictionary<int, int> activeSetPieces = new();
    public event Action OnEquipmentChanged;
    public event Action<EquipmentInstance> OnEquipmentEquipped;
    public event Action<EquipmentInstance> OnEquipmentUnequipped;
    public static event Action OnReady;

    [Header("Equipment Sets")]
    public EquipmentSetBonus[] equipmentSets;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            activeSetPieces.Clear();
        }
        else
            Destroy(gameObject);
    }

    void Start() => OnReady?.Invoke();

    public EquipmentInstance GetEquipped(EquipmentSlot slot) => equippedItems.TryGetValue(slot, out var v) ? v : null;

    public bool IsEquipped(EquipmentData data)
    {
        if (data == null)
            return false;

        foreach (var kvp in equippedItems)
            if (kvp.Value != null && kvp.Value.baseData.itemID == data.itemID)
                return true;

        return false;
    }

    public Dictionary<EquipmentSlot, EquipmentInstance> GetAllEquipped() => new(equippedItems);

    public int GetTotalDamageBonus()
    {
        int total = 0;

        foreach (var inst in equippedItems.Values)
            total += inst.GetDamageBonus();

        return total;
    }

    public int GetTotalDefenseBonus()
    {
        int total = 0;

        foreach (var inst in equippedItems.Values)
            total += inst.GetDefenseBonus();

        return total;
    }

    public bool CanEquip(EquipmentInstance instance)
    {
        if (instance == null || instance.baseData == null)
            return false;

        if (IsEquipped(instance.baseData))
            return false;

        if (ProfileManager.Instance != null && ProfileManager.Instance.profile.level < instance.baseData.requiredLevel)
        {
            Debug.Log($"Level {instance.baseData.requiredLevel} required!");
            return false;
        }

        if (instance.baseData.requiredStatValue > 0 && PlayerStats.Instance != null)
        {
            int cur = PlayerStats.Instance.Get(instance.baseData.requiredStat);

            if (cur < instance.baseData.requiredStatValue)
            {
                Debug.Log($"{instance.baseData.requiredStat} {instance.baseData.requiredStatValue} required!");
                return false;
            }
        }

        return true;
    }

    public bool CanEquip(EquipmentData data) => data != null && CanEquip(new EquipmentInstance(data));

    public bool Equip(EquipmentInstance instance)
    {
        if (!CanEquip(instance))
            return false;

        if (equippedItems.TryGetValue(instance.baseData.slot, out var existing))
        {
            RemoveStats(existing);
            equippedItems.Remove(instance.baseData.slot);
            UpdateSetBonuses();
            OnEquipmentUnequipped?.Invoke(existing);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddInstance(existing, 1);
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveInstance(instance, 1);

        equippedItems[instance.baseData.slot] = instance;
        ApplyStats(instance);
        UpdateSetBonuses();
        OnEquipmentEquipped?.Invoke(instance);
        OnEquipmentChanged?.Invoke();
        SaveSystem.SaveGame();
        return true;
    }

    public bool Equip(EquipmentData data) => data != null && Equip(new EquipmentInstance(data));

    public bool Unequip(EquipmentSlot slot, bool returnToInventory = true, bool save = true)
    {
        if (!equippedItems.TryGetValue(slot, out var instance))
            return false;

        RemoveStats(instance);
        equippedItems.Remove(slot);
        UpdateSetBonuses();
        OnEquipmentUnequipped?.Invoke(instance);
        OnEquipmentChanged?.Invoke();

        if (returnToInventory && InventoryManager.Instance != null)
            InventoryManager.Instance.AddInstance(instance, 1);

        if (save)
            SaveSystem.SaveGame();

        return true;
    }

    public bool UpgradeEquipped(EquipmentSlot slot)
    {
        if (!equippedItems.TryGetValue(slot, out var instance))
            return false;

        if (!instance.CanUpgrade())
            return false;

        ApplyUpgradeDelta(instance);
        instance.upgradeLevel++;
        OnEquipmentChanged?.Invoke();
        SaveSystem.SaveGame();
        return true;
    }

    void ApplyStats(EquipmentInstance inst) => ModifyStats(inst, 1);

    void RemoveStats(EquipmentInstance inst) => ModifyStats(inst, -1);

    void ModifyStats(EquipmentInstance inst, int mult)
    {
        if (PlayerStats.Instance == null)
            return;

        if (inst.GetDamageBonus() != 0)
            PlayerStats.Instance.Modify(StatType.Damage, inst.GetDamageBonus() * mult, false);

        if (inst.GetDefenseBonus() != 0)
            PlayerStats.Instance.Modify(StatType.Defense, inst.GetDefenseBonus() * mult, false);

        if (inst.GetPrimaryBonus() != 0)
            PlayerStats.Instance.Modify(inst.baseData.primaryStat, inst.GetPrimaryBonus() * mult, false);

        if (inst.GetSecondaryBonus() != 0)
            PlayerStats.Instance.Modify(inst.baseData.secondaryStat, inst.GetSecondaryBonus() * mult, false);
    }

    void ApplyUpgradeDelta(EquipmentInstance inst)
    {
        if (PlayerStats.Instance == null)
            return;

        if (inst.baseData.damageBonus > 0)
            PlayerStats.Instance.Modify(StatType.Damage, 1, false);

        if (inst.baseData.defenseBonus > 0)
            PlayerStats.Instance.Modify(StatType.Defense, 1, false);

        if (inst.baseData.primaryStatBonus > 0)
            PlayerStats.Instance.Modify(inst.baseData.primaryStat, 1, false);

        if (inst.baseData.secondaryStatBonus > 0)
            PlayerStats.Instance.Modify(inst.baseData.secondaryStat, 1, false);
    }

    public int GetEquippedSetPieces(int setID)
    {
        int count = 0;

        foreach (var inst in equippedItems.Values)
            if (inst.baseData.setData != null && inst.baseData.setData.setID == setID)
                count++;

        return count;
    }

    void UpdateSetBonuses()
    {
        if (equipmentSets == null || PlayerStats.Instance == null)
            return;

        foreach (var set in equipmentSets)
        {
            int prev = activeSetPieces.TryGetValue(set.data.setID, out int p) ? p : 0;
            int curr = GetEquippedSetPieces(set.data.setID);

            if (prev == curr)
                continue;

            set.Apply(prev, false);
            set.Apply(curr, true);
            activeSetPieces[set.data.setID] = curr;
        }
    }

    public List<string> GetActiveSetBonusDescriptions()
    {
        var list = new List<string>();

        if (equipmentSets == null)
            return list;

        foreach (var set in equipmentSets)
        {
            int pieces = GetEquippedSetPieces(set.data.setID);

            if (pieces == 0)
                continue;

            list.Add($"{set.data.setName} ({pieces}/{set.data.totalPieces})");
            list.Add(set.GetDescription(pieces));
        }

        return list;
    }
}
