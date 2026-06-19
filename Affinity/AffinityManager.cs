using System;
using System.Collections.Generic;
using UnityEngine;

public class AffinityManager : MonoBehaviour
{
    public static AffinityManager Instance;
    public event Action<string, int> OnAffinityChanged;
    public int maxAffinity = 100;
    const string PP = "affinity_";
    readonly Dictionary<string, int> _cache = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int Get(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return 0;

        if (_cache.TryGetValue(characterId, out var v))
            return v;

        v = PlayerPrefs.GetInt(PP + characterId, 0);
        _cache[characterId] = v;
        return v;
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
        PlayerPrefs.SetInt(PP + characterId, v);
        PlayerPrefs.Save();
        OnAffinityChanged?.Invoke(characterId, v);
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
            return "Devoted";

        if (v >= 60)
            return "Close";

        if (v >= 40)
            return "Friendly";

        if (v >= 20)
            return "Acquaintance";

        return "Stranger";
    }
}
