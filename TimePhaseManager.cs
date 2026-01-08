using UnityEngine;

public class TimePhaseManager : MonoBehaviour
{
    public static TimePhaseManager Instance;

    [Header("Time Settings")]
    public TimePhase currentPhase = TimePhase.Morning;
    public bool autoProgress = true;
    public float phaseDuration = 300f; // Duration in seconds (300 = 5 minutes)
    public float timeScale = 1f; // Time speed multiplier

    [Header("Visual Effects")]
    public bool changeScreenColor = true;
    public Camera mainCamera;

    private float phaseTimer = 0f;
    private bool isFirstMorning = true; // Track if this is the first morning (game start)

    public event System.Action<TimePhase> OnPhaseChanged;

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
    }

    void Update()
    {
        if (autoProgress)
        {
            phaseTimer += Time.deltaTime * timeScale;

            if (phaseTimer >= phaseDuration)
            {
                phaseTimer = 0f;
                NextPhase();
            }
        }
    }

    public void SetPhase(TimePhase newPhase)
    {
        if (currentPhase == newPhase) return;

        currentPhase = newPhase;
        phaseTimer = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        SaveSystem.SaveGame();
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

        // Increment day only when transitioning from Night to Morning (new day starts)
        // and not on the first morning (game start)
        if (previousPhase == TimePhase.Night && currentPhase == TimePhase.Morning)
        {
            if (!isFirstMorning && TimeUI.Instance != null)
            {
                TimeUI.Instance.IncrementDay();
            }
            isFirstMorning = false;
        }

        OnPhaseChanged?.Invoke(currentPhase);
        UpdateCameraColor(currentPhase);
        SaveSystem.SaveGame();
    }

    void UpdateCameraColor(TimePhase phase)
    {
        if (!changeScreenColor || mainCamera == null) return;

        Color targetColor = phase switch
        {
            TimePhase.Morning => new Color(1f, 0.95f, 0.85f), // Light yellow-orange
            TimePhase.Noon => new Color(1f, 1f, 1f),          // White (normal)
            TimePhase.Evening => new Color(1f, 0.8f, 0.6f),   // Orange
            TimePhase.Night => new Color(0.6f, 0.65f, 0.8f),  // Blue-purple
            _ => Color.white
        };

        mainCamera.backgroundColor = targetColor;
    }

    public float GetPhaseProgress()
    {
        return phaseTimer / phaseDuration;
    }

    public float GetTimeRemaining()
    {
        return phaseDuration - phaseTimer;
    }

    public string GetTimeRemainingFormatted()
    {
        float remaining = GetTimeRemaining();
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void SetPhaseTimer(float timer)
    {
        phaseTimer = timer;
    }

    public void SetIsFirstMorning(bool value)
    {
        isFirstMorning = value;
    }
}
