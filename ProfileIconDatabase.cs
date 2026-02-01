using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProfileIconDatabase", menuName = "Database/ProfileIconDatabase")]
public class ProfileIconDatabase : ScriptableObject
{
    [System.Serializable]
    public struct IconEntry { public string id; public Sprite sprite; }
    public List<IconEntry> icons;
    private static ProfileIconDatabase _instance;
    public static ProfileIconDatabase Instance => _instance;
    private void OnEnable() => _instance = this;

    public static Sprite GetSprite(string id)
    {
        if (_instance == null) return null;

        foreach (var icon in _instance.icons)
        {
            if (icon.id == id) return icon.sprite;
        }

        return null;
    }
}
