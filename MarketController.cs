using UnityEngine;

public class MarketController : MonoBehaviour
{
    public GameObject marketClosedPanel;
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

        if (marketClosedPanel != null)
            marketClosedPanel.SetActive(!isOpen);

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
        TimePhase.Morning => true,
        TimePhase.Noon => true,
        TimePhase.Evening => true,
        TimePhase.Night => false,
        _ => false
    };
}
