using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private readonly Dictionary<string, int> items = new();
    private readonly Dictionary<string, ItemData> database = new();

    public delegate void InventoryChanged();
    public event InventoryChanged OnChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(ItemData item, int amount)
    {
        if (!database.ContainsKey(item.itemID))
            database[item.itemID] = item;

        if (items.ContainsKey(item.itemID)) items[item.itemID] += amount;
        else items.Add(item.itemID, amount);

        OnChanged?.Invoke();
    }

    public void RemoveItem(ItemData item, int amount)
    {
        if (!items.ContainsKey(item.itemID)) return;

        items[item.itemID] -= amount;
        if (items[item.itemID] <= 0) items.Remove(item.itemID);

        OnChanged?.Invoke();
    }

    public Dictionary<string, int> GetItems() => items;

    public ItemData GetItem(string id) => database[id];

    public int GetQuantity(ItemData item) =>
        items.ContainsKey(item.itemID) ? items[item.itemID] : 0;

    public void Clear()
    {
        items.Clear();
        OnChanged?.Invoke();
    }
}
