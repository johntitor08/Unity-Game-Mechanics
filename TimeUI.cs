using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public static TimeUI Instance;
    private int currentDay = 1;
    private Coroutine textAnimationCoroutine;
    private Coroutine iconAnimationCoroutine;

    [Header("UI Elements")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI dayCounterText;
    public Image phaseIcon;
    public Slider progressBar;

    [Header("Phase Icons")]
    public Sprite morningIcon;
    public Sprite noonIcon;
    public Sprite eveningIcon;
    public Sprite nightIcon;

    [Header("Phase Colors")]
    public Color morningColor = new(1f, 0.9f, 0.7f);
    public Color noonColor = new(1f, 1f, 0.9f);
    public Color eveningColor = new(1f, 0.6f, 0.4f);
    public Color nightColor = new(0.4f, 0.4f, 0.7f);

    [Header("Display Options")]
    public bool showDayCounter = true;
    public bool showProgressBar = true;
    public bool animateTransitions = true;

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

        if (dayCounterText != null)
            dayCounterText.gameObject.SetActive(showDayCounter);

        if (progressBar != null)
            progressBar.gameObject.SetActive(showProgressBar);
    }

    void Update()
    {
        if (showProgressBar && progressBar != null && TimePhaseManager.Instance != null)
        {
            progressBar.value = TimePhaseManager.Instance.GetPhaseProgress();
        }
    }

    void OnDestroy()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= UpdateTimeDisplay;
    }

    void UpdateTimeDisplay(TimePhase phase)
    {
        if (phaseText != null)
        {
            phaseText.text = GetPhaseName(phase);
            phaseText.color = GetPhaseColor(phase);

            if (animateTransitions)
            {
                AnimateText();
            }
        }

        if (phaseIcon != null)
        {
            phaseIcon.sprite = GetPhaseIcon(phase);

            if (animateTransitions)
            {
                AnimateIcon();
            }
        }

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
        if (phaseText == null)
            return;

        if (textAnimationCoroutine != null)
            StopCoroutine(textAnimationCoroutine);

        textAnimationCoroutine = StartCoroutine(TextScaleAnimation());
    }

    void AnimateIcon()
    {
        if (phaseIcon == null)
            return;

        if (iconAnimationCoroutine != null)
            StopCoroutine(iconAnimationCoroutine);

        phaseIcon.transform.rotation = Quaternion.identity;
        iconAnimationCoroutine = StartCoroutine(IconRotateAnimation());
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
            t = 1f - Mathf.Pow(1f - t, 3f);
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
            t = 1f - Mathf.Pow(1f - t, 2f);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            phaseIcon.transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        phaseIcon.transform.rotation = Quaternion.identity;
    }

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
