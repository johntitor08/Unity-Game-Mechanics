using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeDebugPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject debugPanel;

    [Header("Buttons")]
    public Button morningButton;
    public Button noonButton;
    public Button eveningButton;
    public Button nightButton;
    public Button nextPhaseButton;

    [Header("Speed Control")]
    public Slider speedSlider;
    public TextMeshProUGUI speedText;

    [Header("Info Display")]
    public TextMeshProUGUI currentPhaseText;
    public TextMeshProUGUI timeRemainingText;

    void Start()
    {
        if (debugPanel != null)
            debugPanel.SetActive(false);

        // Bind button events
        if (morningButton != null)
            morningButton.onClick.AddListener(() => SetPhase(TimePhase.Morning));

        if (noonButton != null)
            noonButton.onClick.AddListener(() => SetPhase(TimePhase.Noon));

        if (eveningButton != null)
            eveningButton.onClick.AddListener(() => SetPhase(TimePhase.Evening));

        if (nightButton != null)
            nightButton.onClick.AddListener(() => SetPhase(TimePhase.Night));

        if (nextPhaseButton != null)
            nextPhaseButton.onClick.AddListener(NextPhase);

        // Slider event
        if (speedSlider != null)
        {
            speedSlider.minValue = 0.1f;
            speedSlider.maxValue = 10f;
            speedSlider.value = 1f;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }
    }

    void Update()
    {
        // Toggle panel with T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (debugPanel != null)
                debugPanel.SetActive(!debugPanel.activeSelf);
        }

        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetPhase(TimePhase.Morning);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetPhase(TimePhase.Noon);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetPhase(TimePhase.Evening);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetPhase(TimePhase.Night);
        if (Input.GetKeyDown(KeyCode.Space)) NextPhase();

        // Update debug info
        UpdateDebugInfo();
    }

    void UpdateDebugInfo()
    {
        if (TimePhaseManager.Instance == null) return;

        if (currentPhaseText != null)
        {
            currentPhaseText.text = "Current Phase: " + TimePhaseManager.Instance.currentPhase.ToString();
        }

        if (timeRemainingText != null)
        {
            timeRemainingText.text = "Time Remaining: " + TimePhaseManager.Instance.GetTimeRemainingFormatted();
        }
    }

    void SetPhase(TimePhase phase)
    {
        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.SetPhase(phase);
        }
    }

    void NextPhase()
    {
        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.NextPhase();
        }
    }

    void OnSpeedChanged(float value)
    {
        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.timeScale = value;

            if (speedText != null)
            {
                speedText.text = $"Speed: {value:F1}x";
            }
        }
    }
}
