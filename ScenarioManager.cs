using UnityEngine;
using System.Collections.Generic;
using System;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    [Header("Active Scenario")]
    public ScenarioData currentScenario;
    private int currentStepIndex = 0;
    private bool isScenarioActive = false;

    [Header("Available Scenarios")]
    public ScenarioData[] availableScenarios;

    private HashSet<string> completedScenarios = new();

    public event Action<ScenarioData> OnScenarioStart;
    public event Action<ScenarioData> OnScenarioComplete;
    public event Action<ScenarioStep> OnStepStart;
    public event Action<ScenarioStep> OnStepComplete;

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

    public bool CanStartScenario(ScenarioData scenario)
    {
        // Check level
        if (ProfileManager.Instance != null)
        {
            if (ProfileManager.Instance.profile.level < scenario.requiredLevel)
                return false;
        }

        // Check flags
        if (scenario.requiredFlags != null)
        {
            foreach (var flag in scenario.requiredFlags)
            {
                if (!StoryFlags.Has(flag))
                    return false;
            }
        }

        // Check prerequisites
        if (scenario.prerequisiteScenarios != null)
        {
            foreach (var prereq in scenario.prerequisiteScenarios)
            {
                if (!IsScenarioCompleted(prereq.scenarioID))
                    return false;
            }
        }

        return true;
    }

    public void StartScenario(ScenarioData scenario)
    {
        if (!CanStartScenario(scenario))
        {
            Debug.Log("Cannot start scenario: requirements not met");
            return;
        }

        currentScenario = scenario;
        currentStepIndex = 0;
        isScenarioActive = true;

        OnScenarioStart?.Invoke(scenario);

        // Start intro dialogue
        if (scenario.introDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(scenario.introDialogue, () => StartNextStep());
        }
        else
        {
            StartNextStep();
        }

        Debug.Log($"Started scenario: {scenario.scenarioName}");
    }

    void StartNextStep()
    {
        if (!isScenarioActive || currentScenario == null) return;

        if (currentStepIndex >= currentScenario.steps.Length)
        {
            CompleteScenario();
            return;
        }

        ScenarioStep step = currentScenario.steps[currentStepIndex];
        OnStepStart?.Invoke(step);

        step.onStepStart?.Invoke();

        Debug.Log($"Starting step: {step.stepName}");

        // Execute step based on type
        switch (step.type)
        {
            case ScenarioStepType.Dialogue:
                ExecuteDialogueStep(step);
                break;
            case ScenarioStepType.Combat:
                ExecuteCombatStep(step);
                break;
            case ScenarioStepType.CollectItem:
                ExecuteCollectItemStep(step);
                break;
            case ScenarioStepType.GoToLocation:
                ExecuteLocationStep(step);
                break;
            case ScenarioStepType.Wait:
                ExecuteWaitStep(step);
                break;
            case ScenarioStepType.Custom:
                // Custom steps handled by events
                break;
        }
    }

    void ExecuteDialogueStep(ScenarioStep step)
    {
        if (step.dialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(step.dialogue, () => CompleteCurrentStep());
        }
        else
        {
            CompleteCurrentStep();
        }
    }

    void ExecuteCombatStep(ScenarioStep step)
    {
        if (step.enemy != null && CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded += OnCombatStepComplete;
            CombatManager.Instance.StartCombat(step.enemy);
        }
        else
        {
            CompleteCurrentStep();
        }
    }

    void OnCombatStepComplete()
    {
        CombatManager.Instance.OnCombatEnded -= OnCombatStepComplete;
        CompleteCurrentStep();
    }

    void ExecuteCollectItemStep(ScenarioStep step)
    {
        // Check if player already has items
        if (InventoryManager.Instance != null)
        {
            int quantity = InventoryManager.Instance.GetQuantity(step.requiredItem);
            if (quantity >= step.requiredQuantity)
            {
                // Remove items
                InventoryManager.Instance.RemoveItem(step.requiredItem, step.requiredQuantity);
                CompleteCurrentStep();
                return;
            }
        }

        // Wait for items to be collected
        StartCoroutine(WaitForItemCollection(step));
    }

    System.Collections.IEnumerator WaitForItemCollection(ScenarioStep step)
    {
        while (true)
        {
            if (InventoryManager.Instance != null)
            {
                int quantity = InventoryManager.Instance.GetQuantity(step.requiredItem);
                if (quantity >= step.requiredQuantity)
                {
                    InventoryManager.Instance.RemoveItem(step.requiredItem, step.requiredQuantity);
                    CompleteCurrentStep();
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void ExecuteLocationStep(ScenarioStep step)
    {
        // Check if player is already at location
        GameObject target = GameObject.FindGameObjectWithTag(step.targetLocationTag);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (target != null && player != null)
        {
            StartCoroutine(WaitForLocation(target, player));
        }
        else
        {
            CompleteCurrentStep();
        }
    }

    System.Collections.IEnumerator WaitForLocation(GameObject target, GameObject player)
    {
        while (true)
        {
            float distance = Vector3.Distance(target.transform.position, player.transform.position);
            if (distance < 2f)
            {
                CompleteCurrentStep();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void ExecuteWaitStep(ScenarioStep step)
    {
        StartCoroutine(WaitForDuration(step.waitDuration));
    }

    System.Collections.IEnumerator WaitForDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        CompleteCurrentStep();
    }

    public void CompleteCurrentStep()
    {
        if (!isScenarioActive || currentScenario == null) return;

        ScenarioStep step = currentScenario.steps[currentStepIndex];
        step.onStepComplete?.Invoke();

        OnStepComplete?.Invoke(step);

        Debug.Log($"Completed step: {step.stepName}");

        currentStepIndex++;
        StartNextStep();
    }

    void CompleteScenario()
    {
        if (currentScenario == null) return;

        // Mark as completed
        completedScenarios.Add(currentScenario.scenarioID);

        // Give rewards
        GiveScenarioRewards();

        // Set flags
        if (currentScenario.flagsToSet != null)
        {
            foreach (var flag in currentScenario.flagsToSet)
            {
                StoryFlags.Add(flag);
            }
        }

        // Outro dialogue
        if (currentScenario.outroDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(currentScenario.outroDialogue, () => FinalizeScenario());
        }
        else
        {
            FinalizeScenario();
        }
    }

    void GiveScenarioRewards()
    {
        // Experience
        if (currentScenario.experienceReward > 0 && ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(currentScenario.experienceReward);
        }

        // Currency
        if (currentScenario.currencyRewards != null)
        {
            foreach (var reward in currentScenario.currencyRewards)
            {
                reward.Grant();
            }
        }

        // Items
        if (currentScenario.itemRewards != null && InventoryManager.Instance != null)
        {
            foreach (var item in currentScenario.itemRewards)
            {
                InventoryManager.Instance.AddItem(item, 1);
            }
        }
    }

    void FinalizeScenario()
    {
        Debug.Log($"Scenario completed: {currentScenario.scenarioName}");

        OnScenarioComplete?.Invoke(currentScenario);

        isScenarioActive = false;
        currentScenario = null;
        currentStepIndex = 0;

        SaveSystem.SaveGame();
    }

    public bool IsScenarioCompleted(string scenarioID)
    {
        return completedScenarios.Contains(scenarioID);
    }

    public bool IsScenarioActive() => isScenarioActive;

    public ScenarioData GetCurrentScenario() => currentScenario;

    public int GetCurrentStepIndex() => currentStepIndex;

    public HashSet<string> GetCompletedScenarios() => new(completedScenarios);

    public void SetCompletedScenarios(HashSet<string> scenarios)
    {
        completedScenarios = new(scenarios);
    }
}
