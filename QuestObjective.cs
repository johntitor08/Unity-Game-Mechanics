using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [Header("Objective Info")]
    public string objectiveID;
    public string description;
    public QuestObjectiveType type;

    [Header("Target")]
    // Kill enemies
    public EnemyData targetEnemy;
    public int targetCount = 1;

    // Collect items
    public ItemData targetItem;
    public int itemCount = 1;
    public bool consumeItems = true;

    // Talk to NPC
    public string npcTag;

    // Go to location
    public string locationTag;
    public float locationRadius = 2f;

    // Interact with object
    public string interactObjectTag;

    // Craft items
    public ItemData craftTarget;
    public int craftCount = 1;

    // Spend currency
    public CurrencyType currencyType;
    public int currencyAmount = 100;

    [Header("Progress")]
    public int currentProgress = 0;
    public bool isCompleted = false;
    public bool isOptional = false;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onObjectiveStart;
    public UnityEngine.Events.UnityEvent onObjectiveComplete;

    public bool IsComplete()
    {
        return currentProgress >= GetRequiredCount();
    }

    public int GetRequiredCount()
    {
        return type switch
        {
            QuestObjectiveType.KillEnemies => targetCount,
            QuestObjectiveType.CollectItems => itemCount,
            QuestObjectiveType.CraftItems => craftCount,
            QuestObjectiveType.SpendCurrency => currencyAmount,
            _ => 1
        };
    }

    public float GetProgressPercentage()
    {
        int required = GetRequiredCount();
        if (required == 0) return 1f;
        return Mathf.Clamp01((float)currentProgress / required);
    }
}

public enum QuestObjectiveType
{
    KillEnemies,
    CollectItems,
    TalkToNPC,
    GoToLocation,
    InteractWithObject,
    CraftItems,
    SpendCurrency,
    Custom
}
