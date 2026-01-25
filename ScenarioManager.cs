using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;
    private static readonly WaitForSeconds WAIT_HALF_SEC = new(0.5f);
    public event Action<ScenarioData> OnScenarioStart;
    public event Action<ScenarioData> OnScenarioComplete;
    public event Action<ScenarioStep> OnStepStart;
    public event Action<ScenarioStep> OnStepComplete;
    private readonly HashSet<string> completedScenarios = new();
    private Coroutine activeCoroutine;

    [Header("Active Scenario")]
    public ScenarioData currentScenario;
    private int currentStepIndex;
    private bool isScenarioActive;

    [Header("Available Scenarios")]
    public ScenarioData[] availableScenarios;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public bool CanStartScenario(ScenarioData scenario)
    {
        if (scenario == null) return false;

        if (ProfileManager.Instance != null &&
            ProfileManager.Instance.profile.level < scenario.requiredLevel)
            return false;

        if (scenario.requiredFlags != null)
        {
            foreach (var flag in scenario.requiredFlags)
                if (!StoryFlags.Has(flag)) return false;
        }

        if (scenario.prerequisiteScenarios != null)
        {
            foreach (var prereq in scenario.prerequisiteScenarios)
                if (!IsScenarioCompleted(prereq.scenarioID)) return false;
        }

        return true;
    }

    public void StartScenario(ScenarioData scenario)
    {
        if (!CanStartScenario(scenario))
        {
            Debug.LogWarning("Scenario cannot be started.");
            return;
        }

        StopActiveCoroutine();
        currentScenario = scenario;
        currentStepIndex = 0;
        isScenarioActive = true;
        OnScenarioStart?.Invoke(scenario);

        if (scenario.introDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(scenario.introDialogue, StartNextStep);
        else
            StartNextStep();
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

        switch (step.type)
        {
            case ScenarioStepType.Dialogue: ExecuteDialogueStep(step); break;
            case ScenarioStepType.Combat: ExecuteCombatStep(step); break;
            case ScenarioStepType.CollectItem: ExecuteCollectItemStep(step); break;
            case ScenarioStepType.GoToLocation: ExecuteLocationStep(step); break;
            case ScenarioStepType.Wait: ExecuteWaitStep(step); break;
            case ScenarioStepType.Custom: ExecuteCustomStep(step); break;
        }
    }

    public void CompleteCurrentStep()
    {
        if (!isScenarioActive || currentScenario == null) return;
        ScenarioStep step = currentScenario.steps[currentStepIndex];
        step.onStepComplete?.Invoke();
        OnStepComplete?.Invoke(step);
        currentStepIndex++;
        StartNextStep();
    }

    void CompleteScenario()
    {
        completedScenarios.Add(currentScenario.scenarioID);
        GiveScenarioRewards();

        if (currentScenario.flagsToSet != null)
        {
            foreach (var flag in currentScenario.flagsToSet)
                StoryFlags.Add(flag);
        }

        if (currentScenario.outroDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(currentScenario.outroDialogue, FinalizeScenario);
        else
            FinalizeScenario();
    }

    void FinalizeScenario()
    {
        OnScenarioComplete?.Invoke(currentScenario);
        currentScenario = null;
        currentStepIndex = 0;
        isScenarioActive = false;
        StopActiveCoroutine();
        SaveSystem.SaveGame();
    }

    void ExecuteDialogueStep(ScenarioStep step)
    {
        if (step.dialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(step.dialogue, CompleteCurrentStep);
        else
            CompleteCurrentStep();
    }

    void ExecuteCombatStep(ScenarioStep step)
    {
        if (step.enemy != null && CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatEnded += OnCombatEnded;
            CombatManager.Instance.StartCombat(step.enemy);
        }
        else CompleteCurrentStep();
    }

    void OnCombatEnded()
    {
        CombatManager.Instance.OnCombatEnded -= OnCombatEnded;
        CompleteCurrentStep();
    }

    void ExecuteCollectItemStep(ScenarioStep step)
    {
        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.GetQuantity(step.requiredItem) >= step.requiredQuantity)
        {
            InventoryManager.Instance.RemoveItem(step.requiredItem, step.requiredQuantity);
            CompleteCurrentStep();
            return;
        }

        activeCoroutine = StartCoroutine(WaitForItem(step));
    }

    IEnumerator WaitForItem(ScenarioStep step)
    {
        while (true)
        {
            if (InventoryManager.Instance != null &&
                InventoryManager.Instance.GetQuantity(step.requiredItem) >= step.requiredQuantity)
            {
                InventoryManager.Instance.RemoveItem(step.requiredItem, step.requiredQuantity);
                CompleteCurrentStep();
                yield break;
            }

            yield return WAIT_HALF_SEC;
        }
    }

    void ExecuteLocationStep(ScenarioStep step)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject target = GameObject.FindGameObjectWithTag(step.targetLocationTag);

        if (player != null && target != null)
            activeCoroutine = StartCoroutine(WaitForLocation(player, target));
        else
            CompleteCurrentStep();
    }

    IEnumerator WaitForLocation(GameObject player, GameObject target)
    {
        while (true)
        {
            if (Vector3.Distance(player.transform.position, target.transform.position) < 2f)
            {
                CompleteCurrentStep();
                yield break;
            }

            yield return WAIT_HALF_SEC;
        }
    }

    void ExecuteWaitStep(ScenarioStep step)
    {
        activeCoroutine = StartCoroutine(Wait(step.waitDuration));
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);
        CompleteCurrentStep();
    }

    void ExecuteCustomStep(ScenarioStep step)
    {
        step.onCustomStepEvent?.Invoke();
        CompleteCurrentStep();
    }

    void GiveScenarioRewards()
    {
        if (ProfileManager.Instance != null && currentScenario.experienceReward > 0)
            ProfileManager.Instance.AddExperience(currentScenario.experienceReward);

        if (currentScenario.currencyRewards != null)
        {
            foreach (var reward in currentScenario.currencyRewards)
                reward.Grant();
        }

        if (currentScenario.itemRewards != null && InventoryManager.Instance != null)
        {
            foreach (var reward in currentScenario.itemRewards)
                InventoryManager.Instance.AddItem(reward.item, reward.quantity);
        }
    }

    void StopActiveCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }

    public bool IsScenarioCompleted(string id) => completedScenarios.Contains(id);

    public HashSet<string> GetCompletedScenarios()
    {
        return new HashSet<string>(completedScenarios);
    }

    public void SetCompletedScenarios(HashSet<string> scenarios)
    {
        completedScenarios.Clear();
        if (scenarios == null) return;

        foreach (var id in scenarios)
            completedScenarios.Add(id);
    }

    public bool IsScenarioActive()
    {
        return isScenarioActive && currentScenario != null;
    }

    public ScenarioData GetCurrentScenario()
    {
        return currentScenario;
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
}
