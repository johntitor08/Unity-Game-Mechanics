using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    private Resolution[] resolutions;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;

    [Header("Graphics Settings")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    [Header("Gameplay Settings")]
    public Slider difficultySlider;
    public Toggle subtitlesToggle;
    public Toggle autosaveToggle;

    [Header("Buttons")]
    public Button applyButton;
    public Button resetButton;
    public Button backButton;

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
        SetupSettings();
        LoadSettings();
    }

    void SetupSettings()
    {
        // Audio sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Graphics
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            var options = new System.Collections.Generic.List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                    currentResolutionIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        if (vsyncToggle != null)
            vsyncToggle.isOn = QualitySettings.vSyncCount > 0;

        // Buttons
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySettings);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSettings);

        if (backButton != null)
            backButton.onClick.AddListener(() => MenuManager.Instance.ResumeGame());
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnMusicVolumeChanged(float value)
    {
        // Set music volume (requires AudioMixer or audio manager)
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnSFXVolumeChanged(float value)
    {
        // Set SFX volume
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    void ApplySettings()
    {
        // Resolution
        if (resolutionDropdown != null)
        {
            Resolution resolution = resolutions[resolutionDropdown.value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        // Fullscreen
        if (fullscreenToggle != null)
            Screen.fullScreen = fullscreenToggle.isOn;

        // VSync
        if (vsyncToggle != null)
            QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;

        SaveSettings();
        Debug.Log("Settings applied!");
    }

    void ResetSettings()
    {
        // Reset to defaults
        if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 0.8f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
        if (qualityDropdown != null) qualityDropdown.value = QualitySettings.GetQualityLevel();
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;
        if (vsyncToggle != null) vsyncToggle.isOn = true;
        ApplySettings();
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider != null ? masterVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider != null ? musicVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f);
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;

        AudioListener.volume = masterVolume;
    }
}
