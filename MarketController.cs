using UnityEngine;

public class MarketController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject marketOpenUI;
    public GameObject marketClosedUI;

    [Header("Open During Phases")]
    public bool openDuringMorning = true;
    public bool openDuringNoon = true;
    public bool openDuringEvening = false;
    public bool openDuringNight = false;

    void Start()
    {
        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(TimePhaseManager.Instance.currentPhase);
        }
    }

    void OnDestroy()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(TimePhase phase)
    {
        bool isOpen = phase switch
        {
            TimePhase.Morning => openDuringMorning,
            TimePhase.Noon => openDuringNoon,
            TimePhase.Evening => openDuringEvening,
            TimePhase.Night => openDuringNight,
            _ => false
        };

        if (marketOpenUI != null)
            marketOpenUI.SetActive(isOpen);

        if (marketClosedUI != null)
            marketClosedUI.SetActive(!isOpen);
    }
}
