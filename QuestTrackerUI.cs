using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestTrackerUI : MonoBehaviour
{
    public static QuestTrackerUI Instance;

    [Header("Tracker")]
    public GameObject trackerPanel;
    public Transform trackedQuestsParent;
    public QuestTrackerEntry trackerEntryPrefab;
    public int maxTrackedQuests = 3;

    [Header("Settings")]
    public bool autoTrackNewQuests = true;

    private List<string> trackedQuestIDs = new();
    private Dictionary<string, QuestTrackerEntry> trackerEntries = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;
        }

        UpdateTracker();
    }

    void OnQuestStarted(QuestData quest)
    {
        if (autoTrackNewQuests && quest.trackObjectives)
        {
            TrackQuest(quest.questID);
        }
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
        if (trackedQuestIDs.Contains(questID)) return;
        if (trackedQuestIDs.Count >= maxTrackedQuests) return;

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
        // Clear old entries
        foreach (var entry in trackerEntries.Values)
        {
            if (entry != null) Destroy(entry.gameObject);
        }
        trackerEntries.Clear();

        // Create new entries
        foreach (var questID in trackedQuestIDs)
        {
            var quest = QuestManager.Instance.GetActiveQuest(questID);
            if (quest == null) continue;

            var entry = Instantiate(trackerEntryPrefab, trackedQuestsParent);
            entry.Setup(quest);
            trackerEntries[questID] = entry;
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
