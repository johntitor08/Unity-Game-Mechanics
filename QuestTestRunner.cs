using UnityEngine;

public class QuestTestRunner : MonoBehaviour
{
    public enum TestOrigin { BoundArchivist, ForeignEcho, SinnedGuardian }
    private BoundArchivistQuestController _archivist;
    private ForeignEchoQuestController _echo;
    private SinnedGuardianQuestController _guardian;

    [Header("Test Settings")]
    public TestOrigin origin = TestOrigin.BoundArchivist;
    public bool autoStartOnPlay = true;

    [Header("Archivist — EoW branch")]
    public bool archivistEowTokenGranted = true;
    public bool archivistEowInvited = true;

    [Header("Echo — exchange")]
    public bool echoExchangeAccepted = true;

    void Start()
    {
        _archivist = FindAnyObjectByType<BoundArchivistQuestController>();
        _echo = FindAnyObjectByType<ForeignEchoQuestController>();
        _guardian = FindAnyObjectByType<SinnedGuardianQuestController>();

        if (autoStartOnPlay)
            StartOrigin();
    }

    [ContextMenu("1 · Start origin")]
    public void StartOrigin()
    {
        switch (origin)
        {
            case TestOrigin.BoundArchivist:
                if (!Require(_archivist, nameof(BoundArchivistQuestController)))
                    return;

                _archivist.OnOpeningSceneComplete();
                Log($"Archivist başladı → {QuestIds.Q_BA01}");
                break;

            case TestOrigin.ForeignEcho:
                if (!Require(_echo, nameof(ForeignEchoQuestController)))
                    return;

                _echo.OnOpeningSceneComplete();
                Log($"Echo başladı → {QuestIds.Q_FE01}");
                break;

            case TestOrigin.SinnedGuardian:
                if (!Require(_guardian, nameof(SinnedGuardianQuestController)))
                    return;

                _guardian.OnOpeningSceneComplete();
                Log($"Guardian başladı → {QuestIds.Q_SG01}");
                break;
        }
    }

    [ContextMenu("Archivist · 2 · Maren gate met")]
    public void Archivist_MarenGateMet()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnMarenGateMet();
        Log("obj_speak_maren_gate ✓");
    }

    [ContextMenu("Archivist · 3 · Maren kitchen complete")]
    public void Archivist_MarenKitchenComplete()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnMarenKitchenComplete();
        Log("obj_learn_maren_knowledge ✓");
    }

    [ContextMenu("Archivist · 4 · Elis met (optional)")]
    public void Archivist_ElisMet()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnElisMet();
        Log("obj_find_elis ✓ (optional)");
    }

    [ContextMenu("Archivist · 5 · EoW operative met")]
    public void Archivist_EowOperativeMet()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnEowOperativeMet(archivistEowTokenGranted);
        Log($"obj_handle_eow_operative ✓  token:{archivistEowTokenGranted}");
    }

    [ContextMenu("Archivist · 6 · Voss day 3")]
    public void Archivist_VossDay3()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnVossDay3Complete();
        Log("obj_speak_voss_day3 ✓");
    }

    [ContextMenu("Archivist · 7 · Cross reference")]
    public void Archivist_CrossReference()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnCrossReferenceComplete();
        Log("obj_cross_reference ✓");
    }

    [ContextMenu("Archivist · 8 · Complete quest")]
    public void Archivist_Complete()
    {
        if (!Require(_archivist))
            return;

        _archivist.OnQuestComplete(archivistEowInvited);
        Log($"{QuestIds.Q_BA01} COMPLETE  eowInvited:{archivistEowInvited}");
    }

    [ContextMenu("Echo · 2 · Maren door complete")]
    public void Echo_MarenDoor()
    {
        if (!Require(_echo))
            return;

        _echo.OnMarenDoorComplete();
        Log("obj_follow_shadow_maren ✓");
    }

    [ContextMenu("Echo · 3 · Maren mission given")]
    public void Echo_MarenMission()
    {
        if (!Require(_echo))
            return;

        _echo.OnMarenMissionGiven();
        Log("obj_understand_maren_need ✓");
    }

    [ContextMenu("Echo · 4 · Mireya met (optional)")]
    public void Echo_MireyaMet()
    {
        if (!Require(_echo))
            return;

        _echo.OnMireyaMet();
        Log("obj_speak_mireya ✓ (optional)");
    }

    [ContextMenu("Echo · 5 · Chico met (optional)")]
    public void Echo_ChicoMet()
    {
        if (!Require(_echo))
            return;

        _echo.OnChicoMet(echoExchangeAccepted);
        Log($"obj_meet_chico ✓ (optional)  exchange:{echoExchangeAccepted}");
    }

    [ContextMenu("Echo · 6 · Voss day 3")]
    public void Echo_VossDay3()
    {
        if (!Require(_echo))
            return;

        _echo.OnVossDay3Complete();
        Log("obj_speak_voss_day3 ✓");
    }

    [ContextMenu("Echo · 7 · Three crystals seen")]
    public void Echo_ThreeCrystals()
    {
        if (!Require(_echo))
            return;

        _echo.OnThreeCrystalsSeen();
        Log("three_crystals_seen flag set");
    }

    [ContextMenu("Echo · 8 · Fourth slot resonates")]
    public void Echo_FourthSlot()
    {
        if (!Require(_echo))
            return;

        _echo.OnFourthSlotResonates();
        Log("fourth_slot_resonates_foreign_echo flag set");
    }

    [ContextMenu("Echo · 9 · Originating record detected")]
    public void Echo_OriginatingRecord()
    {
        if (!Require(_echo))
            return;

        _echo.OnOriginatingRecordDetected();
        Log("obj_identify_axios_anomaly ✓");
    }

    [ContextMenu("Echo · 10 · Complete quest")]
    public void Echo_Complete()
    {
        if (!Require(_echo))
            return;

        _echo.OnQuestComplete();
        Log($"{QuestIds.Q_FE01} COMPLETE");
    }

    [ContextMenu("Guardian · 2 · Maren night met")]
    public void Guardian_MarenNight()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnMarenNightMet();
        Log("q_sg01_obj1 ✓");
    }

    [ContextMenu("Guardian · 3 · Aslude met")]
    public void Guardian_AsludeMet()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnAsludeMet();
        Log("q_sg01_obj2 ✓");
    }

    [ContextMenu("Guardian · 4 · Corvin spoken (optional)")]
    public void Guardian_CorvinSpoken()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnCorvinOptionalSpoken();
        Log("q_sg01_obj3 ✓ (optional)");
    }

    [ContextMenu("Guardian · 5 · Western edge learned")]
    public void Guardian_WesternEdge()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnWesternEdgeLearned();
        Log("q_sg01_obj4 ✓");
    }

    [ContextMenu("Guardian · 6 · Voss tracked")]
    public void Guardian_VossTracked()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnVossTracked();
        Log("q_sg01_obj5 ✓");
    }

    [ContextMenu("Guardian · 7 · Voss day 3")]
    public void Guardian_VossDay3()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnVossDay3Complete();
        Log("q_sg01_obj6 ✓");
    }

    [ContextMenu("Guardian · 8 · Second wall found")]
    public void Guardian_SecondWall()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnSecondWallFound();
        Log("q_sg01_obj7 ✓");
    }

    [ContextMenu("Guardian · 9 · Second wall examined")]
    public void Guardian_SecondWallExamined()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnSecondWallExamined();
        Log("q_sg01_obj8 ✓");
    }

    [ContextMenu("Guardian · 10 · Complete quest")]
    public void Guardian_Complete()
    {
        if (!Require(_guardian))
            return;

        _guardian.OnQuestComplete();
        Log($"{QuestIds.Q_SG01} COMPLETE");
    }

    bool Require<T>(T obj, string name = null) where T : Object
    {
        if (obj != null)
            return true;

        Debug.LogError($"[QuestTestRunner] {name ?? typeof(T).Name} sahnede bulunamadı. " + $"Controller'ın sahneye eklendiğinden emin ol.");
        return false;
    }

    void Log(string msg) => Debug.Log($"<color=#00d4a0>[QuestTestRunner]</color> {msg}");
}
