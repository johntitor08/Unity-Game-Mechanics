using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public static event Action OnReady;
    public event Action OnInventoryChanged;
    private readonly Dictionary<string, int> stock = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnReady?.Invoke();
    }

    static string Key(string itemID, int upgradeLevel) => $"{itemID}:{upgradeLevel}";

    static string Key(ItemData item, int upgradeLevel) => Key(item.itemID, upgradeLevel);

    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return;

        AddInternal(Key(item, 0), amount);
    }

    public void AddUpgradedItem(EquipmentData data, int upgradeLevel, int amount = 1)
    {
        if (data == null || amount <= 0)
            return;

        AddInternal(Key(data, upgradeLevel), amount);
    }

    public void AddInstance(EquipmentInstance inst, int amount = 1)
    {
        if (inst == null || amount <= 0)
            return;

        AddInternal(Key(inst.baseData, inst.upgradeLevel), amount);
    }

    void AddInternal(string key, int amount)
    {
        stock[key] = stock.TryGetValue(key, out int cur) ? cur + amount : amount;
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(ItemData item, int amount = 1) => RemoveInternal(Key(item, 0), amount);

    public bool RemoveInstance(EquipmentInstance inst, int amount = 1) => RemoveInternal(Key(inst.baseData, inst.upgradeLevel), amount);

    public bool RemoveUpgradedItem(EquipmentData data, int upgradeLevel, int amount = 1) => RemoveInternal(Key(data, upgradeLevel), amount);

    bool RemoveInternal(string key, int amount)
    {
        if (!stock.TryGetValue(key, out int cur) || cur < amount)
            return false;

        if (cur == amount)
            stock.Remove(key);
        else
            stock[key] = cur - amount;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetQuantity(ItemData item) => item == null ? 0 : stock.TryGetValue(Key(item, 0), out int q) ? q : 0;

    public int GetQuantity(EquipmentData data) => GetQuantity(data as ItemData);

    public int GetQuantity(EquipmentInstance inst) => inst == null ? 0 : stock.TryGetValue(Key(inst.baseData, inst.upgradeLevel), out int q) ? q : 0;

    public int GetUpgradedQuantity(EquipmentData data, int upgradeLevel) => data == null ? 0 : stock.TryGetValue(Key(data, upgradeLevel), out int q) ? q : 0;

    public int GetTotalQuantity(string itemID)
    {
        int total = 0;

        foreach (var kv in stock)
            if (kv.Key.StartsWith(itemID + ":"))
                total += kv.Value;

        return total;
    }

    public bool HasDuplicates(EquipmentData data) => GetTotalQuantity(data.itemID) >= 2;

    public List<(EquipmentInstance inst, int qty)> GetEquipmentInstances()
    {
        var result = new List<(EquipmentInstance, int)>();
        var db = ItemDatabase.Instance;

        if (db == null)
            return result;

        foreach (var kv in stock)
        {
            if (kv.Value <= 0)
                continue;

            int sep = kv.Key.LastIndexOf(':');

            if (sep < 0)
                continue;

            string id = kv.Key[..sep];

            if (!int.TryParse(kv.Key[(sep + 1)..], out int lvl))
                continue;

            var data = db.GetByID(id) as EquipmentData;

            if (data == null)
                continue;

            result.Add((new EquipmentInstance(data, lvl), kv.Value));
        }

        return result;
    }

    public List<(ItemData item, int qty)> GetNonEquipmentItems()
    {
        var result = new List<(ItemData, int)>();
        var db = ItemDatabase.Instance;

        if (db == null)
            return result;

        foreach (var kv in stock)
        {
            if (kv.Value <= 0)
                continue;

            int sep = kv.Key.LastIndexOf(':');

            if (sep < 0)
                continue;

            string id = kv.Key[..sep];

            if (!int.TryParse(kv.Key[(sep + 1)..], out int lvl))
                continue;

            var item = db.GetByID(id);

            if (item == null || item is EquipmentData)
                continue;

            result.Add((item, kv.Value));
        }
        return result;
    }

    public IReadOnlyDictionary<string, int> GetItems()
    {
        var dict = new Dictionary<string, int>();

        foreach (var kv in stock)
        {
            int sep = kv.Key.LastIndexOf(':');

            if (sep < 0)
                continue;

            string id = kv.Key[..sep];

            if (!int.TryParse(kv.Key[(sep + 1)..], out int lvl) || lvl != 0)
                continue;

            dict[id] = kv.Value;
        }

        return dict;
    }

    public IReadOnlyDictionary<string, int> GetRawStock() => stock;

    public ItemData GetItem(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        var db = ItemDatabase.Instance;

        return db == null ? null : db.GetByID(id);
    }

    public void Clear()
    {
        stock.Clear();
        OnInventoryChanged?.Invoke();
    }
}
