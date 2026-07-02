using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public ItemData targetItem;
    public int itemCount = 1;
    public bool consumeItems = true;
    public string npcTag;
    public string locationTag;
    public float locationRadius = 2f;
    public string interactObjectTag;
    public ItemData craftTarget;
    public int craftCount = 1;
    public CurrencyType currencyType;
    public int currencyAmount = 100;

    [Header("Objective Info")]
    public string objectiveID;
    public string description;
    public string descriptionTR;
    public QuestObjectiveType type;

    [Header("Target")]
    public EnemyData targetEnemy;
    public int targetCount = 1;

    [Header("Progress")]
    public bool isOptional = false;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onObjectiveStart;
    public UnityEngine.Events.UnityEvent onObjectiveComplete;

    public string DisplayDescription => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(descriptionTR) ? descriptionTR : description;

    public int GetRequiredCount()
    {
        return type switch
        {
            QuestObjectiveType.KillEnemies => targetCount,
            QuestObjectiveType.CollectItems => itemCount,
            QuestObjectiveType.CraftItems => craftCount,
            QuestObjectiveType.SpendCurrency => currencyAmount,
            QuestObjectiveType.TalkToNPC => targetCount > 0 ? targetCount : 1,
            QuestObjectiveType.InteractWithObject => targetCount > 0 ? targetCount : 1,
            QuestObjectiveType.GoToLocation => targetCount > 0 ? targetCount : 1,
            _ => 1
        };
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
