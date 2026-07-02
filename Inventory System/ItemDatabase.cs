using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
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

    #if UNITY_EDITOR

    [ContextMenu("Populate From All ItemData")]
    public void PopulateFromProject()
    {
        items.Clear();
        var seen = new HashSet<string>();

        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:ItemData"))
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item == null || string.IsNullOrEmpty(item.itemID))
                continue;

            if (seen.Add(item.itemID))
                items.Add(item);
            else
                Debug.LogError($"Duplicate itemID '{item.itemID}' at {path} — skipped.");
        }

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"[ItemDatabase] Populated {items.Count} items from the project.");
    }

    #endif
}
