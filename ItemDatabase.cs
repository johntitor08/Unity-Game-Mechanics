using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<ItemData> items;

    private readonly Dictionary<string, ItemData> dict = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var i in items)
        {
            if (!dict.ContainsKey(i.itemID))
                dict.Add(i.itemID, i);
            else
                Debug.LogError("Ayný itemID iki kez var: " + i.itemID);
        }
    }

    public ItemData GetByID(string id)
    {
        if (!dict.ContainsKey(id))
        {
            Debug.LogError("ItemDatabase'de yok: " + id);
            return null;
        }

        return dict[id];
    }
}
