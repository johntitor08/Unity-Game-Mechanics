using UnityEngine;

public class ForeignEchoQuestController : MonoBehaviour
{
    private static bool IsForeignEcho => StoryFlags.Has(QuestFlags.ForeignEchoStart);

    public void OnOpeningSceneComplete()
    {
        if (StoryFlags.Has(QuestFlags.ForeignEchoOpeningComplete))
            return;

        StoryFlags.Add(QuestFlags.ForeignEchoOpeningComplete);
        StoryFlags.Add(QuestFlags.ForeignEchoStart);
        StoryFlags.Add(QuestFlags.ShadowAnomalySeen);
        StoryFlags.Add(QuestFlags.AshenveilEntered);
        TryAutoStartQuest("quest_foreign_echo_01");
        Debug.Log("[ForeignEchoQuest] Scene 1.1 flags set.");
    }

    public void OnMarenDoorComplete()
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.MarenMetForeignEcho);
        StoryFlags.Add(QuestFlags.VossCantIdentifyForeignEcho);
        StoryFlags.Add(QuestFlags.AxiosResonanceExplained);
        UpdateObjective("quest_foreign_echo_01", "obj_follow_shadow_maren");
        Debug.Log("[ForeignEchoQuest] Maren door dialogue done.");
    }

    public void OnMarenMissionGiven()
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.MarenMissionGiven);
        StoryFlags.Add(QuestFlags.ForeignEchoInvisibleToTracking);
        StoryFlags.Add(QuestFlags.ChamberTargetKnown);
        UpdateObjective("quest_foreign_echo_01", "obj_understand_maren_need");
        Debug.Log("[ForeignEchoQuest] Maren mission given.");
    }

    public void OnMireyaMet()
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.MireyaMetForeignEcho);
        StoryFlags.Add(QuestFlags.LurkerPatrolData);
        UpdateObjective("quest_foreign_echo_01", "obj_speak_mireya");
        Debug.Log("[ForeignEchoQuest] Mireya met (optional).");
    }

    public void OnChicoMet(bool exchangeAccepted)
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.ChicoMet);
        StoryFlags.Add(QuestFlags.AwamorIncomingKnown);

        if (exchangeAccepted)
            StoryFlags.Add(QuestFlags.AxiosFrequencyShared);

        UpdateObjective("quest_foreign_echo_01", "obj_meet_chico");
        Debug.Log($"[ForeignEchoQuest] Chico met. Exchange accepted: {exchangeAccepted}");
    }

    public void OnVossDay3Complete()
    {
        if (!IsForeignEcho)
            return;

        if (!StoryFlags.Has(QuestFlags.ChamberTargetKnown))
            Debug.LogWarning("[ForeignEchoQuest] Voss Day 3 fired but chamber_target_known is not set.");

        StoryFlags.Add(QuestFlags.VossDay3ForeignEcho);
        StoryFlags.Add(QuestFlags.ChamberFrequencyWarning);
        StoryFlags.Add(QuestFlags.VossCannotTrackForeignEcho);
        UpdateObjective("quest_foreign_echo_01", "obj_speak_voss_day3");
        Debug.Log("[ForeignEchoQuest] Voss Day 3 dialogue done.");
    }

    public void OnThreeCrystalsSeen()
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.ThreeCrystalsSeen);
        Debug.Log("[ForeignEchoQuest] Three crystals observed.");
    }

    public void OnFourthSlotResonates()
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.FourthSlotResonatesForeignEcho);
        Debug.Log("[ForeignEchoQuest] Fourth slot resonates with Echo frequency.");
    }

    public void OnOriginatingRecordDetected()
    {
        if (!IsForeignEcho)
            return;

        StoryFlags.Add(QuestFlags.OriginatingRecordBelow);
        StoryFlags.Add(QuestFlags.AxiosAnomalyIdentified);
        UpdateObjective("quest_foreign_echo_01", "obj_identify_axios_anomaly");
        Debug.Log("[ForeignEchoQuest] Originating record detected below chamber floor.");
    }

    public void OnQuestComplete()
    {
        if (!IsForeignEcho)
            return;

        var quest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest("quest_foreign_echo_01") : null;

        if (quest != null)
            QuestManager.Instance.CompleteQuest(quest);
        else
        {
            StoryFlags.Add(QuestFlags.ForeignEchoQuest1Done);
            StoryFlags.Add(QuestFlags.ChamberInteriorSeen);
        }

        Debug.Log("[ForeignEchoQuest] Quest 1 complete.");
    }

    public static float ChamberDetectionMultiplier()
    {
        float mult = 1f;

        if (StoryFlags.Has(QuestFlags.LurkerPatrolData))
            mult -= 0.40f;

        if (StoryFlags.Has(QuestFlags.ChamberFrequencyWarning))
            mult -= 0.15f;

        return Mathf.Clamp01(mult);
    }

    public static bool HasBothOptionals => StoryFlags.Has(QuestFlags.MireyaMetForeignEcho) && StoryFlags.Has(QuestFlags.ChicoMet);

    public static bool ChamberFullyExplored => StoryFlags.Has(QuestFlags.ThreeCrystalsSeen) && StoryFlags.Has(QuestFlags.FourthSlotResonatesForeignEcho) && StoryFlags.Has(QuestFlags.OriginatingRecordBelow);

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
