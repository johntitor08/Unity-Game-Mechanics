using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private readonly Dictionary<string, int> items = new();
    public event Action OnInventoryChanged;
    public static event Action OnReady;

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

    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return;

        if (items.ContainsKey(item.itemID))
            items[item.itemID] += amount;
        else
            items[item.itemID] = amount;

        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || !items.ContainsKey(item.itemID)) return false;
        items[item.itemID] -= amount;

        if (items[item.itemID] <= 0)
            items.Remove(item.itemID);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetQuantity(ItemData item)
    {
        if (item == null) return 0;
        return items.TryGetValue(item.itemID, out int qty) ? qty : 0;
    }

    public IReadOnlyDictionary<string, int> GetItems() => items;

    public ItemData GetItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return ItemDatabase.Instance.GetByID(id);
    }

    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
