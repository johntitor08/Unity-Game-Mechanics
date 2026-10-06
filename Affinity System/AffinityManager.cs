using System;
using System.Collections.Generic;
using UnityEngine;

public class AffinityManager : MonoBehaviour
{
    public static AffinityManager Instance;
    public event Action<string, int> OnAffinityChanged;
    public int maxAffinity = 100;
    const string LegacyPrefsPrefix = "affinity_";
    static readonly Dictionary<string, int> _cache = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DeleteLegacyPrefs();
    }

    static void DeleteLegacyPrefs()
    {
        bool any = false;

        foreach (var id in Characters)
            if (PlayerPrefs.HasKey(LegacyPrefsPrefix + id))
            {
                PlayerPrefs.DeleteKey(LegacyPrefsPrefix + id);
                any = true;
            }

        if (any)
            PlayerPrefs.Save();
    }

    public int Get(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return 0;

        return _cache.TryGetValue(characterId, out var v) ? v : 0;
    }

    public void Add(string characterId, int delta)
    {
        if (string.IsNullOrEmpty(characterId) || delta == 0)
            return;

        Set(characterId, Get(characterId) + delta);
    }

    public void Set(string characterId, int value)
    {
        if (string.IsNullOrEmpty(characterId))
            return;

        int v = Mathf.Clamp(value, 0, maxAffinity);
        _cache[characterId] = v;
        OnAffinityChanged?.Invoke(characterId, v);
    }

    public static readonly string[] Characters = { "Serena", "Maren", "Voss", "Awamori" };

    public static void ResetAll()
    {
        _cache.Clear();
    }

    public void ExportTo(List<string> keys, List<int> values)
    {
        keys.Clear();
        values.Clear();

        foreach (var id in Characters)
        {
            keys.Add(id);
            values.Add(Get(id));
        }
    }

    public void ImportFrom(List<string> keys, List<int> values)
    {
        _cache.Clear();

        if (keys == null)
            return;

        foreach (var id in Characters)
        {
            int i = keys.IndexOf(id);
            Set(id, i >= 0 && i < values.Count ? values[i] : 0);
        }
    }

    public string HeartBar(string characterId)
    {
        int v = Get(characterId);
        int filled = Mathf.Clamp(Mathf.CeilToInt((v / (float)Mathf.Max(1, maxAffinity)) * 10f), 0, 10);
        return "[" + new string('=', filled) + new string('-', 10 - filled) + "]";
    }

    public string Tier(string characterId)
    {
        int v = Get(characterId);

        if (v >= 80)
            return Loc.T("Devoted", "Sadık");

        if (v >= 60)
            return Loc.T("Close", "Yakın");

        if (v >= 40)
            return Loc.T("Friendly", "Dost");

        if (v >= 20)
            return Loc.T("Acquaintance", "Tanıdık");

        return Loc.T("Stranger", "Yabancı");
    }
}
