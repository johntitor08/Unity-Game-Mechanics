using UnityEngine;

public class OriginManager : MonoBehaviour
{
    public static OriginManager Instance { get; private set; }
    public PlayerOriginData CurrentOrigin { get; private set; }
    public bool OriginSelected { get; private set; }
    public const string OriginBoundArchivist = "bound_archivist";
    public const string OriginForeignEcho = "foreign_echo";
    public const string OriginSinnedGuardian = "sinned_guardian";
    const string OriginStoryUnlockScenario = "ashenveil_day2";
    const float ChapterPollInterval = 0.5f;
    bool _originStoryPending;
    float _chapterPollTimer;

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
        if (ScenarioManager.Instance != null)
            ScenarioManager.Instance.OnScenarioComplete += HandleScenarioComplete;

        TryRestoreFromFlags();
    }

    void OnDestroy()
    {
        if (Instance == this && ScenarioManager.Instance != null)
            ScenarioManager.Instance.OnScenarioComplete -= HandleScenarioComplete;
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
        SaveSystem.SavingEnabled = true;
        StoryFlags.Add(QuestFlags.OriginPrefix + origin.originID);
        ApplyStats(origin);
        GrantStartingItems(origin);

        if (IsOriginStoryUnlocked())
        {
            StartOriginStory(origin);
        }
        else
        {
            _originStoryPending = true;
            Debug.Log($"[OriginManager] Origin selected: {origin.displayName}. Story deferred until the Day 2 scenario is completed.");
        }
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
        ResumeOrDeferOriginStory(origin);
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
            ResumeOrDeferOriginStory(o);
            Debug.Log($"[OriginManager] Origin restored from flags: {o.displayName}");
            return;
        }
    }

    void ResumeOrDeferOriginStory(PlayerOriginData origin)
    {
        if (HasOriginStoryStarted(origin))
        {
            if (!IsOriginOpeningComplete(origin.originID))
                RunOriginOpeningScene(origin.originID);

            ApplyOriginFlags(origin);
            AshenveilQuestFactory.OnOriginApplied();
            return;
        }

        _originStoryPending = true;
    }

    void StartOriginStory(PlayerOriginData origin)
    {
        _originStoryPending = false;
        RunOriginOpeningScene(origin.originID);
        ApplyOriginFlags(origin);
        AshenveilQuestFactory.OnOriginApplied();
        Debug.Log($"[OriginManager] Origin story started: {origin.displayName}");
    }

    void HandleScenarioComplete(ScenarioData scenario) => _chapterPollTimer = 0f;

    void Update()
    {
        if (!OriginSelected || CurrentOrigin == null)
            return;

        _chapterPollTimer -= Time.unscaledDeltaTime;

        if (_chapterPollTimer > 0f)
            return;

        _chapterPollTimer = ChapterPollInterval;

        if (IsStoryBusy())
            return;

        if (_originStoryPending)
        {
            if (IsOriginStoryUnlocked())
                StartOriginStory(CurrentOrigin);

            return;
        }

        UnlockNextChapter(CurrentOrigin.originID);
    }

    static void UnlockNextChapter(string originID)
    {
        var (chapter1Done, chapter2Done, _) = ChapterDoneFlags(originID);

        if (chapter1Done == null || (SceneEvent.Instance != null && !SceneEvent.Instance.IsInTownSquare))
            return;

        if (!StoryFlags.Has(QuestFlags.OriginChapter2Ready))
        {
            if (StoryFlags.Has(chapter1Done) && CurrentDay() >= 3 && (SceneEvent.Instance == null || !SceneEvent.Instance.IsSleeping))
                StoryFlags.Add(QuestFlags.OriginChapter2Ready);

            return;
        }

        if (!StoryFlags.Has(QuestFlags.OriginChapter3Ready) && StoryFlags.Has(chapter2Done) && StoryFlags.Has(QuestFlags.Q09VossWarehouseFound))
            StoryFlags.Add(QuestFlags.OriginChapter3Ready);
    }

    static (string chapter1Done, string chapter2Done, string chapter3Done) ChapterDoneFlags(string originID) => originID switch
    {
        OriginBoundArchivist => (QuestFlags.BoundArchivistQuest1Done, "archivist_quest2_done", "archivist_quest3_done"),
        OriginForeignEcho => (QuestFlags.ForeignEchoQuest1Done, "echo_quest2_done", "echo_quest3_done"),
        OriginSinnedGuardian => (QuestFlags.SinnedGuardianQuest1Done, "sinned_guardian_quest2_done", "sinned_guardian_quest3_done"),
        _ => (null, null, null)
    };

    public bool ShouldHoldDayScenario(string scenarioID)
    {
        if (scenarioID != "ashenveil_day3" || !OriginSelected || CurrentOrigin == null)
            return false;

        var (chapter1Done, _, chapter3Done) = ChapterDoneFlags(CurrentOrigin.originID);
        return chapter1Done != null && StoryFlags.Has(chapter1Done) && !StoryFlags.Has(chapter3Done);
    }

    static int CurrentDay() => TimeUI.Instance != null ? TimeUI.Instance.GetCurrentDay() : 1;

    static bool IsStoryBusy() => (ScenarioManager.Instance != null && ScenarioManager.Instance.IsScenarioActive()) || (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue()) || (CombatManager.Instance != null && CombatManager.Instance.inCombat);

    static bool IsOriginStoryUnlocked() => ScenarioManager.Instance != null && ScenarioManager.Instance.IsScenarioCompleted(OriginStoryUnlockScenario);

    static bool HasOriginStoryStarted(PlayerOriginData origin)
    {
        if (origin == null || origin.flagsOnSelect == null)
            return false;

        foreach (var flag in origin.flagsOnSelect)
            if (StoryFlags.Has(flag))
                return true;

        return false;
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
