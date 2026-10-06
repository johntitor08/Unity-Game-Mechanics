using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance;
    public event Action OnGuideChanged;
    const string LegacyPrefsKey = "guide_unlocked";
    static readonly HashSet<string> Unlocked = new();

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

        if (PlayerPrefs.HasKey(LegacyPrefsKey))
        {
            PlayerPrefs.DeleteKey(LegacyPrefsKey);
            PlayerPrefs.Save();
        }

        AddDefaults();
    }

    void AddDefaults()
    {
        if (entries == null)
            return;

        foreach (var e in entries)
            if (e != null && e.unlockedByDefault && !string.IsNullOrEmpty(e.id))
                Unlocked.Add(e.id);
    }

    public bool IsUnlocked(string id) => !string.IsNullOrEmpty(id) && Unlocked.Contains(id);

    public void Unlock(string id)
    {
        if (string.IsNullOrEmpty(id) || !Unlocked.Add(id))
            return;

        OnGuideChanged?.Invoke();
    }

    public static void ResetAll()
    {
        Unlocked.Clear();

        if (Instance != null)
        {
            Instance.AddDefaults();
            Instance.OnGuideChanged?.Invoke();
        }
    }

    public static void ExportTo(List<string> ids)
    {
        ids.Clear();
        ids.AddRange(Unlocked);
    }

    public static void ImportFrom(List<string> ids)
    {
        Unlocked.Clear();

        if (ids != null)
            foreach (var id in ids)
                if (!string.IsNullOrEmpty(id))
                    Unlocked.Add(id);

        if (Instance != null)
        {
            Instance.AddDefaults();

            if (Instance.entries != null)
                foreach (var e in Instance.entries)
                    if (e != null && !string.IsNullOrEmpty(e.unlockFlag) && StoryFlags.Has(e.unlockFlag))
                        Unlocked.Add(e.id);

            Instance.OnGuideChanged?.Invoke();
        }
    }

    public List<GuideEntry> GetEntries(GuideCategory category)
    {
        if (entries == null)
            return new List<GuideEntry>();

        return entries.Where(e => e != null && e.category == category && IsUnlocked(e.id)).ToList();
    }
}
