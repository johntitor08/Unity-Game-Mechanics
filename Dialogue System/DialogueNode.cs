using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Speaker")]
    public string speakerName = "NPC";
    public Sprite speakerPortrait;
    public Color speakerNameColor = Color.gold;

    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    public string[] lines;
    public string speakerNameTR;
    [TextArea(2, 5)]
    public string[] linesTR;

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
    public string cameraTargetTag = "MainCamera";

    [Header("Character")]
    public Sprite characterImage;

    [Header("Background")]
    public Sprite backgroundImage;
    public bool fadeToBlack = false;

    [Header("Scene")]
    public SceneProgress sceneContext = SceneProgress.Scene1;

    public string[] flagsToSetOnExit;
    public bool isFinalNode;
    public string DisplaySpeaker => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(speakerNameTR) ? speakerNameTR : speakerName;

    public string GetDisplayLine(int i)
    {
        if (LanguageManager.Current == GameLanguage.TR && linesTR != null && i >= 0 && i < linesTR.Length && !string.IsNullOrEmpty(linesTR[i]))
            return linesTR[i];

        return (lines != null && i >= 0 && i < lines.Length) ? lines[i] : "";
    }
}

[System.Serializable]
public class DialogueChoice
{
    [Header("Choice Text")]
    public string choiceText;
    public string choiceTextTR;
    public DialogueNode nextNode;
    public string DisplayChoiceText => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(choiceTextTR) ? choiceTextTR : choiceText;

    [Header("Conditions")]
    public bool requiresItem;
    public ItemData requiredItem;
    public bool requiresStat;
    public StatType requiredStat;
    public int requiredStatValue;
    public bool requiresCurrency;
    public CurrencyType requiredCurrency;
    public int requiredCurrencyAmount;
    public bool requiresFlag;
    public string requiredFlag;

    [Header("Effects")]
    public bool consumeItem;
    public bool setFlag;
    public string flagToSet;

    [Header("Affinity")]
    public string affinityTarget;
    public int affinityDelta;

    [Header("Affinity Requirement")]
    public bool requiresAffinity;
    public string affinityCharacter;
    public int requiredAffinity;

    [Header("Rewards")]
    public bool giveReward;
    public CurrencyReward[] currencyRewards;
    public ItemData[] itemRewards;
    public int experienceReward;

    [Header("Visual")]
    public Color choiceColor = Color.black;
    public bool isDisabledChoice = false;
}
