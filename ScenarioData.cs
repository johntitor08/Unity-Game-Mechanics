using UnityEngine;

[CreateAssetMenu(menuName = "Scenario/Scenario Data")]
public class ScenarioData : ScriptableObject
{
    [Header("Scenario Info")]
    public string scenarioID;
    public string scenarioName;
    [TextArea] public string description;
    public Sprite thumbnail;

    [Header("Requirements")]
    public int requiredLevel = 1;
    public string[] requiredFlags;
    public ScenarioData[] prerequisiteScenarios;

    [Header("Scenario Flow")]
    public DialogueNode introDialogue;
    public ScenarioStep[] steps;
    public DialogueNode outroDialogue;

    [Header("Rewards")]
    public int experienceReward;
    public CurrencyReward[] currencyRewards;
    public ItemReward[] itemRewards;
    public string[] flagsToSet;

    [Header("Failure")]
    public bool canFail = false;
    public DialogueNode failureDialogue;
}

[System.Serializable]
public class ScenarioStep
{
    public string stepName;
    [TextArea] public string stepDescription;
    public ScenarioStepType type;
    [Header("Step Data")]
    public EnemyData enemy;
    public DialogueNode dialogue;
    public ItemData requiredItem;
    public int requiredQuantity = 1;
    public string targetLocationTag;
    public float waitDuration = 5f;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onStepStart;
    public UnityEngine.Events.UnityEvent onStepComplete;
    public UnityEngine.Events.UnityEvent onCustomStepEvent;
}

public enum ScenarioStepType
{
    Dialogue,
    Combat,
    CollectItem,
    GoToLocation,
    Wait,
    Custom
}

[System.Serializable]
public class ItemReward
{
    public ItemData item;
    public int quantity = 1;
}
