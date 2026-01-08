using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public static TimeUI Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI dayCounterText;
    public Image phaseIcon;
    public Slider progressBar;

    [Header("Phase Icons (4 Sprites Required)")]
    public Sprite morningIcon;     // Morning Icon
    public Sprite noonIcon;        // Noon Icon
    public Sprite eveningIcon;     // Evening Icon
    public Sprite nightIcon;       // Night Icon

    [Header("Phase Colors")]
    public Color morningColor = new Color(1f, 0.9f, 0.7f);      // Light yellow
    public Color noonColor = new Color(1f, 1f, 0.9f);           // Whitish yellow
    public Color eveningColor = new Color(1f, 0.6f, 0.4f);      // Orange
    public Color nightColor = new Color(0.4f, 0.4f, 0.7f);      // Blue

    [Header("Display Options")]
    public bool showDayCounter = true;
    public bool showProgressBar = true;
    public bool animateTransitions = true;

    private int currentDay = 1;
    private Coroutine animationCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.OnPhaseChanged += UpdateTimeDisplay;
            UpdateTimeDisplay(TimePhaseManager.Instance.currentPhase);
        }

        // Display settings
        if (dayCounterText != null)
            dayCounterText.gameObject.SetActive(showDayCounter);

        if (progressBar != null)
            progressBar.gameObject.SetActive(showProgressBar);
    }

    void OnDestroy()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= UpdateTimeDisplay;
    }

    void Update()
    {
        // Update progress bar
        if (showProgressBar && progressBar != null && TimePhaseManager.Instance != null)
        {
            progressBar.value = TimePhaseManager.Instance.GetPhaseProgress();
        }
    }

    void UpdateTimeDisplay(TimePhase phase)
    {
        // Update text
        if (phaseText != null)
        {
            phaseText.text = GetPhaseName(phase);
            phaseText.color = GetPhaseColor(phase);

            if (animateTransitions)
            {
                AnimateText();
            }
        }

        // Update icon
        if (phaseIcon != null)
        {
            phaseIcon.sprite = GetPhaseIcon(phase);

            if (animateTransitions)
            {
                AnimateIcon();
            }
        }

        // Note: Day increment is handled in TimePhaseManager's NextPhase
        // to avoid incrementing on game start
        UpdateDayCounter();
    }

    void UpdateDayCounter()
    {
        if (dayCounterText != null && showDayCounter)
        {
            dayCounterText.text = "Day " + currentDay;
        }
    }

    string GetPhaseName(TimePhase phase)
    {
        return phase switch
        {
            TimePhase.Morning => "Morning",
            TimePhase.Noon => "Noon",
            TimePhase.Evening => "Evening",
            TimePhase.Night => "Night",
            _ => "Unknown"
        };
    }

    Sprite GetPhaseIcon(TimePhase phase)
    {
        return phase switch
        {
            TimePhase.Morning => morningIcon,
            TimePhase.Noon => noonIcon,
            TimePhase.Evening => eveningIcon,
            TimePhase.Night => nightIcon,
            _ => null
        };
    }

    Color GetPhaseColor(TimePhase phase)
    {
        return phase switch
        {
            TimePhase.Morning => morningColor,
            TimePhase.Noon => noonColor,
            TimePhase.Evening => eveningColor,
            TimePhase.Night => nightColor,
            _ => Color.white
        };
    }

    void AnimateText()
    {
        if (phaseText == null) return;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(TextScaleAnimation());
    }

    void AnimateIcon()
    {
        if (phaseIcon == null) return;

        StartCoroutine(IconRotateAnimation());
    }

    System.Collections.IEnumerator TextScaleAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 1.5f;
        Vector3 endScale = Vector3.one;

        phaseText.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out
            phaseText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        phaseText.transform.localScale = endScale;
    }

    System.Collections.IEnumerator IconRotateAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float startAngle = 360f;
        float endAngle = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 2f); // Ease out
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            phaseIcon.transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        phaseIcon.transform.rotation = Quaternion.identity;
    }

    // Public methods
    public void SetDay(int day)
    {
        currentDay = day;
        UpdateDayCounter();
    }

    public int GetCurrentDay() => currentDay;

    public void IncrementDay()
    {
        currentDay++;
        UpdateDayCounter();
    }
}
