using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private readonly Dictionary<string, int> items = new();
    private readonly Dictionary<string, ItemData> database = new();
    public event Action OnChanged;
    public static event Action OnReady;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            OnReady?.Invoke();
        }
        else Destroy(gameObject);
    }

    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return;
        database[item.itemID] = item;

        if (items.ContainsKey(item.itemID))
            items[item.itemID] += amount;
        else
            items[item.itemID] = amount;

        OnChanged?.Invoke();
    }

    public void RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || !items.ContainsKey(item.itemID)) return;
        items[item.itemID] -= amount;

        if (items[item.itemID] <= 0)
            items.Remove(item.itemID);

        OnChanged?.Invoke();
    }

    public Dictionary<string, int> GetItems()
    {
        return new Dictionary<string, int>(items);
    }

    public ItemData GetItem(string id)
    {
        if (database.TryGetValue(id, out var item))
            return item;

        Debug.LogWarning($"Item not found in database: {id}");
        return null;
    }

    public int GetQuantity(ItemData item)
    {
        if (item == null) return 0;
        return items.TryGetValue(item.itemID, out int qty) ? qty : 0;
    }

    public void Clear()
    {
        items.Clear();
        OnChanged?.Invoke();
    }
}
