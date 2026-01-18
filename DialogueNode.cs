using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Speaker")]
    public string speakerName = "NPC";
    public Sprite speakerPortrait;
    public Color speakerNameColor = Color.wheat;

    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Choices")]
    public DialogueChoice[] choices;

    [Header("Auto Continue")]
    public bool autoAdvance = false;
    public float autoAdvanceDelay = 3f;

    [Header("Events")]
    public UnityEvent onEnter;
    public UnityEvent onExit;

    [Header("Camera")]
    public bool changeCameraOnEnter = false;
    public string cameraTargetTag = "DialogueCamera";

    [Header("Background")]
    public Sprite backgroundImage;
    public bool fadeToBlack = false;
}

[System.Serializable]
public class DialogueChoice
{
    [Header("Choice Text")]
    public string choiceText;
    public DialogueNode nextNode;

    [Header("Conditions")]
    public bool requiresFlag;
    public string requiredFlag;

    public bool requiresItem;
    public ItemData requiredItem;

    public bool requiresStat;
    public StatType requiredStat;
    public int requiredStatValue;

    public bool requiresCurrency;
    public CurrencyType requiredCurrency;
    public int requiredCurrencyAmount;

    [Header("Effects")]
    public bool consumeItem;
    public bool setFlag;
    public string flagToSet;

    public bool giveReward;
    public CurrencyReward[] currencyRewards;
    public ItemData[] itemRewards;
    public int experienceReward;

    [Header("Visual")]
    public Color choiceColor = Color.black;
    public bool isDisabledChoice = false;
}
