using UnityEngine;

public class SinnedGuardianQuestController : MonoBehaviour
{
    private static bool IsGuardian => StoryFlags.Has(QuestFlags.SinnedGuardianStart);

    public void OnOpeningSceneComplete()
    {
        if (StoryFlags.Has(QuestFlags.SinnedGuardianOpeningComplete))
            return;

        StoryFlags.Add(QuestFlags.SinnedGuardianOpeningComplete);
        StoryFlags.Add(QuestFlags.SinnedGuardianStart);
        TryAutoStartQuest(QuestIds.Q_SG01);
        Debug.Log("[GuardianQuest] Opening scene complete.");
    }

    public void OnMarenNightMet()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.MarenFirstMeeting);
        StoryFlags.Add(QuestFlags.MarenToldGuardianMission);
        StoryFlags.Add(QuestFlags.VossCollectsNotDestroysKnown);
        StoryFlags.Add(QuestFlags.ShadowGardenQuestAssigned);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj1");
        Debug.Log("[GuardianQuest] Maren night dialogue done.");
    }

    public void OnAsludeMet()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.AsludeMet);
        StoryFlags.Add(QuestFlags.VossReturnsDay3Known);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj2");
        Debug.Log("[GuardianQuest] Aslude met at well.");
    }

    public void OnCorvinOptionalSpoken()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.CorvinOptionalSpoken);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj3");
        Debug.Log("[GuardianQuest] Corvin optional dialogue done.");
    }

    public void OnWesternEdgeLearned()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.DragsimEastClue);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj4");
        Debug.Log("[GuardianQuest] Western edge locations learned.");
    }

    public void OnVossTracked()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.VossTrackedGuardian);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj5");
        Debug.Log("[GuardianQuest] Voss tracked from western road.");
    }

    public void OnVossDay3Complete()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.VossDay3Guardian);
        StoryFlags.Add(QuestFlags.VossReversalClauseHinted);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj6");
        Debug.Log("[GuardianQuest] Voss Day 3 dialogue done.");
    }

    public void OnSecondWallFound()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.DragsimSecondWallKnown);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj7");
        Debug.Log("[GuardianQuest] Second wall found east of Dragsimo.");
    }

    public void OnSecondWallExamined()
    {
        if (!IsGuardian)
            return;

        StoryFlags.Add(QuestFlags.FourthCrystalSlotSeen);
        StoryFlags.Add(QuestFlags.VossPreparedGuardianContract);
        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj8");
        Debug.Log("[GuardianQuest] Behind the second wall examined.");
    }

    public void OnQuestComplete()
    {
        if (!IsGuardian)
            return;

        UpdateObjective(QuestIds.Q_SG01, "q_sg01_obj9");
        var quest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest(QuestIds.Q_SG01) : null;

        if (quest != null)
            QuestManager.Instance.CompleteQuest(quest);
        else
        {
            StoryFlags.Add(QuestFlags.ThreeFamiliesLocated);
            StoryFlags.Add(QuestFlags.ShadowGardenRank1);
            StoryFlags.Add(QuestFlags.SinnedGuardianQuest1Done);
        }

        Debug.Log("[GuardianQuest] Quest 1 complete.");
    }

    public static bool HasCorvinBonus => StoryFlags.Has(QuestFlags.CorvinOptionalSpoken);

    public static bool KnowsReversalClue => StoryFlags.Has(QuestFlags.VossReversalClauseHinted);

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
