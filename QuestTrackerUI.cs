using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class QuestTrackerUI : MonoBehaviour
{
    public static QuestTrackerUI Instance;
    private HashSet<string> trackedQuestIDs = new();
    private readonly Dictionary<string, QuestTrackerEntry> trackerEntries = new();
    private readonly List<string> toRemove = new();
    private bool isSubscribed = false;

    [Header("Tracker")]
    public GameObject trackerPanel;
    public Transform trackedQuestsParent;
    public QuestTrackerEntry trackerEntryPrefab;
    public int maxTrackedQuests = 3;
    public Button closeButton;

    [Header("Settings")]
    public bool autoTrackNewQuests = true;
    public bool persistAcrossScenes = false;

    [Header("Visibility")]
    [SerializeField] private bool isPanelHiddenByUser = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnEnable()
    {
        QuestManager.OnReady += TrySubscribe;
        TrySubscribe();
    }

    void OnDisable()
    {
        if (isSubscribed && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated -= OnObjectiveUpdated;
            QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveCompleted;
            QuestManager.Instance.OnTrackingToggleRequested -= OnTrackingToggleRequested;
            QuestManager.Instance.OnSaveDataRequested -= OnSaveDataRequested;
            QuestManager.Instance.OnTrackedQuestsLoaded -= OnTrackedQuestsLoaded;
            QuestManager.Instance.OnQuestAbandoned -= OnQuestAbandoned;
            isSubscribed = false;
        }

        QuestManager.OnReady -= TrySubscribe;
    }

    void TrySubscribe()
    {
        if (QuestManager.Instance != null && !isSubscribed)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;
            QuestManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;
            QuestManager.Instance.OnTrackingToggleRequested += OnTrackingToggleRequested;
            QuestManager.Instance.OnSaveDataRequested += OnSaveDataRequested;
            QuestManager.Instance.OnTrackedQuestsLoaded += OnTrackedQuestsLoaded;
            QuestManager.Instance.OnQuestAbandoned += OnQuestAbandoned;
            isSubscribed = true;
            UpdateTracker();
        }
    }

    void OnQuestStarted(QuestData quest)
    {
        if (autoTrackNewQuests && quest.trackObjectives)
        {
            if (!TrackQuest(quest.questID))
                Debug.Log($"[QuestTrackerUI] Max limit ({maxTrackedQuests}) dolu, '{quest.questName}' takibe alınamadı.");
        }
    }

    void OnQuestCompleted(QuestData quest) => UntrackQuest(quest.questID);

    void OnQuestAbandoned(QuestData quest) => UntrackQuest(quest.questID);

    void OnObjectiveUpdated(QuestData quest, QuestObjective objective) => RefreshEntries();

    void OnObjectiveCompleted(QuestData quest, QuestObjective objective) => RefreshEntries();

    void OnTrackingToggleRequested(QuestData quest) => ToggleQuest(quest);

    void OnSaveDataRequested(QuestSaveData data)
    {
        data.trackedQuestIDs = new List<string>(trackedQuestIDs);
    }

    void OnTrackedQuestsLoaded(List<string> questIDs)
    {
        if (questIDs == null)
            return;

        var qm = QuestManager.Instance;
        var validIDs = questIDs.Where(id => qm != null && qm.GetActiveQuest(id) != null).Take(maxTrackedQuests);
        trackedQuestIDs = new HashSet<string>(validIDs);
        UpdateTracker();
    }

    public bool IsTracked(string questID) => trackedQuestIDs.Contains(questID);

    public void ToggleQuest(QuestData quest)
    {
        if (IsTracked(quest.questID))
            UntrackQuest(quest.questID);
        else
            TrackQuest(quest.questID);
    }

    public bool TrackQuest(string questID)
    {
        if (trackedQuestIDs.Count >= maxTrackedQuests)
            return false;

        var qm = QuestManager.Instance;

        if (qm == null || qm.GetActiveQuest(questID) == null || !trackedQuestIDs.Add(questID))
            return false;

        isPanelHiddenByUser = false;

        UpdateTracker();
        return true;
    }

    public void UntrackQuest(string questID)
    {
        if (!trackedQuestIDs.Remove(questID))
            return;

        UpdateTracker();
    }

    void OnCloseClicked()
    {
        isPanelHiddenByUser = true;

        if (trackerPanel != null)
            trackerPanel.SetActive(false);
    }

    void UpdateTracker()
    {
        toRemove.Clear();

        foreach (var questID in trackerEntries.Keys)
            if (!trackedQuestIDs.Contains(questID))
                toRemove.Add(questID);

        foreach (var questID in toRemove)
        {
            if (trackerEntries[questID] != null)
                Destroy(trackerEntries[questID].gameObject);

            trackerEntries.Remove(questID);
        }

        foreach (var questID in trackedQuestIDs)
        {
            if (QuestManager.Instance == null)
                break;

            var quest = QuestManager.Instance.GetActiveQuest(questID);

            if (quest == null)
                continue;

            if (!trackerEntries.ContainsKey(questID))
            {
                if (trackerEntryPrefab == null)
                {
                    Debug.LogError("[QuestTrackerUI] trackerEntryPrefab atanmamış.", this);
                    continue;
                }

                var entry = Instantiate(trackerEntryPrefab, trackedQuestsParent);
                trackerEntries[questID] = entry;
            }

            trackerEntries[questID].Setup(quest);
        }

        if (trackerPanel != null)
            trackerPanel.SetActive(!isPanelHiddenByUser && trackedQuestIDs.Count > 0);
    }

    void RefreshEntries()
    {
        List<string> orphaned = null;
        var qm = QuestManager.Instance;

        foreach (var (questID, entry) in trackerEntries)
        {
            if (entry == null)
            {
                (orphaned ??= new()).Add(questID);
                continue;
            }

            var quest = qm != null ? qm.GetActiveQuest(questID) : null;

            if (quest != null)
                entry.Setup(quest);
            else
                (orphaned ??= new()).Add(questID);
        }

        if (orphaned != null)
        {
            foreach (var id in orphaned)
                trackedQuestIDs.Remove(id);

            UpdateTracker();
        }
    }

    public List<string> GetTrackedQuests() => new(trackedQuestIDs);
}
