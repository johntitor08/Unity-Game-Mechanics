using UnityEngine;
using System.Collections.Generic;
using System;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;
    private readonly Dictionary<EquipmentSlot, EquipmentInstance> equippedItems = new();
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

    public bool CanEquip(EquipmentInstance instance)
    {
        if (instance == null || instance.baseData == null || IsEquipped(instance.baseData))
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

        var before = SnapshotStats();

        if (equippedItems.TryGetValue(instance.baseData.slot, out var existing))
        {
            equippedItems.Remove(instance.baseData.slot);
            OnEquipmentUnequipped?.Invoke(existing);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddInstance(existing, 1);
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveInstance(instance, 1);

        equippedItems[instance.baseData.slot] = instance;
        OnEquipmentEquipped?.Invoke(instance);
        AfterChange(before);
        SaveSystem.SaveGame();
        return true;
    }

    public bool Equip(EquipmentData data) => data != null && Equip(new EquipmentInstance(data));

    public bool Unequip(EquipmentSlot slot, bool returnToInventory = true, bool save = true)
    {
        if (!equippedItems.TryGetValue(slot, out var instance))
            return false;

        var before = SnapshotStats();
        equippedItems.Remove(slot);
        OnEquipmentUnequipped?.Invoke(instance);
        AfterChange(before);

        if (returnToInventory && InventoryManager.Instance != null)
            InventoryManager.Instance.AddInstance(instance, 1);

        if (save)
            SaveSystem.SaveGame();

        return true;
    }

    public void RestoreEquipped(IEnumerable<EquipmentInstance> items)
    {
        var before = SnapshotStats();
        equippedItems.Clear();

        foreach (var inst in items)
            if (inst != null && inst.baseData != null)
                equippedItems[inst.baseData.slot] = inst;

        AfterChange(before);
    }

    public bool UpgradeEquipped(EquipmentSlot slot)
    {
        if (!equippedItems.TryGetValue(slot, out var instance) || !instance.CanUpgrade())
            return false;

        var before = SnapshotStats();
        instance.upgradeLevel++;
        AfterChange(before);
        SaveSystem.SaveGame();
        return true;
    }

    public int GetStatBonus(StatType stat)
    {
        int total = 0;

        foreach (var inst in equippedItems.Values)
            total += inst.GetStatTotal(stat);

        if (equipmentSets != null)
            foreach (var set in equipmentSets)
                if (set != null && set.data != null)
                    total += set.GetBonus(GetEquippedSetPieces(set.data.setID), stat);

        return total;
    }

    private static Dictionary<StatType, int> SnapshotStats() => PlayerStats.Instance != null ? PlayerStats.Instance.SnapshotStats() : null;

    private void AfterChange(Dictionary<StatType, int> before)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.RefreshResourceCaps();
            PlayerStats.Instance.NotifyStatChanges(before);
        }

        OnEquipmentChanged?.Invoke();
    }

    public int GetEquippedSetPieces(int setID)
    {
        int count = 0;

        foreach (var inst in equippedItems.Values)
            if (inst.baseData.setData != null && inst.baseData.setData.setID == setID)
                count++;

        return count;
    }

    public List<string> GetActiveSetBonusDescriptions()
    {
        var list = new List<string>();

        if (equipmentSets == null)
            return list;

        foreach (var set in equipmentSets)
        {
            if (set == null || set.data == null)
                continue;

            int pieces = GetEquippedSetPieces(set.data.setID);

            if (pieces == 0)
                continue;

            list.Add($"{set.data.DisplaySetName} ({pieces}/{set.data.totalPieces})");
            list.Add(set.GetDescription(pieces));
        }

        return list;
    }
}
