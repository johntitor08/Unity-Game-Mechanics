using System.Collections;
using UnityEngine;

public class ShowOnBackgrounds : MonoBehaviour
{
    public GameObject target;
    public int[] visibleOnBackgrounds;
    [Min(1)]
    public int minimumDay = 1;
    public TimePhase[] restrictToPhases;
    public GameObject hover;
    public string hoverHiddenByFlag;
    [Min(1)]
    public int hoverMinimumDay = 1;
    [Min(0f)]
    public float fadeDuration = 0.35f;
    private bool bgSubscribed;
    private bool phaseSubscribed;
    private bool flagSubscribed;
    private int lastBackgroundIndex = -1;
    private bool covered;
    private CanvasGroup group;
    private NPCDialogue ownTalk;
    private Coroutine fade;

    void Start() => TrySubscribe();

    void OnEnable() => TrySubscribe();

    void OnDisable()
    {
        if (bgSubscribed && SceneEvent.Instance != null)
            SceneEvent.Instance.OnBackgroundChanged -= HandleBackgroundChanged;

        if (phaseSubscribed && TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;

        if (flagSubscribed)
            StoryFlags.OnFlagAdded -= HandleFlagAdded;

        bgSubscribed = false;
        phaseSubscribed = false;
        flagSubscribed = false;
    }

    void Update()
    {
        if (!bgSubscribed || !phaseSubscribed)
            TrySubscribe();

        bool nowCovered = IsSceneCovered();

        if (nowCovered != covered)
        {
            covered = nowCovered;
            DialogueNode started = DialogueManager.Instance != null ? DialogueManager.Instance.LastStartNode : null;
            bool sceneReplaced = (CombatManager.Instance != null && CombatManager.Instance.inCombat) || (SceneEvent.Instance != null && SceneEvent.Instance.IsShowingQuestLocation) || (ScenarioManager.Instance != null && ScenarioManager.Instance.IsScenarioActive()) || (DialogueManager.WorldClicksBlocked && started != null && started.backgroundImage != null);
            Refresh(animate: !sceneReplaced);
        }
    }

    bool IsSceneCovered() => ((DialogueManager.WorldClicksBlocked || (SceneEvent.Instance != null && SceneEvent.Instance.IsHudReturning)) && !OwnTalkJustEnded()) || (ScenarioManager.Instance != null && ScenarioManager.Instance.IsScenarioActive()) || (CombatManager.Instance != null && CombatManager.Instance.inCombat) || (SceneEvent.Instance != null && SceneEvent.Instance.IsShowingQuestLocation);

    bool OwnTalkJustEnded()
    {
        DialogueManager dm = DialogueManager.Instance;

        if (dm == null || dm.IsInDialogue() || hover == null)
            return false;

        if (ownTalk == null)
            ownTalk = hover.GetComponent<NPCDialogue>();

        return ownTalk != null && ownTalk.startNode != null && ownTalk.startNode.backgroundImage == null && dm.LastStartNode == ownTalk.startNode;
    }

    void TrySubscribe()
    {
        if (!bgSubscribed && SceneEvent.Instance != null)
        {
            SceneEvent.Instance.OnBackgroundChanged += HandleBackgroundChanged;
            bgSubscribed = true;

            if (target != null)
                target.SetActive(false);
        }

        if (!phaseSubscribed && TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            phaseSubscribed = true;
        }

        if (!flagSubscribed)
        {
            StoryFlags.OnFlagAdded += HandleFlagAdded;
            flagSubscribed = true;
        }
    }

    void HandleBackgroundChanged(int index)
    {
        lastBackgroundIndex = index;
        Refresh(animate: false);
    }

    void HandlePhaseChanged(TimePhase phase) => Refresh(animate: true);

    void HandleFlagAdded(string flag)
    {
        if (flag == hoverHiddenByFlag)
            ApplyHover();
    }

    void Refresh(bool animate)
    {
        if (target == null)
            return;

        covered = IsSceneCovered();
        bool show = !covered && BackgroundOk() && DayOk() && PhaseOk();
        ApplyHover();

        if (fade != null)
        {
            StopCoroutine(fade);
            fade = null;
        }

        if (!animate || fadeDuration <= 0f || !isActiveAndEnabled)
        {
            Group().alpha = 1f;
            target.SetActive(show);
            return;
        }

        if (show == target.activeSelf && Group().alpha >= 1f)
            return;

        fade = StartCoroutine(Fade(show));
    }

    void ApplyHover()
    {
        if (hover == null)
            return;

        bool offerOpen = TimeUI.Instance == null || TimeUI.Instance.GetCurrentDay() >= hoverMinimumDay;
        hover.SetActive(offerOpen && (string.IsNullOrEmpty(hoverHiddenByFlag) || !StoryFlags.Has(hoverHiddenByFlag)));
    }

    IEnumerator Fade(bool show)
    {
        CanvasGroup g = Group();
        float from = target.activeSelf ? g.alpha : 0f;
        float to = show ? 1f : 0f;

        if (show)
            target.SetActive(true);

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            g.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        g.alpha = to;

        if (!show)
        {
            target.SetActive(false);
            g.alpha = 1f;
        }

        fade = null;
    }

    CanvasGroup Group()
    {
        if (group == null)
        {
            group = target.GetComponent<CanvasGroup>();

            if (group == null)
                group = target.AddComponent<CanvasGroup>();
        }

        return group;
    }

    bool BackgroundOk()
    {
        if (visibleOnBackgrounds == null)
            return false;

        foreach (int i in visibleOnBackgrounds)
            if (i == lastBackgroundIndex)
                return true;

        return false;
    }

    bool DayOk() => TimeUI.Instance == null || TimeUI.Instance.GetCurrentDay() >= minimumDay;

    bool PhaseOk()
    {
        if (restrictToPhases == null || restrictToPhases.Length == 0 || TimePhaseManager.Instance == null)
            return true;

        TimePhase current = TimePhaseManager.Instance.currentPhase;

        foreach (TimePhase p in restrictToPhases)
            if (p == current)
                return true;

        return false;
    }
}
