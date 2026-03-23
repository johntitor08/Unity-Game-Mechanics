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
        if (TimePhaseManager.Instance == null)
            return;

        TimePhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        OnPhaseChanged(TimePhaseManager.Instance.currentPhase);
    }

    void OnDestroy()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(TimePhase phase)
    {
        bool isOpen = IsOpenDuring(phase);

        if (marketOpenUI != null)
            marketOpenUI.SetActive(isOpen);

        if (marketClosedUI != null)
            marketClosedUI.SetActive(!isOpen);

        if (ShopUI.Instance != null)
            ShopUI.Instance.UpdateMarketStatus();
    }

    public bool IsOpen()
    {
        if (TimePhaseManager.Instance == null)
            return true;

        return IsOpenDuring(TimePhaseManager.Instance.currentPhase);
    }

    private bool IsOpenDuring(TimePhase phase) => phase switch
    {
        TimePhase.Morning => openDuringMorning,
        TimePhase.Noon => openDuringNoon,
        TimePhase.Evening => openDuringEvening,
        TimePhase.Night => openDuringNight,
        _ => false
    };
}
