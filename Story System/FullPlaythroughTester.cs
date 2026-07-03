using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FullPlaythroughTester : MonoBehaviour
{
    [Header("Run")]
    public bool runOnPlay = false;
    public KeyCode hotkey = KeyCode.F10;
    public float delayBetweenQuests = 0.1f;

    [Header("Scope")]
    public bool runQuests = true;
    public bool runScenario = true;

    QuestManager QM => QuestManager.Instance;

    void Start()
    {
        if (runOnPlay)
            StartCoroutine(RunEverythingRoutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(hotkey))
            StartCoroutine(RunEverythingRoutine());
    }

    [ContextMenu("Run ALL quests")]
    public void RunAllQuestsMenu() => StartCoroutine(RunEverythingRoutine(scenarioOverride: false));

    [ContextMenu("Run ALL quests + scenario")]
    public void RunEverythingMenu() => StartCoroutine(RunEverythingRoutine(scenarioOverride: true));

    [ContextMenu("Log · Status of every quest")]
    public void LogStatus()
    {
        if (QM == null || QM.allQuests == null)
        {
            Debug.LogWarning("[FullTest] QuestManager/allQuests null.");
            return;
        }

        var sb = new StringBuilder("=== Quest status ===\n");

        foreach (var q in QM.allQuests)
        {
            if (q == null)
                continue;

            string s = QM.IsQuestCompleted(q.questID) ? "DONE" : QM.IsQuestActive(q.questID) ? "ACTIVE" : "—";
            sb.AppendLine($"[{s,-6}] {q.questID}  ({q.questName})");
        }

        Debug.Log(sb.ToString());
    }

    IEnumerator RunEverythingRoutine(bool? scenarioOverride = null)
    {
        if (QM == null || QM.allQuests == null)
        {
            Debug.LogError("[FullTest] QuestManager not ready. Is it in the scene and initialized?");
            yield break;
        }

        int pass = 0, fail = 0;
        var report = new StringBuilder("========== FULL PLAYTHROUGH TEST ==========\n");

        if (runQuests)
        {
            var quests = new List<QuestData>(QM.allQuests);

            foreach (var quest in quests)
            {
                if (quest == null) continue;

                if (QM.IsQuestCompleted(quest.questID))
                {
                    report.AppendLine($"• {quest.questID}: already DONE");
                    pass++;
                    continue;
                }

                bool ok = RunSingleQuest(quest, report);

                if (ok)
                    pass++;
                else
                    fail++;

                if (delayBetweenQuests > 0f)
                    yield return new WaitForSeconds(delayBetweenQuests);
            }
        }

        bool doScenario = scenarioOverride ?? runScenario;

        if (doScenario)
            TryRunScenario(report);

        report.AppendLine($"========== DONE — pass:{pass} fail:{fail} ==========");

        if (fail == 0)
            Debug.Log($"<color=#00d4a0>{report}</color>");

        else Debug.LogWarning(report.ToString());
    }

    bool RunSingleQuest(QuestData quest, StringBuilder report)
    {
        string id = quest.questID;

        if (quest.requiredFlags != null)
            foreach (var f in quest.requiredFlags)
                if (!string.IsNullOrEmpty(f)) StoryFlags.Add(f);

        if (!QM.IsQuestActive(id))
        {
            bool started = QM.StartQuest(quest);

            if (!started && !QM.IsQuestActive(id))
            {
                if (quest.prerequisiteQuests != null)
                    foreach (var pre in quest.prerequisiteQuests)
                        if (pre != null && pre.flagsToSetOnComplete != null)
                            foreach (var f in pre.flagsToSetOnComplete)
                                if (!string.IsNullOrEmpty(f)) StoryFlags.Add(f);

                started = QM.StartQuest(quest);
            }

            if (!started && !QM.IsQuestActive(id))
            {
                report.AppendLine($"✗ {id}: could NOT start");
                return false;
            }
        }

        if (quest.objectives != null)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj == null)
                    continue;

                var state = QM.GetObjectiveState(id, obj.objectiveID);
                int remaining = obj.GetRequiredCount() - state.currentProgress;

                if (remaining <= 0)
                    continue;

                CompleteObjective(id, obj, remaining);
            }
        }

        var active = QM.GetActiveQuest(id);

        if (active != null)
            QM.CompleteQuest(active);

        bool done = QM.IsQuestCompleted(id);
        report.AppendLine(done ? $"✓ {id}: completed" : $"✗ {id}: objectives done but not marked complete");
        return done;
    }

    void CompleteObjective(string questID, QuestObjective obj, int amount)
    {
        switch (obj.type)
        {
            case QuestObjectiveType.TalkToNPC:
                QM.NotifyTalkToNPC(Tag(obj.npcTag, obj), amount);
                break;

            case QuestObjectiveType.InteractWithObject:
                QM.NotifyObjectInteracted(Tag(obj.interactObjectTag, obj), amount);
                break;

            case QuestObjectiveType.GoToLocation:
                QM.NotifyLocationReached(Tag(obj.locationTag, obj), amount);
                break;

            default:
                QM.UpdateObjectiveProgress(questID, obj.objectiveID, amount);
                break;
        }
    }

    static string Tag(string tag, QuestObjective obj) => string.IsNullOrEmpty(tag) ? obj.objectiveID : tag;

    void TryRunScenario(StringBuilder report)
    {
        var scenarioMgr = FindAnyObjectByType<ScenarioManager>();

        if (scenarioMgr == null)
        {
            report.AppendLine("• scenario: no ScenarioManager in scene (skipped)");
            return;
        }

        var t = scenarioMgr.GetType();
        string[] starters = { "StartScenario", "BeginScenario", "TryStartDayScenario", "StartDayScenario", "AdvanceScenario", "Begin" };

        foreach (var name in starters)
        {
            var m = t.GetMethod(name, System.Type.EmptyTypes);

            if (m != null)
            {
                m.Invoke(scenarioMgr, null);
                report.AppendLine($"• scenario: invoked ScenarioManager.{name}()");
                return;
            }
        }

        report.AppendLine("• scenario: ScenarioManager found but no no-arg start method (drive it via ScenarioDebug)");
    }
}
