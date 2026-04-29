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
    public event Action<QuestData> OnQuestAbandoned;
    public event Action<QuestData, QuestObjective> OnObjectiveUpdated;
    public event Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public event Action<QuestData> OnTrackingToggleRequested;
    public event Action<QuestSaveData> OnSaveDataRequested;
    public event Action<List<string>> OnTrackedQuestsLoaded;
    public static event Action OnReady;
    private readonly Dictionary<string, QuestData> activeQuestsByID = new();
    private readonly List<QuestData> completedQuests = new();
    private readonly HashSet<string> completedQuestIDs = new();
    private readonly Dictionary<string, QuestRuntimeState> runtimeStates = new();
    private readonly Dictionary<string, float> questTimers = new();
    private readonly List<string> expiredQuests = new();
    private readonly List<string> timerKeys = new();
    private readonly List<QuestData> activeQuestBuffer = new();

    [Header("Available Quests")]
    public QuestData[] allQuests;

    [Header("Quest Tracking")]
    public int maxActiveQuests = 5;

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

        OnReady?.Invoke();
    }

    void Start()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatVictory += OnCombatVictory;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += CheckCollectObjectives;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencySpent += CheckSpendObjectives;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatVictory -= OnCombatVictory;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= CheckCollectObjectives;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencySpent -= CheckSpendObjectives;
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
        TickTimers();
        ProcessExpiredQuests();
    }

    void TickTimers()
    {
        expiredQuests.Clear();
        timerKeys.Clear();
        timerKeys.AddRange(questTimers.Keys);

        foreach (var questID in timerKeys)
        {
            questTimers[questID] -= Time.deltaTime;

            if (questTimers[questID] <= 0)
                expiredQuests.Add(questID);
        }
    }

    void ProcessExpiredQuests()
    {
        foreach (var questID in expiredQuests)
        {
            var quest = GetActiveQuest(questID);

            if (quest != null)
                FailQuest(quest);
        }
    }

    void RefreshActiveQuestBuffer()
    {
        activeQuestBuffer.Clear();
        activeQuestBuffer.AddRange(activeQuestsByID.Values);
    }

    public bool CanStartQuest(QuestData quest)
    {
        if (IsQuestActive(quest.questID) || activeQuestsByID.Count >= maxActiveQuests || (IsQuestCompleted(quest.questID) && quest.questType is not QuestType.Repeatable and not QuestType.Daily) || (ProfileManager.Instance != null && ProfileManager.Instance.profile.level < quest.requiredLevel))
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
        activeQuestsByID[quest.questID] = quest;

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
            SaveSystem.SaveGame();
        }
    }

    void CompleteObjective(QuestData quest, QuestObjective objective, ObjectiveRuntimeState objState)
    {
        objState.isCompleted = true;
        objective.onObjectiveComplete?.Invoke();
        OnObjectiveCompleted?.Invoke(quest, objective);
        Debug.Log($"Objective completed: {objective.description}");

        bool allComplete = quest.objectives.All(obj =>
            obj.isOptional || GetObjectiveState(quest.questID, obj.objectiveID).isCompleted);

        if (allComplete)
            CompleteQuest(quest);
        else
            SaveSystem.SaveGame();
    }

    public void CompleteQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID))
            return;

        activeQuestsByID.Remove(quest.questID);

        if (quest.questType is not QuestType.Repeatable and not QuestType.Daily)
        {
            completedQuests.Add(quest);
            completedQuestIDs.Add(quest.questID);
        }

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
        if (!IsQuestActive(quest.questID) || !quest.canFail)
            return;

        activeQuestsByID.Remove(quest.questID);
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

        activeQuestsByID.Remove(quest.questID);
        questTimers.Remove(quest.questID);
        runtimeStates.Remove(quest.questID);
        OnQuestAbandoned?.Invoke(quest);
        Debug.Log($"Quest abandoned: {quest.questName}");
        SaveSystem.SaveGame();
    }

    void OnCombatVictory(EnemyData killedEnemy)
    {
        if (killedEnemy == null)
            return;

        RefreshActiveQuestBuffer();

        foreach (var quest in activeQuestBuffer)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type != QuestObjectiveType.KillEnemies || objective.targetEnemy != killedEnemy)
                    continue;

                var state = GetObjectiveState(quest.questID, objective.objectiveID);

                if (!state.isCompleted)
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, 1);
            }
        }
    }

    void CheckCollectObjectives()
    {
        bool dirty = false;
        RefreshActiveQuestBuffer();

        foreach (var quest in activeQuestBuffer)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type != QuestObjectiveType.CollectItems)
                    continue;

                var objState = GetObjectiveState(quest.questID, objective.objectiveID);

                if (objState.isCompleted)
                    continue;

                int currentCount = InventoryManager.Instance.GetQuantity(objective.targetItem);
                int required = objective.GetRequiredCount();
                int newProgress = Mathf.Min(currentCount, required);
                int delta = newProgress - objState.currentProgress;

                if (delta > 0)
                {
                    objState.currentProgress += delta;

                    if (objState.currentProgress >= required)
                    {
                        objState.currentProgress = required;
                        CompleteObjective(quest, objective, objState);
                    }
                    else
                    {
                        OnObjectiveUpdated?.Invoke(quest, objective);
                    }

                    dirty = true;
                }

                if (objState.isCompleted && objective.consumeItems)
                    InventoryManager.Instance.RemoveItem(objective.targetItem, objective.itemCount);
            }
        }

        if (dirty)
            SaveSystem.SaveGame();
    }

    void CheckSpendObjectives(CurrencyType type, int amount)
    {
        RefreshActiveQuestBuffer();

        foreach (var quest in activeQuestBuffer)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type != QuestObjectiveType.SpendCurrency || objective.currencyType != type)
                    continue;

                var state = GetObjectiveState(quest.questID, objective.objectiveID);

                if (!state.isCompleted)
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, amount);
            }
        }
    }

    public QuestSaveData GetSaveData()
    {
        var data = new QuestSaveData
        {
            activeQuestIDs = activeQuestsByID.Keys.ToList(),
            completedQuestIDs = completedQuestIDs.ToList(),
            runtimeStates = runtimeStates.Values.ToList(),
            questTimerKeys = new List<string>(),
            questTimerValues = new List<float>()
        };

        foreach (var kvp in questTimers)
        {
            data.questTimerKeys.Add(kvp.Key);
            data.questTimerValues.Add(kvp.Value);
        }

        OnSaveDataRequested?.Invoke(data);
        return data;
    }

    public void LoadSaveData(QuestSaveData data)
    {
        if (data == null)
            return;

        if (allQuests == null || allQuests.Length == 0)
        {
            Debug.LogError("[QuestManager] allQuests atanmamış, save data yüklenemedi.");
            return;
        }

        activeQuestsByID.Clear();
        completedQuests.Clear();
        completedQuestIDs.Clear();
        runtimeStates.Clear();
        questTimers.Clear();
        var questLookup = allQuests.ToDictionary(q => q.questID);

        foreach (var id in data.activeQuestIDs)
            if (questLookup.TryGetValue(id, out var q))
                activeQuestsByID[id] = q;

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

        if (data.questTimerKeys != null && data.questTimerValues != null)
            for (int i = 0; i < data.questTimerKeys.Count; i++)
                questTimers[data.questTimerKeys[i]] = data.questTimerValues[i];

        OnTrackedQuestsLoaded?.Invoke(data.trackedQuestIDs);
    }

    public bool IsQuestActive(string questID) => activeQuestsByID.ContainsKey(questID);

    public bool IsQuestCompleted(string questID) => completedQuestIDs.Contains(questID);

    public QuestData GetActiveQuest(string questID) => activeQuestsByID.TryGetValue(questID, out var q) ? q : null;

    public QuestObjective GetObjective(QuestData quest, string objectiveID) => quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);

    public List<QuestData> GetActiveQuests() => activeQuestsByID.Values.ToList();

    public List<QuestData> GetCompletedQuests() => new(completedQuests);

    public List<QuestData> GetAvailableQuests() => allQuests?.Where(CanStartQuest).ToList() ?? new();

    public float GetQuestTimeRemaining(string questID) => questTimers.TryGetValue(questID, out float t) ? t : 0f;

    public void ToggleTracking(QuestData quest)
    {
        if (IsQuestActive(quest.questID))
            OnTrackingToggleRequested?.Invoke(quest);
    }
}
