using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public event Action<QuestData> OnQuestStarted;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestFailed;
    public event Action<QuestData, QuestObjective> OnObjectiveUpdated;
    public event Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public static event Action OnReady;

    private readonly List<QuestData> activeQuests = new();
    private readonly List<QuestData> completedQuests = new();
    private readonly HashSet<string> activeQuestIDs = new();
    private readonly HashSet<string> completedQuestIDs = new();
    private readonly Dictionary<string, QuestRuntimeState> runtimeStates = new();
    private readonly Dictionary<string, float> questTimers = new();

    [Header("Available Quests")]
    public QuestData[] allQuests;

    [Header("Quest Tracking")]
    public int maxActiveQuests = 5;
    public int maxDailyQuests = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        OnReady?.Invoke();

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatEnded += CheckKillObjectives;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += CheckCollectObjectives;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencySpent += CheckSpendObjectives;
    }

    void Update()
    {
        UpdateQuestTimers();
    }

    QuestRuntimeState GetOrCreateRuntimeState(string questID)
    {
        if (!runtimeStates.TryGetValue(questID, out var state))
        {
            state = new QuestRuntimeState(questID);
            runtimeStates[questID] = state;
        }

        return state;
    }

    public ObjectiveRuntimeState GetObjectiveState(string questID, string objectiveID)
    {
        return GetOrCreateRuntimeState(questID).GetObjective(objectiveID);
    }

    void UpdateQuestTimers()
    {
        List<string> expiredQuests = new();

        foreach (var kvp in questTimers)
        {
            questTimers[kvp.Key] -= Time.deltaTime;

            if (questTimers[kvp.Key] <= 0)
                expiredQuests.Add(kvp.Key);
        }

        foreach (var questID in expiredQuests)
        {
            var quest = GetActiveQuest(questID);

            if (quest != null)
                FailQuest(quest);
        }
    }

    public bool CanStartQuest(QuestData quest)
    {
        if (IsQuestActive(quest.questID) || (IsQuestCompleted(quest.questID) && quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily) || activeQuests.Count >= maxActiveQuests || (ProfileManager.Instance != null && ProfileManager.Instance.profile.level < quest.requiredLevel))
            return false;

        if (quest.requiredFlags != null)
            foreach (var flag in quest.requiredFlags)
                if (!StoryFlags.Has(flag))
                    return false;

        if (quest.prerequisiteQuests != null)
            foreach (var prereq in quest.prerequisiteQuests)
                if (!IsQuestCompleted(prereq.questID))
                    return false;

        return true;
    }

    public bool StartQuest(QuestData quest)
    {
        if (!CanStartQuest(quest))
            return false;

        var runtimeState = new QuestRuntimeState(quest.questID);

        foreach (var objective in quest.objectives)
        {
            var objState = runtimeState.GetObjective(objective.objectiveID);
            objState.currentProgress = 0;
            objState.isCompleted = false;
            objective.onObjectiveStart?.Invoke();
        }

        runtimeStates[quest.questID] = runtimeState;
        activeQuests.Add(quest);
        activeQuestIDs.Add(quest.questID);

        if (quest.flagsToSetOnStart != null)
            foreach (var flag in quest.flagsToSetOnStart)
                StoryFlags.Add(flag);

        if (quest.hasTimeLimit)
            questTimers[quest.questID] = quest.timeLimitSeconds;

        quest.onQuestStart?.Invoke();
        OnQuestStarted?.Invoke(quest);
        Debug.Log($"Quest started: {quest.questName}");
        SaveSystem.SaveGame();
        return true;
    }

    public void UpdateObjectiveProgress(string questID, string objectiveID, int amount = 1)
    {
        var quest = GetActiveQuest(questID);

        if (quest == null)
            return;

        var objective = GetObjective(quest, objectiveID);

        if (objective == null)
            return;

        var objState = GetObjectiveState(questID, objectiveID);

        if (objState.isCompleted)
            return;

        objState.currentProgress += amount;
        int required = objective.GetRequiredCount();

        if (objState.currentProgress >= required)
        {
            objState.currentProgress = required;
            CompleteObjective(quest, objective, objState);
        }
        else
        {
            OnObjectiveUpdated?.Invoke(quest, objective);
        }

        SaveSystem.SaveGame();
    }

    void CompleteObjective(QuestData quest, QuestObjective objective, ObjectiveRuntimeState objState)
    {
        objState.isCompleted = true;
        objective.onObjectiveComplete?.Invoke();
        OnObjectiveCompleted?.Invoke(quest, objective);
        Debug.Log($"Objective completed: {objective.description}");
        bool allComplete = quest.objectives.All(obj => obj.isOptional || GetObjectiveState(quest.questID, obj.objectiveID).isCompleted);

        if (allComplete)
            CompleteQuest(quest);
    }

    public void CompleteQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID))
            return;

        activeQuests.Remove(quest);
        activeQuestIDs.Remove(quest.questID);

        if (quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily)
        {
            completedQuests.Add(quest);
            completedQuestIDs.Add(quest.questID);
        }

        if (questTimers.ContainsKey(quest.questID))
            questTimers.Remove(quest.questID);

        GiveQuestRewards(quest);

        if (quest.flagsToSetOnComplete != null)
            foreach (var flag in quest.flagsToSetOnComplete)
                StoryFlags.Add(flag);

        quest.onQuestComplete?.Invoke();
        OnQuestCompleted?.Invoke(quest);

        if (quest.completeDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(quest.completeDialogue);

        Debug.Log($"Quest completed: {quest.questName}");
        SaveSystem.SaveGame();
    }

    void GiveQuestRewards(QuestData quest)
    {
        if (quest.experienceReward > 0 && ProfileManager.Instance != null)
            ProfileManager.Instance.AddExperience(quest.experienceReward);

        if (quest.currencyRewards != null)
            foreach (var reward in quest.currencyRewards)
                reward.Grant();

        if (quest.itemRewards != null && InventoryManager.Instance != null)
        {
            for (int i = 0; i < quest.itemRewards.Length; i++)
            {
                int qty = (quest.itemRewardQuantities != null && i < quest.itemRewardQuantities.Length) ? quest.itemRewardQuantities[i] : 1;
                InventoryManager.Instance.AddItem(quest.itemRewards[i], qty);
            }
        }

        if (QuestRewardUI.Instance != null)
            QuestRewardUI.Instance.ShowRewards(quest);
    }

    public void FailQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID))
            return;

        if (!quest.canFail)
            return;

        activeQuests.Remove(quest);
        activeQuestIDs.Remove(quest.questID);

        if (questTimers.ContainsKey(quest.questID))
            questTimers.Remove(quest.questID);

        quest.onQuestFail?.Invoke();
        OnQuestFailed?.Invoke(quest);
        Debug.Log($"Quest failed: {quest.questName}");
        SaveSystem.SaveGame();
    }

    public void AbandonQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID))
            return;

        activeQuests.Remove(quest);
        activeQuestIDs.Remove(quest.questID);

        if (questTimers.ContainsKey(quest.questID))
            questTimers.Remove(quest.questID);

        runtimeStates.Remove(quest.questID);

        Debug.Log($"Quest abandoned: {quest.questName}");
        SaveSystem.SaveGame();
    }

    void CheckKillObjectives()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.currentEnemy == null)
            return;

        var killedEnemy = CombatManager.Instance.currentEnemy;

        foreach (var quest in activeQuests.ToList())
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == QuestObjectiveType.KillEnemies && objective.targetEnemy == killedEnemy)
                {
                    var state = GetObjectiveState(quest.questID, objective.objectiveID);

                    if (!state.isCompleted)
                        UpdateObjectiveProgress(quest.questID, objective.objectiveID, 1);
                }
            }
        }
    }

    void CheckCollectObjectives()
    {
        foreach (var quest in activeQuests.ToList())
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type != QuestObjectiveType.CollectItems)
                    continue;

                var state = GetObjectiveState(quest.questID, objective.objectiveID);

                if (state.isCompleted)
                    continue;

                int currentCount = InventoryManager.Instance.GetQuantity(objective.targetItem);
                int delta = Mathf.Max(0, currentCount - state.currentProgress);

                if (delta > 0)
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, delta);

                var updatedState = GetObjectiveState(quest.questID, objective.objectiveID);

                if (updatedState.isCompleted && objective.consumeItems)
                    InventoryManager.Instance.RemoveItem(objective.targetItem, objective.itemCount);
            }
        }
    }

    void CheckSpendObjectives(CurrencyType type, int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == QuestObjectiveType.SpendCurrency && objective.currencyType == type)
                {
                    var state = GetObjectiveState(quest.questID, objective.objectiveID);

                    if (!state.isCompleted)
                        UpdateObjectiveProgress(quest.questID, objective.objectiveID, amount);
                }
            }
        }
    }

    public QuestSaveData GetSaveData()
    {
        var data = new QuestSaveData
        {
            activeQuestIDs = activeQuests.Select(q => q.questID).ToList(),
            completedQuestIDs = completedQuests.Select(q => q.questID).ToList(),
            runtimeStates = runtimeStates.Values.ToList()
        };

        foreach (var kvp in questTimers)
        {
            data.questTimerKeys.Add(kvp.Key);
            data.questTimerValues.Add(kvp.Value);
        }

        if (QuestTrackerUI.Instance != null)
            data.trackedQuestIDs = QuestTrackerUI.Instance.GetTrackedQuests();

        return data;
    }

    public void LoadSaveData(QuestSaveData data)
    {
        if (data == null)
            return;

        activeQuests.Clear();
        activeQuestIDs.Clear();
        completedQuests.Clear();
        completedQuestIDs.Clear();
        runtimeStates.Clear();
        questTimers.Clear();
        var questLookup = allQuests.ToDictionary(q => q.questID);

        foreach (var id in data.activeQuestIDs)
        {
            if (questLookup.TryGetValue(id, out var q))
            {
                activeQuests.Add(q);
                activeQuestIDs.Add(id);
            }
        }

        foreach (var id in data.completedQuestIDs)
        {
            if (questLookup.TryGetValue(id, out var q))
            {
                completedQuests.Add(q);
                completedQuestIDs.Add(id);
            }
        }

        if (data.runtimeStates != null)
            foreach (var state in data.runtimeStates)
            {
                state.RebuildLookup();
                runtimeStates[state.questID] = state;
            }

        for (int i = 0; i < data.questTimerKeys.Count; i++)
            questTimers[data.questTimerKeys[i]] = data.questTimerValues[i];

        if (QuestTrackerUI.Instance != null && data.trackedQuestIDs != null)
            QuestTrackerUI.Instance.SetTrackedQuests(data.trackedQuestIDs);
    }

    public bool IsQuestActive(string questID) => activeQuestIDs.Contains(questID);

    public bool IsQuestCompleted(string questID) => completedQuestIDs.Contains(questID);

    public QuestData GetActiveQuest(string questID) => activeQuests.FirstOrDefault(q => q.questID == questID);

    public QuestObjective GetObjective(QuestData quest, string objectiveID) => quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);

    public List<QuestData> GetActiveQuests() => new(activeQuests);

    public List<QuestData> GetCompletedQuests() => new(completedQuests);

    public List<QuestData> GetAvailableQuests() => allQuests.Where(CanStartQuest).ToList();

    public float GetQuestTimeRemaining(string questID) => questTimers.TryGetValue(questID, out float t) ? t : 0f;

    public void ToggleTracking(QuestData quest)
    {
        if (QuestTrackerUI.Instance == null || !IsQuestActive(quest.questID))
            return;

        QuestTrackerUI.Instance.ToggleQuest(quest);
    }
}
