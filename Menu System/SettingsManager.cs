using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    private Resolution[] resolutions;
    static readonly int[] QualityLevels = { 1, 3, 5 };
    const int MaxFrameRateWithoutVSync = 144;
    float _snapMaster, _snapMusic, _snapSfx, _snapDiff;
    int _snapQuality, _snapLang;

    [Header("Panel")]
    public GameObject settingsPanel;
    public GameObject savePanel;

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

    [Header("Language")]
    public TMP_Dropdown languageDropdown;

    [Header("Buttons")]
    public Button savePanelButton;
    public Button applyButton;
    public Button resetButton;
    public Button closeButton;
    public Button mainMenuButton;
    public Button quitButton;

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
        LanguageManager.OnLanguageChanged += OnGameLanguageChanged;
    }

    void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnGameLanguageChanged;
    }

    void OnGameLanguageChanged(GameLanguage lang)
    {
        RefreshQualityOptions();

        if (difficultySlider != null)
            OnDifficultyChanged(difficultySlider.value);
    }

    public void OnOpenFromMenu()
    {
        BindCloseButton();
    }

    void BindCloseButton()
    {
        if (closeButton == null)
            return;

        closeButton.onClick.RemoveAllListeners();

        if (MainMenuManager.Instance != null)
            closeButton.onClick.AddListener(MainMenuManager.Instance.OnSettingsClose);
        else if (GameMenuManager.Instance != null)
            closeButton.onClick.AddListener(GameMenuManager.Instance.OnSettingsClose);
        else
            Debug.LogWarning("[SettingsManager] Back button bağlanamadı: ne MainMenuManager ne GameMenuManager sahnede var.");
    }

    void SetupDropdowns()
    {
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Türkçe" });
            languageDropdown.value = (int)LanguageManager.Current;
            languageDropdown.RefreshShownValue();
        }

        RefreshQualityOptions();

        if (resolutionDropdown != null)
        {
            var seen = new HashSet<Vector2Int>();
            var filtered = new List<Resolution>();

            foreach (var r in Screen.resolutions)
            {
                var fit = AspectLock.Fit(r.width, r.height);

                if (seen.Add(fit))
                    filtered.Add(new Resolution { width = fit.x, height = fit.y });
            }

            filtered.Sort((a, b) => a.width != b.width ? a.width.CompareTo(b.width) : a.height.CompareTo(b.height));
            var options = new List<string>();
            int currentIndex = 0;
            int bestDiff = int.MaxValue;

            for (int i = 0; i < filtered.Count; i++)
            {
                var r = filtered[i];
                options.Add($"{r.width} x {r.height}");

                int diff = Mathf.Abs(r.width - Screen.width) + Mathf.Abs(r.height - Screen.height);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    currentIndex = i;
                }
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

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (savePanelButton != null)
        {
            savePanelButton.onClick.RemoveAllListeners();
            savePanelButton.onClick.AddListener(OpenSavePanel);
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

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => { if (GameMenuManager.Instance != null) GameMenuManager.Instance.OnMainMenuClicked(); });
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => { if (GameMenuManager.Instance != null) GameMenuManager.Instance.OnQuitClicked(); });
        }
    }

    void OnMasterVolumeChanged(float value)
    {
        SetMixerVolume("Master", value);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.SetMasterVolume(value);

        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnMusicVolumeChanged(float value)
    {
        SetMixerVolume("Music", value);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.SetMusicVolume(value);

        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void OnSFXVolumeChanged(float value)
    {
        SetMixerVolume("SFX", value);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.SetSfxVolume(value);

        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.Round(value * 100) + "%";
    }

    void RefreshQualityOptions()
    {
        if (qualityDropdown == null)
            return;

        int current = qualityDropdown.value;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>
        {
            Loc.T("Low", "Düşük"),
            Loc.T("Medium", "Orta"),
            Loc.T("High", "Yüksek")
        });
        qualityDropdown.SetValueWithoutNotify(Mathf.Clamp(current, 0, QualityLevels.Length - 1));
        qualityDropdown.RefreshShownValue();
    }

    static int QualityOptionFor(int level)
    {
        int option = 0;

        for (int i = 0; i < QualityLevels.Length; i++)
            if (QualityLevels[i] <= level)
                option = i;

        return option;
    }

    void ApplyQualityOption(int option)
    {
        int level = QualityLevels[Mathf.Clamp(option, 0, QualityLevels.Length - 1)];
        QualitySettings.SetQualityLevel(Mathf.Min(level, QualitySettings.names.Length - 1), true);
        ApplyVSync(vsyncToggle != null ? vsyncToggle.isOn : QualitySettings.vSyncCount > 0);
    }

    static void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
        Application.targetFrameRate = on ? -1 : MaxFrameRateWithoutVSync;
    }

    void OnQualityChanged(int index)
    {
        ApplyQualityOption(index);
    }

    void OnLanguageChanged(int index)
    {
        LanguageManager.SetLanguage(index == 0 ? GameLanguage.EN : GameLanguage.TR);
    }

    void OnDifficultyChanged(float value)
    {
        if (difficultyText == null)
            return;

        difficultyText.text = Mathf.RoundToInt(value) switch
        {
            0 => Loc.T("Easy", "Kolay"),
            1 => Loc.T("Normal", "Normal"),
            2 => Loc.T("Hard", "Zor"),
            _ => Loc.T("Nightmare", "Kâbus")
        };
    }

    void SetMixerVolume(string parameter, float value)
    {
        if (audioMixer == null)
            return;

        float db = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameter, db);
    }

    public void OpenSavePanel()
    {
        if (settingsPanel != null && savePanel != null)
        {
            settingsPanel.SetActive(false);
            savePanel.SetActive(true);
        }
    }

    void ApplySettings()
    {
        bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;
        var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        if (resolutionDropdown != null && resolutions != null && resolutions.Length > 0)
        {
            var r = resolutions[resolutionDropdown.value];
            Screen.SetResolution(r.width, r.height, mode);
        }
        else
        {
            Screen.fullScreenMode = mode;
        }

        if (vsyncToggle != null)
            ApplyVSync(vsyncToggle.isOn);

        SaveSettings();
        CaptureSnapshot();
    }

    public void CaptureSnapshot()
    {
        _snapMaster = masterVolumeSlider != null ? masterVolumeSlider.value : 1f;
        _snapMusic = musicVolumeSlider != null ? musicVolumeSlider.value : 0.8f;
        _snapSfx = sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;
        _snapDiff = difficultySlider != null ? difficultySlider.value : 1f;
        _snapQuality = qualityDropdown != null ? qualityDropdown.value : QualityOptionFor(QualitySettings.GetQualityLevel());
        _snapLang = languageDropdown != null ? languageDropdown.value : (int)LanguageManager.Current;
    }

    public void RestoreSnapshot()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = _snapMaster;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = _snapMusic;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = _snapSfx;

        if (difficultySlider != null)
            difficultySlider.value = _snapDiff;

        if (qualityDropdown != null)
        {
            qualityDropdown.SetValueWithoutNotify(_snapQuality);
            qualityDropdown.RefreshShownValue();
            ApplyQualityOption(_snapQuality);
        }

        if (languageDropdown != null)
        {
            languageDropdown.value = _snapLang;
            LanguageManager.SetLanguage(_snapLang == 0 ? GameLanguage.EN : GameLanguage.TR);
        }
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
            qualityDropdown.value = QualityLevels.Length - 1;

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
        PlayerPrefs.SetFloat("Master", masterVolumeSlider != null ? masterVolumeSlider.value : 1f);
        PlayerPrefs.SetFloat("Music", musicVolumeSlider != null ? musicVolumeSlider.value : 0.8f);
        PlayerPrefs.SetFloat("SFX", sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f);
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle != null ? (fullscreenToggle.isOn ? 1 : 0) : (Screen.fullScreen ? 1 : 0));
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
        PlayerPrefs.SetFloat("Difficulty", difficultySlider != null ? difficultySlider.value : 1f);
        PlayerPrefs.SetInt("Subtitles", subtitlesToggle != null && subtitlesToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Autosave", autosaveToggle != null && autosaveToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("Master", 1f);
        float musicVolume = PlayerPrefs.GetFloat("Music", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFX", 1f);
        float difficulty = PlayerPrefs.GetFloat("Difficulty", 1f);

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        if (difficultySlider != null)
            difficultySlider.value = difficulty;

        SetMixerVolume("Master", masterVolume);
        SetMixerVolume("Music", musicVolume);
        SetMixerVolume("SFX", sfxVolume);

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.SetMasterVolume(masterVolume);
            GameAudioManager.Instance.SetMusicVolume(musicVolume);
            GameAudioManager.Instance.SetSfxVolume(sfxVolume);
        }

        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.Round(masterVolume * 100) + "%";

        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.Round(musicVolume * 100) + "%";

        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.Round(sfxVolume * 100) + "%";

        if (difficultySlider != null)
            OnDifficultyChanged(difficulty);

        if (vsyncToggle != null)
            vsyncToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("VSync", 1) > 0);

        int savedLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        int qualityOption = QualityOptionFor(savedLevel);

        if (qualityDropdown != null)
        {
            qualityDropdown.SetValueWithoutNotify(qualityOption);
            qualityDropdown.RefreshShownValue();
        }

        ApplyQualityOption(qualityOption);

        if (languageDropdown != null)
            languageDropdown.value = (int)LanguageManager.Current;

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);

        if (subtitlesToggle != null)
            subtitlesToggle.isOn = PlayerPrefs.GetInt("Subtitles", 1) == 1;

        if (autosaveToggle != null)
            autosaveToggle.isOn = PlayerPrefs.GetInt("Autosave", 1) == 1;
    }

    public void ClosePanel() => UIPanelAnimator.Hide(settingsPanel);

    public bool IsAutosaveEnabled() => autosaveToggle != null && autosaveToggle.isOn;

    public bool IsSubtitlesEnabled() => subtitlesToggle != null && subtitlesToggle.isOn;

    public float GetDifficulty() => difficultySlider != null ? difficultySlider.value : 1f;
}
