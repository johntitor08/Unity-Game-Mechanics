using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Available Quests")]
    public QuestData[] allQuests;

    [Header("Quest Tracking")]
    public int maxActiveQuests = 5;
    public int maxDailyQuests = 3;

    private List<QuestData> activeQuests = new();
    private List<QuestData> completedQuests = new();
    private Dictionary<string, float> questTimers = new();

    public event Action<QuestData> OnQuestStarted;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestFailed;
    public event Action<QuestData, QuestObjective> OnObjectiveUpdated;
    public event Action<QuestData, QuestObjective> OnObjectiveCompleted;

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
        // Subscribe to game events for quest tracking
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded += CheckKillObjectives;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnChanged += CheckCollectObjectives;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencySpent += CheckSpendObjectives;
        }
    }

    void Update()
    {
        UpdateQuestTimers();
    }

    void UpdateQuestTimers()
    {
        List<string> expiredQuests = new();

        foreach (var kvp in questTimers)
        {
            questTimers[kvp.Key] -= Time.deltaTime;

            if (questTimers[kvp.Key] <= 0)
            {
                expiredQuests.Add(kvp.Key);
            }
        }

        foreach (var questID in expiredQuests)
        {
            var quest = GetActiveQuest(questID);
            if (quest != null)
            {
                FailQuest(quest);
            }
        }
    }

    public bool CanStartQuest(QuestData quest)
    {
        // Check if already active or completed
        if (IsQuestActive(quest.questID))
        {
            Debug.Log("Quest already active");
            return false;
        }

        if (IsQuestCompleted(quest.questID) && quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily)
        {
            Debug.Log("Quest already completed");
            return false;
        }

        // Check max active quests
        if (activeQuests.Count >= maxActiveQuests)
        {
            Debug.Log("Too many active quests");
            return false;
        }

        // Check level requirement
        if (ProfileManager.Instance != null)
        {
            if (ProfileManager.Instance.profile.level < quest.requiredLevel)
            {
                Debug.Log($"Level {quest.requiredLevel} required");
                return false;
            }
        }

        // Check flag requirements
        if (quest.requiredFlags != null)
        {
            foreach (var flag in quest.requiredFlags)
            {
                if (!StoryFlags.Has(flag))
                {
                    Debug.Log($"Missing required flag: {flag}");
                    return false;
                }
            }
        }

        // Check prerequisite quests
        if (quest.prerequisiteQuests != null)
        {
            foreach (var prereq in quest.prerequisiteQuests)
            {
                if (!IsQuestCompleted(prereq.questID))
                {
                    Debug.Log($"Prerequisite quest not completed: {prereq.questName}");
                    return false;
                }
            }
        }

        return true;
    }

    public bool StartQuest(QuestData quest)
    {
        if (!CanStartQuest(quest))
            return false;

        // Reset objectives
        foreach (var objective in quest.objectives)
        {
            objective.currentProgress = 0;
            objective.isCompleted = false;
            objective.onObjectiveStart?.Invoke();
        }

        activeQuests.Add(quest);

        // Set flags
        if (quest.flagsToSetOnStart != null)
        {
            foreach (var flag in quest.flagsToSetOnStart)
            {
                StoryFlags.Add(flag);
            }
        }

        // Start timer if applicable
        if (quest.hasTimeLimit)
        {
            questTimers[quest.questID] = quest.timeLimitSeconds;
        }

        // Trigger events
        quest.onQuestStart?.Invoke();
        OnQuestStarted?.Invoke(quest);

        // Start dialogue
        if (quest.startDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(quest.startDialogue);
        }

        Debug.Log($"Quest started: {quest.questName}");
        SaveSystem.SaveGame();
        return true;
    }

    public void UpdateObjectiveProgress(string questID, string objectiveID, int amount = 1)
    {
        var quest = GetActiveQuest(questID);
        if (quest == null) return;

        var objective = GetObjective(quest, objectiveID);
        if (objective == null || objective.isCompleted) return;

        objective.currentProgress += amount;

        if (objective.currentProgress >= objective.GetRequiredCount())
        {
            CompleteObjective(quest, objective);
        }
        else
        {
            OnObjectiveUpdated?.Invoke(quest, objective);
        }

        SaveSystem.SaveGame();
    }

    void CompleteObjective(QuestData quest, QuestObjective objective)
    {
        objective.isCompleted = true;
        objective.onObjectiveComplete?.Invoke();
        OnObjectiveCompleted?.Invoke(quest, objective);

        Debug.Log($"Objective completed: {objective.description}");

        // Check if all objectives complete
        bool allComplete = true;
        foreach (var obj in quest.objectives)
        {
            if (!obj.isOptional && !obj.isCompleted)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            CompleteQuest(quest);
        }
    }

    public void CompleteQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID)) return;

        activeQuests.Remove(quest);

        if (quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily)
        {
            completedQuests.Add(quest);
        }

        // Remove timer
        if (questTimers.ContainsKey(quest.questID))
        {
            questTimers.Remove(quest.questID);
        }

        // Give rewards
        GiveQuestRewards(quest);

        // Set flags
        if (quest.flagsToSetOnComplete != null)
        {
            foreach (var flag in quest.flagsToSetOnComplete)
            {
                StoryFlags.Add(flag);
            }
        }

        // Trigger events
        quest.onQuestComplete?.Invoke();
        OnQuestCompleted?.Invoke(quest);

        // Complete dialogue
        if (quest.completeDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(quest.completeDialogue);
        }

        Debug.Log($"Quest completed: {quest.questName}");
        SaveSystem.SaveGame();
    }

    void GiveQuestRewards(QuestData quest)
    {
        // Experience
        if (quest.experienceReward > 0 && ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(quest.experienceReward);
        }

        // Currency
        if (quest.currencyRewards != null)
        {
            foreach (var reward in quest.currencyRewards)
            {
                reward.Grant();
            }
        }

        // Items
        if (quest.itemRewards != null && InventoryManager.Instance != null)
        {
            foreach (var item in quest.itemRewards)
            {
                int quantity = quest.itemRewardQuantities > 0 ? quest.itemRewardQuantities : 1;
                InventoryManager.Instance.AddItem(item, quantity);
            }
        }

        // Show reward UI
        if (QuestRewardUI.Instance != null)
        {
            QuestRewardUI.Instance.ShowRewards(quest);
        }
    }

    public void FailQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID)) return;

        activeQuests.Remove(quest);

        if (questTimers.ContainsKey(quest.questID))
        {
            questTimers.Remove(quest.questID);
        }

        quest.onQuestFail?.Invoke();
        OnQuestFailed?.Invoke(quest);

        Debug.Log($"Quest failed: {quest.questName}");
        SaveSystem.SaveGame();
    }

    public void AbandonQuest(QuestData quest)
    {
        if (!IsQuestActive(quest.questID)) return;

        activeQuests.Remove(quest);

        if (questTimers.ContainsKey(quest.questID))
        {
            questTimers.Remove(quest.questID);
        }

        Debug.Log($"Quest abandoned: {quest.questName}");
        SaveSystem.SaveGame();
    }

    // Event handlers for automatic tracking
    void CheckKillObjectives()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.currentEnemy == null)
            return;

        var killedEnemy = CombatManager.Instance.currentEnemy;

        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == QuestObjectiveType.KillEnemies &&
                    !objective.isCompleted &&
                    objective.targetEnemy == killedEnemy)
                {
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, 1);
                }
            }
        }
    }

    void CheckCollectObjectives()
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == QuestObjectiveType.CollectItems && !objective.isCompleted)
                {
                    int currentCount = InventoryManager.Instance.GetQuantity(objective.targetItem);
                    objective.currentProgress = currentCount;

                    if (objective.IsComplete() && !objective.isCompleted)
                    {
                        if (objective.consumeItems)
                        {
                            InventoryManager.Instance.RemoveItem(objective.targetItem, objective.itemCount);
                        }
                        CompleteObjective(quest, objective);
                    }
                }
            }
        }
    }

    void CheckSpendObjectives(CurrencyType type, int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.type == QuestObjectiveType.SpendCurrency &&
                    !objective.isCompleted &&
                    objective.currencyType == type)
                {
                    UpdateObjectiveProgress(quest.questID, objective.objectiveID, amount);
                }
            }
        }
    }

    // Query methods
    public bool IsQuestActive(string questID)
    {
        return activeQuests.Any(q => q.questID == questID);
    }

    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Any(q => q.questID == questID);
    }

    public QuestData GetActiveQuest(string questID)
    {
        return activeQuests.FirstOrDefault(q => q.questID == questID);
    }

    public QuestObjective GetObjective(QuestData quest, string objectiveID)
    {
        return quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);
    }

    public List<QuestData> GetActiveQuests() => new(activeQuests);
    public List<QuestData> GetCompletedQuests() => new(completedQuests);

    public List<QuestData> GetAvailableQuests()
    {
        return allQuests.Where(q => CanStartQuest(q)).ToList();
    }

    public float GetQuestTimeRemaining(string questID)
    {
        return questTimers.ContainsKey(questID) ? questTimers[questID] : 0f;
    }
}
