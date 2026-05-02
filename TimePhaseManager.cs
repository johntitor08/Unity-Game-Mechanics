using UnityEngine;
using UnityEngine.UI;

public enum TimePhase
{
    Morning,
    Noon,
    Evening,
    Night
}

public class TimePhaseManager : MonoBehaviour
{
    public static TimePhaseManager Instance;
    private float phaseTimer = 0f;
    public event System.Action<TimePhase> OnPhaseChanged;

    [Header("Time Settings")]
    public TimePhase currentPhase = TimePhase.Morning;
    public bool autoProgress = true;
    public float phaseDuration = 300f;
    public float timeScale = 1f;

    [Header("Visual Effects")]
    public bool changeScreenColor = true;
    public Camera mainCamera;

    [Header("Phase Buttons")]
    public Button nextPhaseButton;
    public Button previousPhaseButton;

    [Header("Previous Phase Cost")]
    public CurrencyType previousPhaseCostType = CurrencyType.Gold;
    public int previousPhaseCost = 50;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Start()
    {
        if (nextPhaseButton != null)
            nextPhaseButton.onClick.AddListener(GoNextPhase);

        if (previousPhaseButton != null)
            previousPhaseButton.onClick.AddListener(GoPreviousPhase);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;

        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    void Update()
    {
        if (!autoProgress)
            return;

        phaseTimer += Time.deltaTime * timeScale;

        if (phaseTimer >= phaseDuration)
        {
            phaseTimer = 0f;
            NextPhase();
        }
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        if (type == previousPhaseCostType)
            UpdatePhaseButtons();
    }

    public void SetPhase(TimePhase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        currentPhase = newPhase;
        phaseTimer = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    public void NextPhase()
    {
        TimePhase previousPhase = currentPhase;

        currentPhase = currentPhase switch
        {
            TimePhase.Morning => TimePhase.Noon,
            TimePhase.Noon => TimePhase.Evening,
            TimePhase.Evening => TimePhase.Night,
            TimePhase.Night => TimePhase.Morning,
            _ => TimePhase.Morning
        };

        phaseTimer = 0f;

        if (previousPhase == TimePhase.Night && currentPhase == TimePhase.Morning)
            TriggerNewDay();

        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    public void GoPreviousPhase()
    {
        if (currentPhase == TimePhase.Morning || CurrencyManager.Instance == null || !CurrencyManager.Instance.Spend(previousPhaseCostType, previousPhaseCost))
            return;

        currentPhase = currentPhase switch
        {
            TimePhase.Morning => TimePhase.Night,
            TimePhase.Noon => TimePhase.Morning,
            TimePhase.Evening => TimePhase.Noon,
            TimePhase.Night => TimePhase.Evening,
            _ => TimePhase.Morning
        };

        phaseTimer = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    public void GoNextPhase()
    {
        NextPhase();
    }

    public void TriggerNewDay()
    {
        if (TimeUI.Instance != null)
            TimeUI.Instance.IncrementDay();
    }

    void UpdateCameraColor(TimePhase phase)
    {
        if (!changeScreenColor || mainCamera == null)
            return;

        mainCamera.backgroundColor = phase switch
        {
            TimePhase.Morning => new Color(1f, 0.95f, 0.85f),
            TimePhase.Noon => Color.white,
            TimePhase.Evening => new Color(1f, 0.8f, 0.6f),
            TimePhase.Night => new Color(0.6f, 0.65f, 0.8f),
            _ => Color.white
        };
    }

    void UpdatePhaseButtons()
    {
        if (previousPhaseButton != null)
        {
            bool canGoBack = currentPhase != TimePhase.Morning && CurrencyManager.Instance != null && CurrencyManager.Instance.Has(previousPhaseCostType, previousPhaseCost);
            previousPhaseButton.interactable = canGoBack;
        }

        if (nextPhaseButton != null)
            nextPhaseButton.interactable = currentPhase != TimePhase.Night;
    }

    public float GetPhaseProgress() => Mathf.Clamp01(phaseTimer / phaseDuration);

    public float GetTimeRemaining() => Mathf.Max(0f, phaseDuration - phaseTimer);

    public void SetPhaseProgress(float progress)
    {
        phaseTimer = Mathf.Clamp01(progress) * phaseDuration;
    }

    public string GetTimeRemainingFormatted()
    {
        float remaining = GetTimeRemaining();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
