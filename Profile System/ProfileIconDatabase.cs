using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProfileIconDatabase", menuName = "Database/ProfileIconDatabase")]
public class ProfileIconDatabase : ScriptableObject
{
    public List<IconEntry> icons;
    private static ProfileIconDatabase _instance;
    public static ProfileIconDatabase Instance => _instance;
    private void OnEnable() => _instance = this;

    public Sprite GetIconSprite(string id)
    {
        foreach (var icon in icons)
            if (icon.id == id) return icon.sprite;

        return null;
    }

    public static Sprite GetSprite(string id) => _instance != null ? _instance.GetIconSprite(id) : null;
}

[System.Serializable]
public struct IconEntry
{
    public string id;
    public Sprite sprite;
    public int cost;
    public string displayName;
}
