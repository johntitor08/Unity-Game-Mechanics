using UnityEngine;

public class VossTimeActivator : MonoBehaviour
{
    [Header("Activation")]
    public int activateOnDay = 3;
    public TimePhase activateOnPhase = TimePhase.Evening;

    void OnEnable()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    void Start()
    {
        gameObject.SetActive(false);
        RefreshVisibility();
    }

    void OnPhaseChanged(TimePhase phase) => RefreshVisibility();

    void RefreshVisibility()
    {
        if (TimeUI.Instance == null || TimePhaseManager.Instance == null)
            return;

        int currentDay = TimeUI.Instance.GetCurrentDay();
        TimePhase currentPhase = TimePhaseManager.Instance.currentPhase;
        bool shouldBeActive = currentDay >= activateOnDay && currentPhase >= activateOnPhase;
        gameObject.SetActive(shouldBeActive);
    }
}
