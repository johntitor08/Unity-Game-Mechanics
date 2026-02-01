using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public static ItemDatabase Instance;
    private readonly Dictionary<string, ItemData> dict = new();

    [Header("All Items in the Game")]
    public List<ItemData> items = new();

    public void Initialize()
    {
        dict.Clear();

        foreach (var i in items)
        {
            if (!dict.ContainsKey(i.itemID))
                dict.Add(i.itemID, i);
            else
                Debug.LogError($"Duplicate itemID in database: {i.itemID}");
        }
    }

    public ItemData GetByID(string id)
    {
        if (!dict.TryGetValue(id, out var item))
        {
            Debug.LogError($"ItemDatabase missing: {id}");
            return null;
        }
        
        return item;
    }

    public void SetInstance()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Debug.LogWarning("ItemDatabase Instance already set!");

        Initialize();
    }
}
