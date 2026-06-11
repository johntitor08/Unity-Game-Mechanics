using UnityEngine;

public class ShowOnBackgrounds : MonoBehaviour
{
    [Tooltip("Object to show/hide. Use a child so this component stays active.")]
    public GameObject target;

    [Tooltip("Background indices (SceneEvent.bgs) on which the target is visible.")]
    public int[] visibleOnBackgrounds;

    [Min(1)]
    [Tooltip("Target stays hidden before this in-game day.")]
    public int minimumDay = 1;

    [Tooltip("Optional. If non-empty, the target is only visible during these phases.")]
    public TimePhase[] restrictToPhases;

    private bool bgSubscribed;
    private bool phaseSubscribed;
    private int lastBackgroundIndex = -1;

    void Start() => TrySubscribe();

    void OnEnable() => TrySubscribe();

    void OnDisable()
    {
        if (bgSubscribed && SceneEvent.Instance != null)
            SceneEvent.Instance.OnBackgroundChanged -= HandleBackgroundChanged;

        if (phaseSubscribed && TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;

        bgSubscribed = false;
        phaseSubscribed = false;
    }

    void Update()
    {
        if (!bgSubscribed || !phaseSubscribed)
            TrySubscribe();
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
    }

    void HandleBackgroundChanged(int index)
    {
        lastBackgroundIndex = index;
        Refresh();
    }

    void HandlePhaseChanged(TimePhase phase) => Refresh();

    void Refresh()
    {
        if (target == null)
            return;

        target.SetActive(BackgroundOk() && DayOk() && PhaseOk());
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
