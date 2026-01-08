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
    public ItemData[] itemRewards;
    public string[] flagsToSet;

    [Header("Failure")]
    public bool canFail = false;
    public DialogueNode failureDialogue;
}

[System.Serializable]
public class ScenarioStep
{
    public string stepName;
    public string stepDescription;

    public ScenarioStepType type;

    // Combat step
    public EnemyData enemy;

    // Dialogue step
    public DialogueNode dialogue;

    // Item collection
    public ItemData requiredItem;
    public int requiredQuantity = 1;

    // Location step
    public string targetLocationTag;

    // Wait step
    public float waitDuration = 5f;

    // Custom step
    public UnityEngine.Events.UnityEvent onStepStart;
    public UnityEngine.Events.UnityEvent onStepComplete;
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
