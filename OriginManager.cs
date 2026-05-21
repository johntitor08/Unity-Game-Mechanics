using UnityEngine;

public class OriginManager : MonoBehaviour
{
    public static OriginManager Instance { get; private set; }
    public PlayerOriginData CurrentOrigin { get; private set; }
    public bool OriginSelected { get; private set; }

    [Header("All Origins")]
    public PlayerOriginData[] allOrigins;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        TryRestoreFromFlags();
    }

    public void SelectOrigin(PlayerOriginData origin)
    {
        if (origin == null)
        {
            Debug.LogWarning("[OriginManager] SelectOrigin: null origin.");
            return;
        }

        CurrentOrigin = origin;
        OriginSelected = true;
        StoryFlags.Add("origin:" + origin.originID);
        ApplyOriginFlags(origin);
        ApplyStats(origin);
        GrantStartingItems(origin);
        AshenveilQuestFactory.OnOriginApplied();
        Debug.Log($"[OriginManager] Origin selected: {origin.displayName}");
    }

    public void SelectOrigin(string originID)
    {
        foreach (var o in allOrigins)
            if (o.originID == originID)
            {
                SelectOrigin(o);
                return;
            }

        Debug.LogWarning($"[OriginManager] No origin with ID '{originID}'.");
    }

    public PlayerOriginData GetOrigin(string originID)
    {
        foreach (var o in allOrigins)
            if (o.originID == originID)
                return o;

        return null;
    }

    public string GetSaveID() => CurrentOrigin != null ? CurrentOrigin.GetSaveID() : "";

    public void LoadFromSaveID(string savedOriginID)
    {
        if (string.IsNullOrEmpty(savedOriginID))
            return;

        var origin = GetOrigin(savedOriginID);

        if (origin == null)
        {
            Debug.LogWarning($"[OriginManager] LoadFromSaveID: '{savedOriginID}' not found.");
            return;
        }

        CurrentOrigin = origin;
        OriginSelected = true;
        ApplyOriginFlags(origin);
        AshenveilQuestFactory.OnOriginApplied();
        Debug.Log($"[OriginManager] Origin loaded from save: {origin.displayName}");
    }

    void TryRestoreFromFlags()
    {
        foreach (var o in allOrigins)
        {
            if (!StoryFlags.Has("origin:" + o.originID))
                continue;

            CurrentOrigin = o;
            OriginSelected = true;
            ApplyOriginFlags(o);
            AshenveilQuestFactory.OnOriginApplied();
            Debug.Log($"[OriginManager] Origin restored from flags: {o.displayName}");
            return;
        }
    }

    void ApplyOriginFlags(PlayerOriginData origin)
    {
        if (origin.flagsOnSelect == null)
            return;

        foreach (var flag in origin.flagsOnSelect)
            StoryFlags.Add(flag);
    }

    void ApplyStats(PlayerOriginData origin)
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("[OriginManager] PlayerStats.Instance is null — stats not applied.");
            return;
        }

        var ps = PlayerStats.Instance;
        ps.Set(StatType.MaxHealth, origin.baseHP, save: false);
        ps.Set(StatType.Health, origin.baseHP, save: false);
        ps.Set(StatType.MaxEnergy, origin.baseMANA, save: false);
        ps.Set(StatType.Energy, origin.baseMANA, save: false);
        ps.Set(StatType.Strength, origin.baseATK, save: false);
        ps.Set(StatType.Defense, origin.baseDEF, save: false);
        ps.Set(StatType.Speed, origin.baseSPD, save: false);
        SaveSystem.SaveGame();
    }

    void GrantStartingItems(PlayerOriginData origin)
    {
        if (origin.startingItems == null || InventoryManager.Instance == null)
            return;

        for (int i = 0; i < origin.startingItems.Length; i++)
        {
            if (origin.startingItems[i] == null)
                continue;

            int qty = (origin.startingItemQty != null && i < origin.startingItemQty.Length) ? origin.startingItemQty[i] : 1;
            InventoryManager.Instance.AddItem(origin.startingItems[i], qty);
        }
    }
}
