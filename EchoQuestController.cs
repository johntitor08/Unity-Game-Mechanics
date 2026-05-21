using UnityEngine;

public class EchoQuestController : MonoBehaviour
{
    private static bool IsEcho => StoryFlags.Has(QuestFlags.EchoStart);

    public void OnOpeningSceneComplete()
    {
        StoryFlags.Add(QuestFlags.EchoStart);
        StoryFlags.Add(QuestFlags.ShadowAnomalySeen);
        StoryFlags.Add(QuestFlags.AshenveilEntered);
        TryAutoStartQuest("quest_echo_01");
        Debug.Log("[EchoQuest] Scene 1.1 flags set.");
    }

    public void OnMarenDoorComplete()
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.MarenMetEcho);
        StoryFlags.Add(QuestFlags.VossCantIdentifyEcho);
        StoryFlags.Add(QuestFlags.AxiosResonanceExplained);
        UpdateObjective("quest_echo_01", "obj_follow_shadow_maren");
        Debug.Log("[EchoQuest] Maren door dialogue done.");
    }

    public void OnMarenMissionGiven()
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.MarenMissionGiven);
        StoryFlags.Add(QuestFlags.EchoInvisibleToTracking);
        StoryFlags.Add(QuestFlags.ChamberTargetKnown);
        UpdateObjective("quest_echo_01", "obj_understand_maren_need");
        Debug.Log("[EchoQuest] Maren mission given.");
    }

    public void OnMireyaMet()
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.MireyaMetEcho);
        StoryFlags.Add(QuestFlags.LurkerPatrolData);
        UpdateObjective("quest_echo_01", "obj_speak_mireya");
        Debug.Log("[EchoQuest] Mireya met (optional).");
    }

    public void OnChicoMet(bool exchangeAccepted)
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.ChicoMet);
        StoryFlags.Add(QuestFlags.AwamorIncomingKnown);

        if (exchangeAccepted)
            StoryFlags.Add(QuestFlags.AxiosFrequencyShared);

        UpdateObjective("quest_echo_01", "obj_meet_chico");
        Debug.Log($"[EchoQuest] Chico met. Exchange accepted: {exchangeAccepted}");
    }

    public void OnVossDay3Complete()
    {
        if (!IsEcho)
            return;

        if (!StoryFlags.Has(QuestFlags.ChamberTargetKnown))
            Debug.LogWarning("[EchoQuest] Voss Day 3 fired but chamber_target_known is not set.");

        StoryFlags.Add(QuestFlags.VossDay3Echo);
        StoryFlags.Add(QuestFlags.ChamberFrequencyWarning);
        StoryFlags.Add(QuestFlags.VossCannotTrackEcho);
        UpdateObjective("quest_echo_01", "obj_speak_voss_day3");
        Debug.Log("[EchoQuest] Voss Day 3 dialogue done.");
    }

    public void OnThreeCrystalsSeen()
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.ThreeCrystalsSeen);
        Debug.Log("[EchoQuest] Three crystals observed.");
    }

    public void OnFourthSlotResonates()
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.FourthSlotResonatesEcho);
        Debug.Log("[EchoQuest] Fourth slot resonates with Echo frequency.");
    }

    public void OnOriginatingRecordDetected()
    {
        if (!IsEcho)
            return;

        StoryFlags.Add(QuestFlags.OriginatingRecordBelow);
        StoryFlags.Add(QuestFlags.AxiosAnomalyIdentified);
        UpdateObjective("quest_echo_01", "obj_identify_axios_anomaly");
        Debug.Log("[EchoQuest] Originating record detected below chamber floor.");
    }

    public void OnQuestComplete()
    {
        if (!IsEcho)
            return;

        var quest = QuestManager.Instance != null ? QuestManager.Instance.GetActiveQuest("quest_echo_01") : null;

        if (quest != null)
            QuestManager.Instance.CompleteQuest(quest);
        else
        {
            StoryFlags.Add(QuestFlags.EchoQuest1Done);
            StoryFlags.Add(QuestFlags.ChamberInteriorSeen);
        }

        Debug.Log("[EchoQuest] Quest 1 complete.");
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

    public static bool HasBothOptionals => StoryFlags.Has(QuestFlags.MireyaMetEcho) && StoryFlags.Has(QuestFlags.ChicoMet);

    public static bool ChamberFullyExplored => StoryFlags.Has(QuestFlags.ThreeCrystalsSeen) && StoryFlags.Has(QuestFlags.FourthSlotResonatesEcho) && StoryFlags.Has(QuestFlags.OriginatingRecordBelow);

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
