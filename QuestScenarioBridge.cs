using UnityEngine;

public class QuestScenarioBridge : MonoBehaviour
{
    [System.Serializable]
    public struct BranchMap
    {
        public string startFlag;
        public string scenarioID;
        public string questID;
    }

    [Tooltip("Origin start flag -> ScenarioData.scenarioID -> quest id.")]
    public BranchMap[] branches;

    private ScenarioManager _sm;

    void Start() => TryHook();

    void OnEnable()
    {
        StoryFlags.OnFlagAdded += OnFlagAdded;
        TryHook();
    }

    void OnDisable()
    {
        StoryFlags.OnFlagAdded -= OnFlagAdded;

        if (_sm != null)
        {
            _sm.OnStepComplete -= OnStepComplete;
            _sm.OnScenarioComplete -= OnScenarioComplete;
            _sm = null;
        }
    }

    void TryHook()
    {
        if (_sm == null && ScenarioManager.Instance != null)
        {
            _sm = ScenarioManager.Instance;
            _sm.OnStepComplete += OnStepComplete;
            _sm.OnScenarioComplete += OnScenarioComplete;
        }
    }

    void OnFlagAdded(string flag)
    {
        if (branches == null)
            return;

        foreach (var b in branches)
            if (b.startFlag == flag)
            {
                StartBranch(b.scenarioID);
                return;
            }
    }

    void StartBranch(string scenarioID)
    {
        TryHook();
        var data = FindScenario(scenarioID);

        if (data != null && ScenarioManager.Instance != null && ScenarioManager.Instance.CanStartScenario(data))
        {
            ScenarioManager.Instance.StartScenario(data);
            StartQuestForScenario(scenarioID);
        }
    }

    void StartQuestForScenario(string scenarioID)
    {
        string questID = QuestForScenario(scenarioID);

        if (string.IsNullOrEmpty(questID) || QuestManager.Instance == null)
            return;

        var quest = FindQuest(questID);

        if (quest != null && QuestManager.Instance.CanStartQuest(quest))
            QuestManager.Instance.StartQuest(quest);
    }

    QuestData FindQuest(string questID)
    {
        var qm = QuestManager.Instance;

        if (qm == null || qm.allQuests == null)
            return null;

        foreach (var q in qm.allQuests)
            if (q != null && q.questID == questID)
                return q;

        return null;
    }

    void OnStepComplete(ScenarioStep step)
    {
        if (step == null || string.IsNullOrEmpty(step.stepName) || step.stepName == "complete" || QuestManager.Instance == null)
            return;

        var current = ScenarioManager.Instance != null ? ScenarioManager.Instance.GetCurrentScenario() : null;

        if (current == null)
            return;

        string questID = QuestForScenario(current.scenarioID);

        if (!string.IsNullOrEmpty(questID))
            QuestManager.Instance.UpdateObjectiveProgress(questID, step.stepName, 1);
    }

    void OnScenarioComplete(ScenarioData scenario) { }

    ScenarioData FindScenario(string scenarioID)
    {
        var sm = ScenarioManager.Instance;

        if (sm == null || sm.availableScenarios == null)
            return null;

        foreach (var s in sm.availableScenarios)
            if (s != null && s.scenarioID == scenarioID)
                return s;

        return null;
    }

    string QuestForScenario(string scenarioID)
    {
        if (branches == null)
            return null;

        foreach (var b in branches)
            if (b.scenarioID == scenarioID)
                return b.questID;

        return null;
    }
}
