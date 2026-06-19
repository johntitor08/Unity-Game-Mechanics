using UnityEngine;

public class BoundArchivistQuestController : MonoBehaviour
{
    private static bool IsBoundArchivist => StoryFlags.Has(QuestFlags.BoundArchivistStart);

    public void OnOpeningSceneComplete()
    {
        if (StoryFlags.Has(QuestFlags.BoundArchivistOpeningComplete))
            return;

        StoryFlags.Add(QuestFlags.BoundArchivistOpeningComplete);
        StoryFlags.Add(QuestFlags.BoundArchivistStart);
        StoryFlags.Add(QuestFlags.BrahmaLeft);
        StoryFlags.Add(QuestFlags.SealedFileTaken);
        TryAutoStartQuest(QuestIds.Q_BA01);
        Debug.Log("[BoundArchivistQuest] Scene 1.1 flags set.");
    }

    public void OnMarenGateMet()
    {
        if (!IsBoundArchivist)
            return;

        StoryFlags.Add(QuestFlags.MarenMetBoundArchivist);
        StoryFlags.Add(QuestFlags.AshenveilEntered);
        UpdateObjective(QuestIds.Q_BA01, "obj_speak_maren_gate");
        Debug.Log("[BoundArchivistQuest] Maren met at gate.");
    }

    public void OnMarenKitchenComplete()
    {
        if (!IsBoundArchivist)
            return;

        StoryFlags.Add(QuestFlags.MarenDialogueDone);
        StoryFlags.Add(QuestFlags.ReversalClauseKnown);
        StoryFlags.Add(QuestFlags.ChurchRuinsClue);
        UpdateObjective(QuestIds.Q_BA01, "obj_learn_maren_knowledge");
        Debug.Log("[BoundArchivistQuest] Maren kitchen dialogue done.");
    }

    public void OnElisMet()
    {
        if (!IsBoundArchivist)
            return;

        StoryFlags.Add(QuestFlags.ElisMet);
        StoryFlags.Add(QuestFlags.ThreeLocationsKnown);
        UpdateObjective(QuestIds.Q_BA01, "obj_find_elis");
        Debug.Log("[BoundArchivistQuest] Elis met (optional).");
    }

    public void OnEowOperativeMet(bool tokenGranted)
    {
        if (!IsBoundArchivist)
            return;

        StoryFlags.Add(QuestFlags.EowOperativeMet);
        StoryFlags.Add(QuestFlags.EowWatching);

        if (tokenGranted)
            StoryFlags.Add(QuestFlags.BoundArchivistEowToken);

        UpdateObjective(QuestIds.Q_BA01, "obj_handle_eow_operative");
        Debug.Log($"[BoundArchivistQuest] EoW operative met. Token granted: {tokenGranted}");
    }

    public void OnVossDay3Complete()
    {
        if (!IsBoundArchivist)
            return;

        if (!StoryFlags.Has(QuestFlags.EowWatching))
            Debug.LogWarning("[BoundArchivistQuest] Voss Day 3 fired but eow_watching is not set.");

        StoryFlags.Add(QuestFlags.VossDay3BoundArchivist);
        StoryFlags.Add(QuestFlags.RecordNotChurchConfirmed);
        StoryFlags.Add(QuestFlags.VossMovedRecord);
        UpdateObjective(QuestIds.Q_BA01, "obj_speak_voss_day3");
        Debug.Log("[BoundArchivistQuest] Voss Day 3 dialogue done.");
    }

    public void OnCrossReferenceComplete()
    {
        if (!IsBoundArchivist)
            return;

        StoryFlags.Add(QuestFlags.RecordLocationNarrowed);
        StoryFlags.Add(QuestFlags.WitnessRequiredKnown);
        UpdateObjective(QuestIds.Q_BA01, "obj_cross_reference");
        Debug.Log("[BoundArchivistQuest] Cross-reference complete — location narrowed.");
    }

    public void OnQuestComplete(bool branchC_eowInvited)
    {
        if (!IsBoundArchivist)
            return;

        if (branchC_eowInvited)
            StoryFlags.Add(QuestFlags.BoundArchivistQ1EowInvited);

        var quest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest(QuestIds.Q_BA01) : null;

        if (quest != null)
            QuestManager.Instance.CompleteQuest(quest);
        else
            StoryFlags.Add(QuestFlags.BoundArchivistQuest1Done);

        Debug.Log($"[BoundArchivistQuest] Quest 1 complete. EoW invited: {branchC_eowInvited}");
    }

    public static bool HasElisNotes => StoryFlags.Has(QuestFlags.ThreeLocationsKnown);

    public static bool HasEowToken => StoryFlags.Has(QuestFlags.BoundArchivistEowToken);

    public static string GetPreferredWitness()
    {
        if (HasEowToken)
            return "eow";

        if (StoryFlags.Has(QuestFlags.ElisMet))
            return "elis";

        return "maren";
    }

    private static void TryAutoStartQuest(string questID)
    {
        if (QuestManager.Instance == null)
            return;

        var quest = System.Array.Find(QuestManager.Instance.allQuests, q => q.questID == questID);

        if (quest != null && QuestManager.Instance.CanStartQuest(quest))
            QuestManager.Instance.StartQuest(quest);
    }

    private static void UpdateObjective(string questID, string objectiveID)
    {
        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.UpdateObjectiveProgress(questID, objectiveID, 1);
    }
}
