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
    public event Action<QuestData> OnQuestReadyToComplete;
    public event Action<QuestData, QuestObjective> OnObjectiveUpdated;
    public event Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public event Action<QuestData> OnTrackingToggleRequested;
    public event Action<QuestSaveData> OnSaveDataRequested;
    public event Action<List<string>> OnTrackedQuestsLoaded;
    public static event Action OnReady;
    private readonly List<QuestData> activeQuests = new();
    private readonly List<QuestData> completedQuests = new();
    private readonly Dictionary<string, QuestRuntimeState> runtimeStates = new();
    private readonly Dictionary<string, float> questTimers = new();
    private readonly Dictionary<string, ItemData> selectedOptionalRewards = new();

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
        }
    }

    void Start()
    {
        OnReady?.Invoke();
        SubscribeExternalManagers();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        UnsubscribeExternalManagers();
    }

    void SubscribeExternalManagers()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatVictory += CheckKillObjectives;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += CheckCollectObjectives;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencySpent += CheckSpendObjectives;
    }

    void UnsubscribeExternalManagers()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatVictory -= CheckKillObjectives;

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
        var keys = questTimers.Keys.ToList();
        List<string> expiredQuests = new();

        foreach (var key in keys)
        {
            questTimers[key] -= Time.deltaTime;

            if (questTimers[key] <= 0)
                expiredQuests.Add(key);
        }

        foreach (var questID in expiredQuests)
        {
            var quest = GetActiveQuest(questID);

            if (quest != null)
                FailQuest(quest);
            else
                questTimers.Remove(questID);
        }
    }

    public bool CanStartQuest(QuestData quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questID))
            return false;

        bool isCompletedAndNotRepeatable = IsQuestCompleted(quest.questID) && quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily;
        bool dailyLimitReached = quest.questType == QuestType.Daily && activeQuests.Count(q => q.questType == QuestType.Daily) >= maxDailyQuests;
        bool levelTooLow = ProfileManager.Instance != null && ProfileManager.Instance.profile.level < quest.requiredLevel;

        if (IsQuestActive(quest.questID) || isCompletedAndNotRepeatable || activeQuests.Count >= maxActiveQuests || dailyLimitReached || levelTooLow)
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

        foreach (var objective in GetObjectives(quest))
        {
            var objState = runtimeState.GetObjective(objective.objectiveID);
            objState.currentProgress = 0;
            objState.isCompleted = false;
            objective.onObjectiveStart?.Invoke();
        }

        runtimeStates[quest.questID] = runtimeState;
        activeQuests.Add(quest);

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
        if (amount <= 0)
            return;

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

    public void NotifyTalkToNPC(string npcTag, int amount = 1)
    {
        UpdateMatchingObjectives(QuestObjectiveType.TalkToNPC, objective => MatchesTag(objective.npcTag, npcTag), amount);
    }

    public void NotifyLocationReached(string locationTag, int amount = 1)
    {
        UpdateMatchingObjectives(QuestObjectiveType.GoToLocation, objective => MatchesTag(objective.locationTag, locationTag), amount);
    }

    public void NotifyObjectInteracted(string interactObjectTag, int amount = 1)
    {
        UpdateMatchingObjectives(QuestObjectiveType.InteractWithObject, objective => MatchesTag(objective.interactObjectTag, interactObjectTag), amount);
    }

    public void NotifyItemCrafted(ItemData craftedItem, int amount = 1)
    {
        if (craftedItem == null)
            return;

        UpdateMatchingObjectives(QuestObjectiveType.CraftItems, objective => objective.craftTarget == craftedItem, amount);
    }

    public void NotifyCustomObjective(string objectiveID, int amount = 1)
    {
        if (string.IsNullOrEmpty(objectiveID))
            return;

        UpdateMatchingObjectives(QuestObjectiveType.Custom, objective => objective.objectiveID == objectiveID, amount);
    }

    void CompleteObjective(QuestData quest, QuestObjective objective, ObjectiveRuntimeState objState)
    {
        objState.isCompleted = true;
        objective.onObjectiveComplete?.Invoke();
        OnObjectiveCompleted?.Invoke(quest, objective);
        Debug.Log($"Objective completed: {objective.description}");

        if (!AreRequiredObjectivesComplete(quest))
            return;

        if (quest.autoCompleteWhenObjectivesComplete)
            CompleteQuest(quest);
        else
            OnQuestReadyToComplete?.Invoke(quest);
    }

    public void CompleteQuest(QuestData quest)
    {
        CompleteQuest(quest, GetSelectedOptionalReward(quest != null ? quest.questID : null));
    }

    public void CompleteQuest(QuestData quest, ItemData selectedOptionalReward)
    {
        if (quest == null || !IsQuestActive(quest.questID))
            return;

        activeQuests.Remove(quest);

        if (quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily)
            completedQuests.Add(quest);

        if (questTimers.ContainsKey(quest.questID))
            questTimers.Remove(quest.questID);

        var optionalReward = ResolveOptionalReward(quest, selectedOptionalReward);
        GiveQuestRewards(quest, optionalReward);
        selectedOptionalRewards.Remove(quest.questID);

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

    void GiveQuestRewards(QuestData quest, ItemData selectedOptionalReward)
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
                if (quest.itemRewards[i] == null)
                    continue;

                int qty = (quest.itemRewardQuantities != null && i < quest.itemRewardQuantities.Length) ? quest.itemRewardQuantities[i] : 1;
                InventoryManager.Instance.AddItem(quest.itemRewards[i], qty);
            }
        }

        if (selectedOptionalReward != null && InventoryManager.Instance != null)
            InventoryManager.Instance.AddItem(selectedOptionalReward, 1);

        if (QuestRewardUI.Instance != null)
            QuestRewardUI.Instance.ShowRewards(quest, selectedOptionalReward);
    }

    public void FailQuest(QuestData quest)
    {
        if (quest == null || !IsQuestActive(quest.questID))
            return;

        if (!quest.canFail)
        {
            questTimers.Remove(quest.questID);
            return;
        }

        activeQuests.Remove(quest);
        selectedOptionalRewards.Remove(quest.questID);

        if (questTimers.ContainsKey(quest.questID))
            questTimers.Remove(quest.questID);

        quest.onQuestFail?.Invoke();
        OnQuestFailed?.Invoke(quest);

        if (quest.failureDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(quest.failureDialogue);

        Debug.Log($"Quest failed: {quest.questName}");
        SaveSystem.SaveGame();
    }

    public void AbandonQuest(QuestData quest)
    {
        if (quest == null || !IsQuestActive(quest.questID))
            return;

        activeQuests.Remove(quest);
        selectedOptionalRewards.Remove(quest.questID);

        if (questTimers.ContainsKey(quest.questID))
            questTimers.Remove(quest.questID);

        OnQuestAbandoned?.Invoke(quest);
        Debug.Log($"Quest abandoned: {quest.questName}");
        SaveSystem.SaveGame();
    }

    public void ToggleTracking(QuestData quest)
    {
        OnTrackingToggleRequested?.Invoke(quest);
    }

    public bool SelectOptionalReward(QuestData quest, ItemData reward)
    {
        if (quest == null || reward == null || !ContainsOptionalReward(quest, reward))
            return false;

        selectedOptionalRewards[quest.questID] = reward;
        SaveSystem.SaveGame();
        return true;
    }

    public ItemData GetSelectedOptionalReward(string questID)
    {
        if (string.IsNullOrEmpty(questID))
            return null;

        return selectedOptionalRewards.TryGetValue(questID, out var reward) ? reward : null;
    }

    void CheckKillObjectives(EnemyData killedEnemy)
    {
        if (killedEnemy == null)
            return;

        foreach (var quest in activeQuests.ToList())
        {
            foreach (var objective in GetObjectives(quest))
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
            foreach (var objective in GetObjectives(quest))
            {
                if (objective.type != QuestObjectiveType.CollectItems)
                    continue;

                var state = GetObjectiveState(quest.questID, objective.objectiveID);

                if (state.isCompleted)
                    continue;

                int currentCount = InventoryManager.Instance.GetQuantity(objective.targetItem);
                int required = objective.GetRequiredCount();
                int newProgress = Mathf.Min(currentCount, required);

                if (newProgress > state.currentProgress)
                {
                    int delta = newProgress - state.currentProgress;
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, delta);
                    var updatedState = GetObjectiveState(quest.questID, objective.objectiveID);

                    if (updatedState.isCompleted && objective.consumeItems)
                        InventoryManager.Instance.RemoveItem(objective.targetItem, objective.itemCount);
                }
            }
        }
    }

    void CheckSpendObjectives(CurrencyType type, int amount)
    {
        foreach (var quest in activeQuests.ToList())
        {
            foreach (var objective in GetObjectives(quest))
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

        foreach (var kvp in selectedOptionalRewards)
        {
            if (kvp.Value == null || string.IsNullOrEmpty(kvp.Value.itemID))
                continue;

            data.selectedOptionalRewardQuestIDs.Add(kvp.Key);
            data.selectedOptionalRewardItemIDs.Add(kvp.Value.itemID);
        }

        OnSaveDataRequested?.Invoke(data);
        return data;
    }

    public void LoadSaveData(QuestSaveData data)
    {
        if (data == null)
            return;

        activeQuests.Clear();
        completedQuests.Clear();
        runtimeStates.Clear();
        questTimers.Clear();
        selectedOptionalRewards.Clear();
        var questLookup = GetAllQuests().Where(q => !string.IsNullOrEmpty(q.questID)).ToDictionary(q => q.questID);

        foreach (var id in data.activeQuestIDs)
            if (questLookup.TryGetValue(id, out var q)) activeQuests.Add(q);

        foreach (var id in data.completedQuestIDs)
            if (questLookup.TryGetValue(id, out var q)) completedQuests.Add(q);

        if (data.runtimeStates != null)
            foreach (var state in data.runtimeStates)
            {
                state.RebuildLookup();
                runtimeStates[state.questID] = state;
            }

        for (int i = 0; i < data.questTimerKeys.Count; i++)
            questTimers[data.questTimerKeys[i]] = data.questTimerValues[i];

        for (int i = 0; i < data.selectedOptionalRewardQuestIDs.Count && i < data.selectedOptionalRewardItemIDs.Count; i++)
        {
            if (!questLookup.TryGetValue(data.selectedOptionalRewardQuestIDs[i], out var quest))
                continue;

            var reward = ResolveOptionalReward(quest, data.selectedOptionalRewardItemIDs[i]);

            if (reward != null)
                selectedOptionalRewards[quest.questID] = reward;
        }

        OnTrackedQuestsLoaded?.Invoke(data.trackedQuestIDs);
    }

    public bool IsQuestActive(string questID) => activeQuests.Any(q => q.questID == questID);

    public bool IsQuestCompleted(string questID) => completedQuests.Any(q => q.questID == questID);

    public QuestData GetActiveQuest(string questID) => activeQuests.FirstOrDefault(q => q.questID == questID);

    public QuestObjective GetObjective(QuestData quest, string objectiveID) => GetObjectives(quest).FirstOrDefault(o => o.objectiveID == objectiveID);

    public List<QuestData> GetActiveQuests() => new(activeQuests);

    public List<QuestData> GetCompletedQuests() => new(completedQuests);

    public List<QuestData> GetAvailableQuests() => GetAllQuests().Where(CanStartQuest).ToList();

    public float GetQuestTimeRemaining(string questID) => questTimers.TryGetValue(questID, out float t) ? t : 0f;

    public bool AreRequiredObjectivesComplete(QuestData quest)
    {
        if (quest == null)
            return false;

        return GetObjectives(quest).All(obj =>
            obj.isOptional || GetObjectiveState(quest.questID, obj.objectiveID).isCompleted);
    }

    private IEnumerable<QuestData> GetAllQuests()
    {
        return allQuests != null ? allQuests.Where(q => q != null) : Enumerable.Empty<QuestData>();
    }

    private IEnumerable<QuestObjective> GetObjectives(QuestData quest)
    {
        return quest != null && quest.objectives != null ? quest.objectives.Where(o => o != null) : Enumerable.Empty<QuestObjective>();
    }

    private void UpdateMatchingObjectives(QuestObjectiveType type, Func<QuestObjective, bool> predicate, int amount)
    {
        if (amount <= 0 || predicate == null)
            return;

        foreach (var quest in activeQuests.ToList())
        {
            foreach (var objective in GetObjectives(quest))
            {
                if (objective.type != type || !predicate(objective))
                    continue;

                var state = GetObjectiveState(quest.questID, objective.objectiveID);

                if (!state.isCompleted)
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, amount);
            }
        }
    }

    private bool MatchesTag(string expected, string actual)
    {
        return !string.IsNullOrEmpty(expected) && expected == actual;
    }

    private bool ContainsOptionalReward(QuestData quest, ItemData reward)
    {
        return quest.optionalRewards != null && quest.optionalRewards.Any(item => item == reward);
    }

    private ItemData ResolveOptionalReward(QuestData quest, ItemData selectedReward)
    {
        if (selectedReward != null && ContainsOptionalReward(quest, selectedReward))
            return selectedReward;

        return quest.optionalRewards?.FirstOrDefault(item => item != null);
    }

    private ItemData ResolveOptionalReward(QuestData quest, string itemID)
    {
        if (quest == null || quest.optionalRewards == null || string.IsNullOrEmpty(itemID))
            return null;

        return quest.optionalRewards.FirstOrDefault(item => item != null && item.itemID == itemID);
    }
}
