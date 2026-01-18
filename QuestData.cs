using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questID;
    public string questName;
    [TextArea(3, 6)]
    public string description;
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
    public DialogueNode completeDialogue;

    [Header("Rewards")]
    public int experienceReward = 100;
    public CurrencyReward[] currencyRewards;
    public ItemData[] itemRewards;
    public int itemRewardQuantities;

    [Header("Optional Rewards (Choose One)")]
    public ItemData[] optionalRewards;

    [Header("Quest Tracking")]
    public bool trackObjectives = true;
    public bool showOnMap = true;
    public Vector3 questMarkerPosition;

    [Header("Time Limit")]
    public bool hasTimeLimit = false;
    public float timeLimitSeconds = 300f;

    [Header("Events")]
    public UnityEvent onQuestStart;
    public UnityEvent onQuestComplete;
    public UnityEvent onQuestFail;

    [Header("Flags")]
    public string[] flagsToSetOnStart;
    public string[] flagsToSetOnComplete;
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
