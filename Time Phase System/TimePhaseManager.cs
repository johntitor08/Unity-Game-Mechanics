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
    private CurrencyManager subscribedCurrency;
    private ScenarioManager subscribedScenarios;
    private int storyStepCounter = 0;
    public event System.Action<TimePhase> OnPhaseChanged;

    [Header("Time Settings")]
    public TimePhase currentPhase = TimePhase.Morning;
    public bool autoProgress = false;
    [Min(0.01f)]
    public float phaseDuration = 300f;
    public float timeScale = 1f;

    [Header("Story-Driven Time")]
    public bool advanceOnScenarioSteps = true;
    [Min(1)]
    public int stepsPerPhase = 2;

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
            DontDestroyOnLoad(gameObject);
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

        EnsureCurrencySubscription();
        EnsureScenarioSubscription();
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    void Update()
    {
        if (subscribedCurrency == null)
            EnsureCurrencySubscription();

        if (subscribedScenarios == null)
            EnsureScenarioSubscription();

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
        if (subscribedCurrency != null)
            subscribedCurrency.OnCurrencyChanged -= OnCurrencyChanged;

        if (subscribedScenarios != null)
        {
            subscribedScenarios.OnStepStart -= OnScenarioStepStart;
            subscribedScenarios.OnStepComplete -= OnScenarioStepComplete;
        }
    }

    void EnsureScenarioSubscription()
    {
        ScenarioManager current = ScenarioManager.Instance;

        if (current == subscribedScenarios || current == null)
            return;

        if (subscribedScenarios != null)
        {
            subscribedScenarios.OnStepStart -= OnScenarioStepStart;
            subscribedScenarios.OnStepComplete -= OnScenarioStepComplete;
        }

        subscribedScenarios = current;
        subscribedScenarios.OnStepStart += OnScenarioStepStart;
        subscribedScenarios.OnStepComplete += OnScenarioStepComplete;
    }

    void OnScenarioStepStart(ScenarioStep step)
    {
        if (!advanceOnScenarioSteps || step == null)
            return;

        TimePhase? named = PhaseFromStepName(step.stepName);

        if (named.HasValue && named.Value > currentPhase)
        {
            storyStepCounter = 0;
            SetPhase(named.Value);
        }
    }

    void OnScenarioStepComplete(ScenarioStep step)
    {
        if (!advanceOnScenarioSteps || currentPhase == TimePhase.Night)
            return;

        storyStepCounter++;

        if (storyStepCounter >= stepsPerPhase)
        {
            storyStepCounter = 0;
            NextPhase();
        }
    }

    static TimePhase? PhaseFromStepName(string stepName)
    {
        if (string.IsNullOrEmpty(stepName))
            return null;

        string n = stepName.ToLowerInvariant();

        if (n.Contains("dawn") || n.Contains("morning"))
            return TimePhase.Morning;

        if (n.Contains("noon"))
            return TimePhase.Noon;

        if (n.Contains("evening") || n.Contains("dusk"))
            return TimePhase.Evening;

        if (n.Contains("night"))
            return TimePhase.Night;

        return null;
    }

    void EnsureCurrencySubscription()
    {
        CurrencyManager current = CurrencyManager.Instance;

        if (current == subscribedCurrency)
            return;

        if (subscribedCurrency != null)
            subscribedCurrency.OnCurrencyChanged -= OnCurrencyChanged;

        subscribedCurrency = current;

        if (subscribedCurrency != null)
        {
            subscribedCurrency.OnCurrencyChanged += OnCurrencyChanged;
            UpdatePhaseButtons();
        }
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
        storyStepCounter = 0;
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
        storyStepCounter = 0;

        if (previousPhase == TimePhase.Night && currentPhase == TimePhase.Morning)
            TriggerNewDay();

        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    public bool CanChangePhaseManually()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.inCombat)
            return false;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
            return false;

        return true;
    }

    public void GoPreviousPhase()
    {
        if (!CanChangePhaseManually() || currentPhase == TimePhase.Morning || CurrencyManager.Instance == null || !CurrencyManager.Instance.Spend(previousPhaseCostType, previousPhaseCost))
            return;

        currentPhase = currentPhase switch
        {
            TimePhase.Noon => TimePhase.Morning,
            TimePhase.Evening => TimePhase.Noon,
            TimePhase.Night => TimePhase.Evening,
            _ => currentPhase
        };

        phaseTimer = 0f;
        storyStepCounter = 0;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    public void GoNextPhase()
    {
        if (!CanChangePhaseManually())
            return;

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
        {
            bool canGoNext = currentPhase != TimePhase.Night;
            nextPhaseButton.interactable = canGoNext;
        }
    }

    public float GetPhaseProgress()
    {
        if (autoProgress)
            return phaseDuration <= 0f ? 0f : Mathf.Clamp01(phaseTimer / phaseDuration);

        if (advanceOnScenarioSteps)
            return currentPhase == TimePhase.Night ? 1f : Mathf.Clamp01((float)storyStepCounter / stepsPerPhase);

        return 0f;
    }

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
