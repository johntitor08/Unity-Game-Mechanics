using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance;
    public event Action OnGuideChanged;
    const string PP_KEY = "guide_unlocked";
    readonly HashSet<string> _unlocked = new();

    [Header("All entries in the game")]
    public GuideEntry[] entries;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        var saved = PlayerPrefs.GetString(PP_KEY, "");

        foreach (var id in saved.Split(','))
            if (!string.IsNullOrEmpty(id))
                _unlocked.Add(id);

        if (entries != null)
            foreach (var e in entries)
                if (e != null && e.unlockedByDefault && !string.IsNullOrEmpty(e.id))
                    _unlocked.Add(e.id);
    }

    public bool IsUnlocked(string id) => !string.IsNullOrEmpty(id) && _unlocked.Contains(id);

    public void Unlock(string id)
    {
        if (string.IsNullOrEmpty(id) || !_unlocked.Add(id))
            return;

        PlayerPrefs.SetString(PP_KEY, string.Join(",", _unlocked));
        PlayerPrefs.Save();
        OnGuideChanged?.Invoke();
    }

    public List<GuideEntry> GetEntries(GuideCategory category)
    {
        if (entries == null)
            return new List<GuideEntry>();

        return entries.Where(e => e != null && e.category == category && IsUnlocked(e.id)).ToList();
    }
}
