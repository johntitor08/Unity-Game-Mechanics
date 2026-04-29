using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    private static readonly WaitForSeconds _halfSecondWait = new(0.5f);
    public static GameMenuManager Instance { get; private set; }
    private bool _isPaused;
    private AudioSource _audioSource;
    public static event System.Action OnGamePaused;
    public static event System.Action OnGameResumed;

    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingBar;
    public TMPro.TextMeshProUGUI loadingText;

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public KeyCode pauseKey = KeyCode.Escape;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        SetupButtons();

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void SetupButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void PauseGame()
    {
        if (_isPaused)
            return;

        _isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        PlaySound(menuOpenSound);
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!_isPaused)
            return;

        _isPaused = false;
        Time.timeScale = 1f;

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        PlaySound(menuCloseSound);
        OnGameResumed?.Invoke();
    }

    public void OnSettingsClicked()
    {
        PlayButtonSound();
        PlaySound(menuOpenSound);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void OnSettingsBack()
    {
        PlaySound(menuCloseSound);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void OnMainMenuClicked()
    {
        PlayButtonSound();

        if (ConfirmationDialog.Instance != null)
        {
            ConfirmationDialog.Instance.Show("Main Menu", "Unsaved progress will be lost.", ReturnToMainMenu, null);
        }
        else
        {
            ReturnToMainMenu();
        }
    }

    void ReturnToMainMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAsync(mainMenuSceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            Debug.LogError($"[GameMenuManager] Scene not found: {sceneName}");

            if (loadingScreen != null)
                loadingScreen.SetActive(false);

            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);

            if (loadingBar != null)
                loadingBar.value = p;

            if (loadingText != null)
                loadingText.text = $"Loading {Mathf.RoundToInt(p * 100)}%";

            yield return null;
        }

        yield return _halfSecondWait;
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public bool IsPaused() => _isPaused;

    void PlayButtonSound() => PlaySound(buttonClickSound);

    void PlaySound(AudioClip clip)
    {
        if (clip == null || _audioSource == null)
            return;

        _audioSource.PlayOneShot(clip, 0.5f);
    }
}
