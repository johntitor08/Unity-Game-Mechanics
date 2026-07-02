using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questID;
    public string questName;
    public string questNameTR;
    [TextArea(3, 6)]
    public string description;
    [TextArea(3, 6)]
    public string descriptionTR;
    public Sprite icon;
    public QuestType questType = QuestType.Main;
    public QuestDifficulty difficulty = QuestDifficulty.Normal;

    [Header("Requirements")]
    public int requiredLevel = 1;
    public string[] requiredFlags;
    public QuestData[] prerequisiteQuests;

    [Header("Quest Objectives")]
    public QuestObjective[] objectives;

    [Header("Dialogue")]
    public DialogueNode startDialogue;
    public DialogueNode progressDialogue;
    public DialogueNode completionDialogue;
    public DialogueNode completeDialogue;
    public DialogueNode failureDialogue;

    [Header("Rewards")]
    public int experienceReward = 100;
    public CurrencyReward[] currencyRewards;
    public ItemData[] itemRewards;
    public int[] itemRewardQuantities;

    [Header("Optional Rewards (Choose One)")]
    public ItemData[] optionalRewards;

    [Header("Quest Tracking")]
    public bool trackObjectives = true;
    public bool showOnMap = true;
    public Vector3 questMarkerPosition;

    [Header("Completion")]
    public bool autoCompleteWhenObjectivesComplete = false;

    [Header("Time Limit")]
    public bool hasTimeLimit = false;
    public float timeLimitSeconds = 300f;

    [Header("Failure")]
    public bool canFail = false;

    [Header("Events")]
    public UnityEvent onQuestStart;
    public UnityEvent onQuestComplete;
    public UnityEvent onQuestFail;

    [Header("Flags")]
    public string[] flagsToSetOnStart;
    public string[] flagsToSetOnComplete;

    public string DisplayName => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(questNameTR) ? questNameTR : questName;
    public string DisplayDescription => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(descriptionTR) ? descriptionTR : description;
}

public enum QuestType
{
    Main,
    Side,
    Daily,
    Repeatable,
    Event
}

public enum QuestDifficulty
{
    Easy,
    Normal,
    Hard,
    Elite,
    Epic
}
