using UnityEngine;

public class PhaseLockedObject : MonoBehaviour
{
    [Header("Required Phase")]
    public TimePhase requiredPhase;

    [Header("Optional - Multiple Phases")]
    public bool multiplePhasesAllowed = false;
    public TimePhase[] allowedPhases;

    void Start()
    {
        if (TimePhaseManager.Instance == null)
            return;

        TimePhaseManager.Instance.OnPhaseChanged += CheckPhase;
        CheckPhase(TimePhaseManager.Instance.currentPhase);
    }

    void OnDestroy()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= CheckPhase;
    }

    void CheckPhase(TimePhase phase)
    {
        bool shouldBeActive = false;

        if (multiplePhasesAllowed && allowedPhases.Length > 0)
        {
            foreach (var allowedPhase in allowedPhases)
            {
                if (phase == allowedPhase)
                {
                    shouldBeActive = true;
                    break;
                }
            }
        }
        else
        {
            shouldBeActive = phase == requiredPhase;
        }

        gameObject.SetActive(shouldBeActive);
    }
}
