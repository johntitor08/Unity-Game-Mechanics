using UnityEngine;
using System.Collections.Generic;

public class QuestTrackerUI : MonoBehaviour
{
    public static QuestTrackerUI Instance;
    private List<string> trackedQuestIDs = new();
    private readonly Dictionary<string, QuestTrackerEntry> trackerEntries = new();
    private bool isSubscribed = false;

    [Header("Tracker")]
    public GameObject trackerPanel;
    public Transform trackedQuestsParent;
    public QuestTrackerEntry trackerEntryPrefab;
    public int maxTrackedQuests = 3;

    [Header("Settings")]
    public bool autoTrackNewQuests = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        TrySubscribe();
        UpdateTracker();
    }

    void OnEnable()
    {
        if (QuestManager.Instance == null)
        {
            QuestManager.OnReady += TrySubscribe;
        }
    }

    void OnDisable()
    {
        if (isSubscribed && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated -= OnObjectiveUpdated;
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
            isSubscribed = true;
            UpdateTracker();
        }
    }

    void OnQuestStarted(QuestData quest)
    {
        if (autoTrackNewQuests && quest.trackObjectives)
            TrackQuest(quest.questID);
    }

    void OnQuestCompleted(QuestData quest)
    {
        UntrackQuest(quest.questID);
    }

    void OnObjectiveUpdated(QuestData quest, QuestObjective objective)
    {
        UpdateTracker();
    }

    public void TrackQuest(string questID)
    {
        if (trackedQuestIDs.Contains(questID))
            return;

        if (trackedQuestIDs.Count >= maxTrackedQuests)
            return;

        trackedQuestIDs.Add(questID);
        UpdateTracker();
    }

    public void UntrackQuest(string questID)
    {
        trackedQuestIDs.Remove(questID);
        UpdateTracker();
    }

    public void UpdateTracker()
    {
        var toRemove = new List<string>();

        foreach (var questID in trackerEntries.Keys)
        {
            if (!trackedQuestIDs.Contains(questID))
                toRemove.Add(questID);
        }

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
                var entry = Instantiate(trackerEntryPrefab, trackedQuestsParent);
                trackerEntries[questID] = entry;
            }

            trackerEntries[questID].Setup(quest);
        }
    }

    public List<string> GetTrackedQuests()
    {
        return new List<string>(trackedQuestIDs);
    }

    public void SetTrackedQuests(List<string> questIDs)
    {
        trackedQuestIDs = new List<string>(questIDs);
        UpdateTracker();
    }
}
