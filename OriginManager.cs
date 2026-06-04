using UnityEngine;

public class OriginManager : MonoBehaviour
{
    public static OriginManager Instance { get; private set; }
    public PlayerOriginData CurrentOrigin { get; private set; }
    public bool OriginSelected { get; private set; }
    public const string OriginBoundArchivist = "bound_archivist";
    public const string OriginForeignEcho = "foreign_echo";
    public const string OriginSinnedGuardian = "sinned_guardian";

    [Header("All Origins")]
    public PlayerOriginData[] allOrigins;

    [Header("Origin Quest Controllers")]
    [SerializeField] BoundArchivistQuestController boundArchivistQuest;
    [SerializeField] ForeignEchoQuestController foreignEchoQuest;
    [SerializeField] SinnedGuardianQuestController sinnedGuardianQuest;

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
        QuestFlags.MigrateLegacyOriginStartFlags();
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
        StoryFlags.Add(QuestFlags.OriginPrefix + origin.originID);
        RunOriginOpeningScene(origin.originID);
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

        if (!IsOriginOpeningComplete(savedOriginID))
            RunOriginOpeningScene(savedOriginID);

        ApplyOriginFlags(origin);
        AshenveilQuestFactory.OnOriginApplied();
        Debug.Log($"[OriginManager] Origin loaded from save: {origin.displayName}");
    }

    void TryRestoreFromFlags()
    {
        foreach (var o in allOrigins)
        {
            if (!StoryFlags.Has(QuestFlags.OriginPrefix + o.originID))
                continue;

            CurrentOrigin = o;
            OriginSelected = true;

            if (!IsOriginOpeningComplete(o.originID))
                RunOriginOpeningScene(o.originID);

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

    void RunOriginOpeningScene(string originID)
    {
        switch (originID)
        {
            case OriginBoundArchivist:
            {
                var controller = ResolveController(ref boundArchivistQuest);

                if (controller == null)
                    LogMissingController(originID);
                else
                    controller.OnOpeningSceneComplete();

                break;
            }
            case OriginForeignEcho:
            {
                var controller = ResolveController(ref foreignEchoQuest);

                if (controller == null)
                    LogMissingController(originID);
                else
                    controller.OnOpeningSceneComplete();

                break;
            }
            case OriginSinnedGuardian:
            {
                var controller = ResolveController(ref sinnedGuardianQuest);

                if (controller == null)
                    LogMissingController(originID);
                else
                    controller.OnOpeningSceneComplete();

                break;
            }
            default:
                Debug.LogWarning($"[OriginManager] No opening scene handler for origin '{originID}'.");
                break;
        }
    }

    static void LogMissingController(string originID) => Debug.LogWarning($"[OriginManager] Quest controller not found for origin '{originID}'. Add it to the scene or assign it on OriginManager.");

    static bool IsOriginOpeningComplete(string originID) => originID switch
    {
        OriginBoundArchivist => StoryFlags.Has(QuestFlags.BoundArchivistOpeningComplete),
        OriginForeignEcho => StoryFlags.Has(QuestFlags.ForeignEchoOpeningComplete),
        OriginSinnedGuardian => StoryFlags.Has(QuestFlags.SinnedGuardianOpeningComplete),
        _ => true
    };

    static T ResolveController<T>(ref T cached) where T : Object
    {
        if (cached != null)
            return cached;

        cached = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        return cached;
    }
}
