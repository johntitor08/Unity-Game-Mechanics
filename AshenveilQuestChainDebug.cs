using UnityEngine;
using System.Text;

public class AshenveilQuestChainDebug : MonoBehaviour
{
    public string questID = AshenveilQuestIds.Q01;
    QuestManager QM => QuestManager.Instance;

    [Header("Prerequisites (optional)")]
    public bool setRequiredFlagsBeforeStart = true;

    [Header("Output")]
    public bool logToConsole = true;

    [ContextMenu("Log · Active quest state")]
    public void LogQuestState() => Log(BuildQuestState(questID));

    [ContextMenu("Log · Trigger setup catalog (q01–q11)")]
    public void LogTriggerCatalog() => Log(AshenveilQuestTriggerCatalog.FormatSetupReport());

    [ContextMenu("Log · All q01–q11 status")]
    public void LogAllQuestsStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Ashenveil q01–q11 ===");

        foreach (var id in AshenveilQuestIds.MainChain)
            sb.AppendLine(BuildQuestState(id, compact: true));

        Log(sb.ToString());
    }

    [ContextMenu("Start · Selected quest")]
    public void StartSelectedQuest() => StartQuest(questID);

    [ContextMenu("Advance · Next incomplete objective")]
    public void AdvanceNextObjective() => AdvanceNext(questID);

    [ContextMenu("Complete · All objectives (selected)")]
    public void CompleteAllObjectives() => CompleteAllObjectivesFor(questID);

    [ContextMenu("Complete · Selected quest (turn-in)")]
    public void CompleteSelectedQuest() => CompleteQuest(questID);

    [ContextMenu("Abandon · Selected quest")]
    public void AbandonSelectedQuest()
    {
        var quest = GetQuest(questID);

        if (quest == null)
            return;

        QM.AbandonQuest(quest);
        Log($"Abandoned {questID}");
    }

    [ContextMenu("Q01 · Start")]
    public void Q01_Start() => StartQuest(AshenveilQuestIds.Q01);

    [ContextMenu("Q01 · Complete all objectives")]
    public void Q01_CompleteObjectives() => CompleteAllObjectivesFor(AshenveilQuestIds.Q01);

    [ContextMenu("Q02 · Start")]
    public void Q02_Start() => StartQuest(AshenveilQuestIds.Q02);

    [ContextMenu("Q03 · Start")]
    public void Q03_Start() => StartQuest(AshenveilQuestIds.Q03);

    [ContextMenu("Q04 · Start")]
    public void Q04_Start() => StartQuest(AshenveilQuestIds.Q04);

    [ContextMenu("Q05 · Start")]
    public void Q05_Start() => StartQuest(AshenveilQuestIds.Q05);

    [ContextMenu("Q11 · Start (requires VossDefeatedClean)")]
    public void Q11_Start() => StartQuest(AshenveilQuestIds.Q11);

    [ContextMenu("Chain · Set flags through Q03 (skip to q04)")]
    public void Chain_SkipToQ04()
    {
        StoryFlags.Add(QuestFlags.MarenTeaServed);
        StoryFlags.Add(QuestFlags.StillSmokeDone);
        StoryFlags.Add(QuestFlags.MissingContractFound);
        Log("Flags set for q04 prerequisites.");
    }

    [ContextMenu("Chain · Set flags through Q09 (skip to q10)")]
    public void Chain_SkipToQ10()
    {
        Chain_SkipToQ04();
        StoryFlags.Add(QuestFlags.ElderTruthKnown);
        StoryFlags.Add(QuestFlags.VossContractPlayerAware);
        StoryFlags.Add(QuestFlags.FirstPrisonerRescued);
        StoryFlags.Add(QuestFlags.Q09VossWarehouseFound);
        Log("Flags set for q10 prerequisites.");
    }

    void StartQuest(string id)
    {
        if (QM == null)
        {
            LogError("QuestManager missing.");
            return;
        }

        if (setRequiredFlagsBeforeStart)
            ApplyPrerequisiteFlags(id);

        var quest = GetQuest(id);

        if (quest == null)
        {
            LogError($"Quest '{id}' not in QuestManager.allQuests. Is AshenveilQuestFactory in scene?");
            return;
        }

        if (QM.StartQuest(quest))
            Log($"Started {id}");
        else
            LogWarning($"Could not start {id} (already active, completed, or requirements missing).");
    }

    void AdvanceNext(string id)
    {
        var quest = GetQuest(id);

        if (quest == null || QM == null || !QM.IsQuestActive(id))
        {
            LogWarning($"{id} is not active.");
            return;
        }

        foreach (var objective in quest.objectives)
        {
            if (objective == null)
                continue;

            var state = QM.GetObjectiveState(id, objective.objectiveID);

            if (state.isCompleted)
                continue;

            int required = objective.GetRequiredCount();
            int remaining = required - state.currentProgress;

            if (remaining <= 0)
                continue;

            if (IsNotifyObjective(objective))
                AdvanceNotifyObjective(objective, remaining);
            else
                QM.UpdateObjectiveProgress(id, objective.objectiveID, remaining);

            Log($"Advanced {id} / {objective.objectiveID} (+{remaining})");
            return;
        }

        LogWarning($"{id}: no incomplete objectives.");
    }

    void CompleteAllObjectivesFor(string id)
    {
        var quest = GetQuest(id);

        if (quest == null || QM == null)
            return;

        if (!QM.IsQuestActive(id))
        {
            LogWarning($"{id} not active — start it first.");
            return;
        }

        foreach (var objective in quest.objectives)
        {
            if (objective == null)
                continue;

            var state = QM.GetObjectiveState(id, objective.objectiveID);

            if (state.isCompleted)
                continue;

            int required = objective.GetRequiredCount();
            int remaining = required - state.currentProgress;

            if (remaining <= 0)
                continue;

            if (IsNotifyObjective(objective))
                AdvanceNotifyObjective(objective, remaining);
            else
                QM.UpdateObjectiveProgress(id, objective.objectiveID, remaining);
        }

        Log($"All objectives completed for {id} (quest may auto-complete if enabled).");
    }

    void CompleteQuest(string id)
    {
        var quest = QM != null ? QM.GetActiveQuest(id) : null;

        if (quest == null)
        {
            LogWarning($"{id} is not active.");
            return;
        }

        CompleteAllObjectivesFor(id);
        quest = QM.GetActiveQuest(id);

        if (quest != null)
        {
            QM.CompleteQuest(quest);
            Log($"Completed quest {id}");
        }
    }

    static bool IsNotifyObjective(QuestObjective objective) => objective.type == QuestObjectiveType.TalkToNPC || objective.type == QuestObjectiveType.InteractWithObject || objective.type == QuestObjectiveType.GoToLocation;

    static void AdvanceNotifyObjective(QuestObjective objective, int amount)
    {
        string tag = objective.type switch
        {
            QuestObjectiveType.TalkToNPC => objective.npcTag,
            QuestObjectiveType.InteractWithObject => objective.interactObjectTag,
            QuestObjectiveType.GoToLocation => objective.locationTag,
            _ => objective.objectiveID
        };

        if (string.IsNullOrEmpty(tag))
            tag = objective.objectiveID;

        switch (objective.type)
        {
            case QuestObjectiveType.TalkToNPC:
                QuestManager.Instance.NotifyTalkToNPC(tag, amount);
                break;

            case QuestObjectiveType.InteractWithObject:
                QuestManager.Instance.NotifyObjectInteracted(tag, amount);
                break;

            case QuestObjectiveType.GoToLocation:
                QuestManager.Instance.NotifyLocationReached(tag, amount);
                break;
        }
    }

    static void ApplyPrerequisiteFlags(string questId)
    {
        switch (questId)
        {
            case AshenveilQuestIds.Q04:
                StoryFlags.Add(QuestFlags.StillSmokeDone);
                break;

            case AshenveilQuestIds.Q05:
            case AshenveilQuestIds.Q09:
                StoryFlags.Add(QuestFlags.ElderTruthKnown);
                break;

            case AshenveilQuestIds.Q06:
                StoryFlags.Add(QuestFlags.FirstPrisonerRescued);
                break;

            case AshenveilQuestIds.Q07:
                StoryFlags.Add(QuestFlags.MissingContractFound);
                break;

            case AshenveilQuestIds.Q08:
                StoryFlags.Add(QuestFlags.VossContractPlayerAware);
                break;

            case AshenveilQuestIds.Q10:
                StoryFlags.Add(QuestFlags.ElderTruthKnown);
                StoryFlags.Add(QuestFlags.Q09VossWarehouseFound);
                break;

            case AshenveilQuestIds.Q11:
                StoryFlags.Add(QuestFlags.VossDefeatedClean);
                break;
        }
    }

    QuestData GetQuest(string id)
    {
        if (QM == null || QM.allQuests == null)
            return null;

        foreach (var q in QM.allQuests)
            if (q != null && q.questID == id)
                return q;

        return null;
    }

    string BuildQuestState(string id, bool compact = false)
    {
        if (QM == null)
            return $"{id}: QuestManager null";

        var quest = GetQuest(id);
        bool active = QM.IsQuestActive(id);
        bool done = QM.IsQuestCompleted(id);

        if (quest == null)
            return $"{id}: (not registered)";

        if (!active && !done)
            return compact ? $"{id}: —" : $"{id}: inactive";

        if (done && compact)
            return $"{id}: DONE";

        var sb = new StringBuilder();
        sb.Append(compact ? $"{id}: " : $"--- {id} ({quest.questName}) ---\n");

        if (done)
            sb.Append(compact ? "DONE " : "Status: COMPLETED\n");
        else if (active)
            sb.Append(compact ? "ACTIVE " : "Status: ACTIVE\n");

        if (quest.objectives == null)
            return sb.ToString();

        foreach (var obj in quest.objectives)
        {
            if (obj == null)
                continue;

            var state = QM.GetObjectiveState(id, obj.objectiveID);
            string mark = state.isCompleted ? "✓" : " ";

            string tag = obj.type switch
            {
                QuestObjectiveType.TalkToNPC => $"npc={obj.npcTag}",
                QuestObjectiveType.InteractWithObject => $"interact={obj.interactObjectTag}",
                QuestObjectiveType.GoToLocation => $"loc={obj.locationTag}",
                _ => obj.type.ToString()
            };

            sb.Append(compact ? $"{mark}{obj.objectiveID} " : $"  [{mark}] {obj.objectiveID} {state.currentProgress}/{obj.GetRequiredCount()} ({tag}) — {obj.description}\n");
        }

        return sb.ToString().TrimEnd();
    }

    void Log(string msg)
    {
        if (logToConsole)
            Debug.Log($"<color=#7ec8e3>[AshenveilQuestDebug]</color> {msg}");
    }

    void LogWarning(string msg) => Debug.LogWarning($"[AshenveilQuestDebug] {msg}");

    void LogError(string msg) => Debug.LogError($"[AshenveilQuestDebug] {msg}");
}

public static class AshenveilQuestIds
{
    public const string Q01 = "q01_bir_fincan_huzur";
    public const string Q02 = "q02_hala_duman";
    public const string Q03 = "q03_kirik_muhur";
    public const string Q04 = "q04_yasli_adamin_itirafi";
    public const string Q05 = "q05_acik_el";
    public const string Q06 = "q06_tarif_defteri";
    public const string Q07 = "q07_corvinin_borcu";
    public const string Q08 = "q08_defterdeki_sesler";
    public const string Q09 = "q09_bekleyis";
    public const string Q10 = "q10_acik_hesap";
    public const string Q11 = "q11_fincan_basinda";

    public static readonly string[] MainChain =
    {
        Q01, Q02, Q03, Q04, Q05, Q06, Q07, Q08, Q09, Q10, Q11
    };
}
