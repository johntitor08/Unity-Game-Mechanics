using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    private Resolution[] resolutions;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;

    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    [Header("Gameplay")]
    public Slider difficultySlider;
    public TextMeshProUGUI difficultyText;
    public Toggle subtitlesToggle;
    public Toggle autosaveToggle;

    [Header("Buttons")]
    public Button applyButton;
    public Button resetButton;
    public Button backButton;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetupDropdowns();
        LoadSettings();
        BindListeners();
        BindBackButton();
    }

    public void OnOpen()
    {
        BindBackButton();
    }

    void BindBackButton()
    {
        if (backButton == null)
            return;

        backButton.onClick.RemoveAllListeners();

        if (MainMenuManager.Instance != null)
            backButton.onClick.AddListener(MainMenuManager.Instance.OnSettingsBack);
        else if (GameMenuManager.Instance != null)
            backButton.onClick.AddListener(GameMenuManager.Instance.OnSettingsBack);
        else
            Debug.LogWarning("[SettingsManager] Back button bağlanamadı: ne MainMenuManager ne GameMenuManager sahnede var.");
    }

    void SetupDropdowns()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        }

        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            var options = new List<string>();
            var seen = new HashSet<string>();
            var filtered = new List<Resolution>();
            int currentIndex = 0;

            foreach (var r in resolutions)
            {
                string key = $"{r.width}x{r.height}";

                if (seen.Contains(key))
                    continue;

                seen.Add(key);
                filtered.Add(r);
                options.Add($"{r.width} x {r.height}");

                if (r.width == Screen.width && r.height == Screen.height)
                    currentIndex = filtered.Count - 1;
            }

            resolutions = filtered.ToArray();
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    void BindListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (difficultySlider != null)
        {
            difficultySlider.minValue = 0;
            difficultySlider.maxValue = 3;
            difficultySlider.wholeNumbers = true;
            difficultySlider.onValueChanged.RemoveAllListeners();
            difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
        }

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(ApplySettings);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetSettings);
        }
    }

    void OnMasterVolumeChanged(float value)
    {
        SetMixerVolume("MasterVolume", value);

        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnMusicVolumeChanged(float value)
    {
        SetMixerVolume("MusicVolume", value);

        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnSFXVolumeChanged(float value)
    {
        SetMixerVolume("SFXVolume", value);

        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    void OnDifficultyChanged(float value)
    {
        if (difficultyText == null)
            return;

        difficultyText.text = Mathf.RoundToInt(value) switch
        {
            0 => "Easy",
            1 => "Normal",
            2 => "Hard",
            _ => "Nightmare"
        };
    }

    void ApplySettings()
    {
        if (resolutionDropdown != null && resolutions != null && resolutions.Length > 0)
        {
            var r = resolutions[resolutionDropdown.value];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        }

        if (fullscreenToggle != null)
            Screen.fullScreen = fullscreenToggle.isOn;

        if (vsyncToggle != null)
            QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;

        SaveSettings();
    }

    void ResetSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = 1f;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = 0.8f;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = 1f;

        if (qualityDropdown != null)
            qualityDropdown.value = QualitySettings.GetQualityLevel();

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = true;

        if (vsyncToggle != null)
            vsyncToggle.isOn = true;

        if (difficultySlider != null)
            difficultySlider.value = 1f;

        if (subtitlesToggle != null)
            subtitlesToggle.isOn = true;

        if (autosaveToggle != null)
            autosaveToggle.isOn = true;

        ApplySettings();
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider != null ? masterVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider != null ? musicVolumeSlider.value : 0.8f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f);
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
        PlayerPrefs.SetFloat("Difficulty", difficultySlider != null ? difficultySlider.value : 1f);
        PlayerPrefs.SetInt("Subtitles", subtitlesToggle != null && subtitlesToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Autosave", autosaveToggle != null && autosaveToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float difficulty = PlayerPrefs.GetFloat("Difficulty", 1f);

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        if (difficultySlider != null)
            difficultySlider.value = difficulty;

        SetMixerVolume("MasterVolume", masterVolume);
        SetMixerVolume("MusicVolume", musicVolume);
        SetMixerVolume("SFXVolume", sfxVolume);

        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.Round(masterVolume * 100) + "%";

        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.Round(musicVolume * 100) + "%";

        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.Round(sfxVolume * 100) + "%";

        if (difficultySlider != null)
            OnDifficultyChanged(difficulty);

        if (qualityDropdown != null)
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (vsyncToggle != null)
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) > 0;

        if (subtitlesToggle != null)
            subtitlesToggle.isOn = PlayerPrefs.GetInt("Subtitles", 1) == 1;

        if (autosaveToggle != null)
            autosaveToggle.isOn = PlayerPrefs.GetInt("Autosave", 1) == 1;
    }

    void SetMixerVolume(string parameter, float value)
    {
        if (audioMixer == null)
            return;

        float db = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameter, db);
    }

    public bool IsAutosaveEnabled() => autosaveToggle != null && autosaveToggle.isOn;

    public bool IsSubtitlesEnabled() => subtitlesToggle != null && subtitlesToggle.isOn;

    public float GetDifficulty() => difficultySlider != null ? difficultySlider.value : 1f;
}
