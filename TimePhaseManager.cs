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
    private bool isFirstMorning = true;
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
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);

        if (nextPhaseButton != null)
            nextPhaseButton.onClick.AddListener(GoNextPhase);

        if (previousPhaseButton != null)
            previousPhaseButton.onClick.AddListener(GoPreviousPhase);

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

    public void SetPhase(TimePhase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        currentPhase = newPhase;
        phaseTimer = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
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

        if (previousPhase == TimePhase.Night && currentPhase == TimePhase.Morning)
        {
            if (TimeUI.Instance != null)
                TimeUI.Instance.IncrementDay();

            isFirstMorning = false;
        }

        phaseTimer = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
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

    public float GetPhaseProgress()
    {
        return Mathf.Clamp01(phaseTimer / phaseDuration);
    }

    public void SetPhaseProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        phaseTimer = progress * phaseDuration;
    }

    public float GetTimeRemaining()
    {
        return Mathf.Max(0f, phaseDuration - phaseTimer);
    }

    public string GetTimeRemainingFormatted()
    {
        float remaining = GetTimeRemaining();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void SetIsFirstMorning(bool value)
    {
        isFirstMorning = value;
    }

    public void GoPreviousPhase()
    {
        if (TimeUI.Instance != null && TimeUI.Instance.GetCurrentDay() == 1 && currentPhase == TimePhase.Morning)
            return;

        TimePhase previousPhase = currentPhase;

        currentPhase = currentPhase switch
        {
            TimePhase.Morning => TimePhase.Night,
            TimePhase.Noon => TimePhase.Morning,
            TimePhase.Evening => TimePhase.Noon,
            TimePhase.Night => TimePhase.Evening,
            _ => TimePhase.Morning
        };

        if (previousPhase == TimePhase.Morning && currentPhase == TimePhase.Night)
        {
            if (TimeUI.Instance != null && TimeUI.Instance.GetCurrentDay() > 1)
                TimeUI.Instance.SetDay(TimeUI.Instance.GetCurrentDay() - 1);
        }

        phaseTimer = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        UpdatePhaseButtons();
    }

    public void GoNextPhase()
    {
        NextPhase();
        UpdatePhaseButtons();
    }

    void UpdatePhaseButtons()
    {
        if (previousPhaseButton == null)
            return;

        bool isFirstMorningNow = TimeUI.Instance != null && TimeUI.Instance.GetCurrentDay() == 1 && currentPhase == TimePhase.Morning;
        previousPhaseButton.gameObject.SetActive(!isFirstMorningNow);
    }
}
