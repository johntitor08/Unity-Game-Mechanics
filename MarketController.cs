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

    public static MarketController Instance;

    void Awake() => Instance = this;

    void Start()
    {
        TimePhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        OnPhaseChanged(TimePhaseManager.Instance.currentPhase);
    }

    void OnDestroy()
    {
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

        marketOpenUI.SetActive(isOpen);
        marketClosedUI.SetActive(!isOpen);
    }

    public bool IsOpen()
    {
        TimePhase phase = TimePhaseManager.Instance.currentPhase;

        return phase switch
        {
            TimePhase.Morning => openDuringMorning,
            TimePhase.Noon => openDuringNoon,
            TimePhase.Evening => openDuringEvening,
            TimePhase.Night => openDuringNight,
            _ => false
        };
    }
}
