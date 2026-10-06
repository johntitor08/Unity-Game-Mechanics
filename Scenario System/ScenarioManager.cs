using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ScenarioSaveData
{
    public List<string> completedScenarioIDs = new();
}

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;
    private static readonly WaitForSeconds WAIT_HALF_SEC = new(0.5f);
    public event Action<ScenarioData> OnScenarioStart;
    public event Action<ScenarioData> OnScenarioComplete;
    public event Action<ScenarioData> OnScenarioAborted;
    public event Action<ScenarioStep> OnStepStart;
    public event Action<ScenarioStep> OnStepComplete;
    private readonly HashSet<string> completedScenarios = new();
    private Coroutine activeCoroutine;
    private Transform cachedPlayer;
    private ScenarioData _queuedScenario;
    public bool hasStoryStarted = false;
    private bool _introPending;
    private DialogueNode _pendingStepDialogue;
    private DialogueNode _scenarioDialogueStart;
    private int _stepAfterDialogue = -1;
    private int _retreatStep = -1;
    public bool IsWaitingAfterRetreat => isScenarioActive && _retreatStep >= 0;

    [Header("Active Scenario")]
    public ScenarioData currentScenario;
    private int currentStepIndex;
    private bool isScenarioActive;

    [Header("Available Scenarios")]
    public ScenarioData[] availableScenarios;

    Transform PlayerTransform
    {
        get
        {
            if (cachedPlayer == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");

                if (go != null)
                    cachedPlayer = go.transform;
            }

            return cachedPlayer;
        }
    }

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

    public ScenarioSaveData GetSaveData()
    {
        var data = new ScenarioSaveData
        {
            completedScenarioIDs = new List<string>(completedScenarios)
        };

        return data;
    }

    public void LoadSaveData(ScenarioSaveData data)
    {
        completedScenarios.Clear();

        if (data?.completedScenarioIDs == null)
            return;

        foreach (var id in data.completedScenarioIDs)
            completedScenarios.Add(id);
    }

    public ScenarioData GetScenarioByID(string scenarioID)
    {
        if (string.IsNullOrEmpty(scenarioID) || availableScenarios == null)
            return null;

        foreach (var s in availableScenarios)
            if (s != null && s.scenarioID == scenarioID)
                return s;

        return null;
    }

    public bool CanStartScenario(ScenarioData scenario)
    {
        if (scenario == null || IsScenarioCompleted(scenario.scenarioID) || (ProfileManager.Instance != null && ProfileManager.Instance.profile.level < scenario.requiredLevel))
            return false;

        if (scenario.requiredFlags != null)
            foreach (var flag in scenario.requiredFlags)
                if (!StoryFlags.Has(flag))
                    return false;

        if (scenario.prerequisiteScenarios != null)
            foreach (var prereq in scenario.prerequisiteScenarios)
                if (!IsScenarioCompleted(prereq.scenarioID))
                    return false;

        return true;
    }

    public void StartScenario(ScenarioData scenario)
    {
        if (!CanStartScenario(scenario))
        {
            Debug.LogWarning("Scenario cannot be started.");
            return;
        }

        if (isScenarioActive)
        {
            _queuedScenario = scenario;
            return;
        }

        StopActiveCoroutine();
        _introPending = false;
        _pendingStepDialogue = null;
        currentScenario = scenario;
        currentStepIndex = 0;
        isScenarioActive = true;
        hasStoryStarted = true;
        _retreatStep = -1;
        OnScenarioStart?.Invoke(scenario);

        if (scenario.introDialogue != null && DialogueManager.Instance != null)
        {
            _introPending = true;
            activeCoroutine = StartCoroutine(StartIntroWhenIdle(scenario));
        }
        else
            StartNextStep();
    }

    public DialogueNode PeekFollowUpDialogue(DialogueNode endedDialogueStart)
    {
        if (!isScenarioActive || currentScenario == null)
            return null;

        if (_introPending)
            return currentScenario.introDialogue;

        if (_pendingStepDialogue != null)
            return _pendingStepDialogue;

        if (endedDialogueStart == null || endedDialogueStart != _scenarioDialogueStart || currentScenario.steps == null)
            return null;

        int next = _stepAfterDialogue;

        while (next >= 0 && next < currentScenario.steps.Length && currentScenario.steps[next] != null && !currentScenario.steps[next].Applies())
            next++;

        if (next >= 0 && next < currentScenario.steps.Length)
            return currentScenario.steps[next].type == ScenarioStepType.Dialogue ? currentScenario.steps[next].dialogue : null;

        return next == currentScenario.steps.Length ? currentScenario.outroDialogue : null;
    }

    IEnumerator StartIntroWhenIdle(ScenarioData scenario)
    {
        yield return WaitForDialogueSlot();

        _introPending = false;

        if (!isScenarioActive || currentScenario != scenario || DialogueManager.Instance == null)
            yield break;

        _scenarioDialogueStart = scenario.introDialogue;
        _stepAfterDialogue = currentStepIndex;
        DialogueManager.Instance.StartDialogue(scenario.introDialogue, StartNextStep);
    }

    public int ResolveStepIndex(string scenarioID, int savedIndex, string savedStepName)
    {
        var scenario = GetScenarioByID(scenarioID);

        if (scenario == null || scenario.steps == null || string.IsNullOrEmpty(savedStepName))
            return savedIndex;

        for (int i = 0; i < scenario.steps.Length; i++)
            if (scenario.steps[i] != null && scenario.steps[i].stepName == savedStepName)
                return i;

        return savedIndex;
    }

    public void ResumeScenario(string scenarioID, int stepIndex, bool waitForSquare = false)
    {
        var scenario = GetScenarioByID(scenarioID);

        if (scenario == null)
        {
            Debug.LogWarning($"[ScenarioManager] ResumeScenario: scenario '{scenarioID}' not found.");
            return;
        }

        StopActiveCoroutine();
        currentScenario = scenario;
        int stepCount = scenario.steps != null ? scenario.steps.Length : 0;
        currentStepIndex = Mathf.Clamp(stepIndex, 0, stepCount);
        isScenarioActive = true;
        _retreatStep = -1;
        hasStoryStarted = true;
        OnScenarioStart?.Invoke(scenario);

        if (waitForSquare)
        {
            _retreatStep = currentStepIndex;

            if (SceneEvent.Instance != null)
            {
                SceneEvent.Instance.ShowHudPanels();
                SceneEvent.Instance.ShowForegroundMessage(Loc.T("The fight you ran from is waiting for you in the village square.", "Kaçtığın dövüş seni köy meydanında bekliyor."), 4f);
            }

            return;
        }

        StartNextStep();
    }

    void StartNextStep()
    {
        if (!isScenarioActive || currentScenario == null)
            return;

        while (currentStepIndex < currentScenario.steps.Length && currentScenario.steps[currentStepIndex] != null && !currentScenario.steps[currentStepIndex].Applies())
            currentStepIndex++;

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
                ExecuteCustomStep(step);
                break;
        }
    }

    public void CompleteCurrentStep()
    {
        if (!isScenarioActive || currentScenario == null)
            return;

        ScenarioStep step = currentScenario.steps[currentStepIndex];
        step.onStepComplete?.Invoke();
        OnStepComplete?.Invoke(step);
        currentStepIndex++;
        StartNextStep();
    }

    void ExecuteDialogueStep(ScenarioStep step)
    {
        if (step.dialogue != null && DialogueManager.Instance != null)
        {
            _pendingStepDialogue = step.dialogue;
            activeCoroutine = StartCoroutine(StartStepDialogueWhenIdle(step, currentStepIndex));
        }
        else
            CompleteCurrentStep();
    }

    static IEnumerator WaitForDialogueSlot()
    {
        float t = 0f;

        while (DialogueManager.Instance != null && (DialogueManager.Instance.IsInDialogue() || (DialogueManager.Instance.IsSceneTransitionBusy() && t < 3f)))
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator StartStepDialogueWhenIdle(ScenarioStep step, int stepIndex)
    {
        yield return WaitForDialogueSlot();
        _pendingStepDialogue = null;

        if (!isScenarioActive || currentStepIndex != stepIndex || DialogueManager.Instance == null)
            yield break;

        _scenarioDialogueStart = step.dialogue;
        _stepAfterDialogue = stepIndex + 1;
        DialogueManager.Instance.StartDialogue(step.dialogue, CompleteCurrentStep);
    }

    void ExecuteCombatStep(ScenarioStep step)
    {
        if (step.enemy != null && CombatManager.Instance != null)
        {
            SubscribeCombatHandlers();
            CombatManager.Instance.StartCombat(step.enemy);
        }
        else
        {
            CompleteCurrentStep();
        }
    }

    void SubscribeCombatHandlers()
    {
        var cm = CombatManager.Instance;

        if (cm == null)
            return;

        UnsubscribeCombatHandlers();
        cm.OnCombatVictory += OnCombatVictory;
        cm.OnCombatDefeat += OnCombatDefeat;
        cm.OnCombatFled += OnCombatFled;
    }

    void UnsubscribeCombatHandlers()
    {
        var cm = CombatManager.Instance;

        if (cm == null)
            return;

        cm.OnCombatVictory -= OnCombatVictory;
        cm.OnCombatDefeat -= OnCombatDefeat;
        cm.OnCombatFled -= OnCombatFled;
    }

    void OnCombatVictory(EnemyData defeated)
    {
        UnsubscribeCombatHandlers();
        activeCoroutine = StartCoroutine(CompleteAfterCombatCloses(currentStepIndex));
    }

    IEnumerator CompleteAfterCombatCloses(int stepIndex)
    {
        var cm = CombatManager.Instance;

        while (cm != null && cm.inCombat)
            yield return null;

        var ui = CombatUI.Instance;

        while (ui != null && ui.combatPanel != null && ui.combatPanel.activeSelf)
            yield return null;

        if (isScenarioActive && currentStepIndex == stepIndex)
            CompleteCurrentStep();
    }

    void OnCombatDefeat()
    {
        UnsubscribeCombatHandlers();

        if (currentScenario != null && !string.IsNullOrEmpty(currentScenario.retryFlagOnDefeat))
        {
            Debug.Log($"[ScenarioManager] Lost the fight in side scenario '{currentScenario.scenarioID}'. Back to town; ask again to retry.");
            activeCoroutine = StartCoroutine(EndSideScenarioAfterLostCombat(currentScenario));
            return;
        }

        if (currentScenario != null && currentScenario.scenarioID != null && currentScenario.scenarioID.StartsWith("ashenveil_day") && SceneEvent.Instance != null)
        {
            Debug.Log($"[ScenarioManager] Lost the fight in day scenario '{currentScenario.scenarioID}'. Waking up in bed; the day starts over.");
            activeCoroutine = StartCoroutine(WakeUpAfterLostDayFight());
            return;
        }

        Debug.Log($"[ScenarioManager] Combat defeat during scenario '{(currentScenario != null ? currentScenario.scenarioID : null)}'. Retrying the combat.");
        activeCoroutine = StartCoroutine(RetryCombatAfterItCloses());
    }

    void OnCombatFled()
    {
        UnsubscribeCombatHandlers();

        if (currentScenario != null && !string.IsNullOrEmpty(currentScenario.retryFlagOnDefeat))
        {
            Debug.Log($"[ScenarioManager] Fled the fight in side scenario '{currentScenario.scenarioID}'. Back to town; ask again to retry.");
            activeCoroutine = StartCoroutine(EndSideScenarioAfterLostCombat(currentScenario));
            return;
        }

        Debug.Log($"[ScenarioManager] Fled the fight in '{(currentScenario != null ? currentScenario.scenarioID : null)}'. The story waits at the scene before it.");
        activeCoroutine = StartCoroutine(RetreatAfterCombatCloses());
    }

    IEnumerator RetreatAfterCombatCloses()
    {
        var cm = CombatManager.Instance;

        while (cm != null && cm.inCombat)
            yield return null;

        var ui = CombatUI.Instance;

        while (ui != null && ui.combatPanel != null && ui.combatPanel.activeSelf)
            yield return null;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        activeCoroutine = null;

        if (!isScenarioActive || currentScenario == null)
            yield break;

        _retreatStep = LeadInStep(currentStepIndex);
        currentStepIndex = _retreatStep;

        if (SceneEvent.Instance != null)
        {
            SceneEvent.Instance.ShowHudPanels();
            SceneEvent.Instance.ShowForegroundMessage(Loc.T("You got away. Rest, change your gear or shop, then come back to the village square from the map to face it again.", "Kaçmayı başardın. Dinlen, ekipmanını değiştir ya da alışveriş yap; hazır olunca haritadan köy meydanına dönüp yeniden yüzleş."), 5f);
        }

        SaveSystem.SaveGame();
    }

    int LeadInStep(int fightStep)
    {
        for (int i = fightStep - 1; i >= 0; i--)
        {
            var step = currentScenario.steps[i];

            if (step == null || !step.Applies())
                continue;

            return step.type == ScenarioStepType.Dialogue ? i : fightStep;
        }

        return fightStep;
    }

    public void ResumeAfterRetreat()
    {
        if (!IsWaitingAfterRetreat)
            return;

        _retreatStep = -1;
        StartNextStep();
    }

    IEnumerator EndSideScenarioAfterLostCombat(ScenarioData scenario)
    {
        var cm = CombatManager.Instance;

        while (cm != null && cm.inCombat)
            yield return null;

        var ui = CombatUI.Instance;

        while (ui != null && ui.combatPanel != null && ui.combatPanel.activeSelf)
            yield return null;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        activeCoroutine = null;
        AbortScenario();
        StoryFlags.Remove(scenario.retryFlagOnDefeat);

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.ShowHudPanels();

        SaveSystem.SaveGame();
    }

    IEnumerator WakeUpAfterLostDayFight()
    {
        var cm = CombatManager.Instance;

        while (cm != null && cm.inCombat)
            yield return null;

        var ui = CombatUI.Instance;

        while (ui != null && ui.combatPanel != null && ui.combatPanel.activeSelf)
            yield return null;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        activeCoroutine = null;
        AbortScenario();

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.WakeUpInBedToRetryDay();
    }

    IEnumerator RetryCombatAfterItCloses()
    {
        var cm = CombatManager.Instance;

        while (cm != null && cm.inCombat)
            yield return null;

        var ui = CombatUI.Instance;

        while (ui != null && ui.combatPanel != null && ui.combatPanel.activeSelf)
            yield return null;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        if (isScenarioActive && currentScenario != null)
            StartNextStep();
    }

    void ExecuteCollectItemStep(ScenarioStep step)
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.GetQuantity(step.requiredItem) >= step.requiredQuantity)
        {
            InventoryManager.Instance.RemoveItem(step.requiredItem, step.requiredQuantity);
            CompleteCurrentStep();
            return;
        }

        activeCoroutine = StartCoroutine(WaitForItem(step, currentStepIndex));
    }

    IEnumerator WaitForItem(ScenarioStep step, int stepIndex)
    {
        while (isScenarioActive && currentStepIndex == stepIndex)
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.GetQuantity(step.requiredItem) >= step.requiredQuantity)
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
        GameObject target = GameObject.FindGameObjectWithTag(step.targetLocationTag);

        if (PlayerTransform != null && target != null)
            activeCoroutine = StartCoroutine(WaitForLocation(target.transform, currentStepIndex));
        else
            CompleteCurrentStep();
    }

    IEnumerator WaitForLocation(Transform target, int stepIndex)
    {
        while (isScenarioActive && currentStepIndex == stepIndex)
        {
            if (PlayerTransform != null && Vector3.Distance(PlayerTransform.position, target.position) < 2f)
            {
                CompleteCurrentStep();
                yield break;
            }

            yield return WAIT_HALF_SEC;
        }
    }

    void ExecuteWaitStep(ScenarioStep step)
    {
        activeCoroutine = StartCoroutine(Wait(step.waitDuration, currentStepIndex));
    }

    IEnumerator Wait(float duration, int stepIndex)
    {
        yield return new WaitForSeconds(duration);

        if (isScenarioActive && currentStepIndex == stepIndex)
            CompleteCurrentStep();
    }

    void ExecuteCustomStep(ScenarioStep step)
    {
        step.onCustomStepEvent?.Invoke();
        CompleteCurrentStep();
    }

    void CompleteScenario()
    {
        completedScenarios.Add(currentScenario.scenarioID);
        GiveScenarioRewards();

        if (currentScenario.flagsToSet != null)
            foreach (var flag in currentScenario.flagsToSet)
                StoryFlags.Add(flag);

        if (currentScenario.outroDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(currentScenario.outroDialogue, FinalizeScenario);
        else
            FinalizeScenario();
    }

    public void FailScenario()
    {
        if (!isScenarioActive || !currentScenario.canFail)
            return;

        StopActiveCoroutine();

        if (currentScenario.failureDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(currentScenario.failureDialogue, FinalizeScenario);
        else
            FinalizeScenario();
    }

    void FinalizeScenario()
    {
        _retreatStep = -1;
        UnsubscribeCombatHandlers();
        OnScenarioComplete?.Invoke(currentScenario);
        currentScenario = null;
        currentStepIndex = 0;
        isScenarioActive = false;
        StopActiveCoroutine();
        SaveSystem.SaveGame();
        TryStartQueuedScenario();
    }

    void TryStartQueuedScenario()
    {
        if (_queuedScenario == null)
            return;

        var next = _queuedScenario;
        _queuedScenario = null;

        if (CanStartScenario(next))
            StartScenario(next);
    }

    void AbortScenario()
    {
        if (!isScenarioActive)
            return;

        StopActiveCoroutine();
        UnsubscribeCombatHandlers();
        _introPending = false;
        _pendingStepDialogue = null;
        _retreatStep = -1;
        var aborted = currentScenario;
        currentScenario = null;
        currentStepIndex = 0;
        isScenarioActive = false;
        OnScenarioAborted?.Invoke(aborted);
    }

    void GiveScenarioRewards()
    {
        if (ProfileManager.Instance != null && currentScenario.experienceReward > 0)
            ProfileManager.Instance.AddExperience(currentScenario.experienceReward);

        if (currentScenario.currencyRewards != null)
            foreach (var reward in currentScenario.currencyRewards)
                reward.Grant();

        if (currentScenario.itemRewards != null && InventoryManager.Instance != null)
            foreach (var reward in currentScenario.itemRewards)
                InventoryManager.Instance.AddItem(reward.item, reward.quantity);
    }

    void StopActiveCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        _pendingStepDialogue = null;
    }

    public bool IsScenarioCompleted(string id) => completedScenarios.Contains(id);

    public HashSet<string> GetCompletedScenarios() => new(completedScenarios);

    public bool IsScenarioActive() => isScenarioActive && currentScenario != null;

    public ScenarioData GetCurrentScenario() => currentScenario;

    public int GetCurrentStepIndex() => currentStepIndex;

    public bool IsPlayingSequence()
    {
        if (!isScenarioActive || currentScenario == null || currentScenario.steps == null || currentStepIndex >= currentScenario.steps.Length)
            return false;

        return currentScenario.steps[currentStepIndex].type == ScenarioStepType.Wait;
    }

    public void SetCompletedScenarios(HashSet<string> scenarios)
    {
        completedScenarios.Clear();

        if (scenarios == null)
            return;

        foreach (var id in scenarios) completedScenarios.Add(id);
    }
}
