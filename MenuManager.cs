using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    public static MenuManager Instance;
    private readonly Stack<GameObject> menuStack = new();
    private GameObject currentMenu;
    private bool isPaused = false;
    public static event Action<string> OnMenuOpened;
    public static event Action<string> OnMenuClosed;
    public static event Action OnGameStarted;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject pauseMenu;
    public GameObject loadingScreen;

    [Header("Panels")]
    public GameObject buttonPanel;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;
    public Button resumeButton;
    public Button mainMenuButton;

    [Header("Loading Screen")]
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
    }

    void Start()
    {
        SetupButtons();
        UpdateContinueButton();

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey) &&
            SceneManager.GetActiveScene().name != mainMenuSceneName)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    void SetupButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void OnNewGameClicked()
    {
        PlayButtonSound();
        buttonPanel.SetActive(false);
        
        if (SaveSystem.HasSaveFile() && ConfirmationDialog.Instance != null)
        {
            ConfirmationDialog.Instance.Show(
                "New Game",
                "This will overwrite your save. Continue?",
                StartNewGame, () =>
                {
                    if (buttonPanel != null)
                        buttonPanel.SetActive(true);
                });
        }
        else StartNewGame();
    }

    void StartNewGame()
    {
        buttonPanel.SetActive(false);
        SaveSystem.DeleteSave();
        ResetGameManagers();
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
        StartCoroutine(LoadSceneAsync("GameScene"));
    }

    public void OnContinueClicked()
    {
        PlayButtonSound();

        if (!SaveSystem.HasSaveFile())
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        Time.timeScale = 1f;
        SaveSystem.LoadGame();
    }

    public void OnSettingsClicked()
    {
        PlayButtonSound();
        ShowMenu(settingsMenu, "Settings");
    }

    public void OnQuitClicked()
    {
        PlayButtonSound();
        if (ConfirmationDialog.Instance == null) return;

        ConfirmationDialog.Instance.Show(
            "Quit Game",
            "Are you sure?",
            QuitGame,
            null
        );
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
        ShowMenu(pauseMenu, "Pause");
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;
        HideMenu(pauseMenu);
        OnGameResumed?.Invoke();
    }

    public void ReturnToMainMenu()
    {
        PlayButtonSound();
        if (ConfirmationDialog.Instance == null) return;

        ConfirmationDialog.Instance.Show(
            "Main Menu",
            "Unsaved progress will be lost.",
            () =>
            {
                Time.timeScale = 1f;
                StartCoroutine(LoadSceneAsync(mainMenuSceneName));
            },
            null
        );
    }

    void ShowMenu(GameObject menu, string name)
    {
        if (menu == null) return;

        if (currentMenu != null)
        {
            menuStack.Push(currentMenu);
            currentMenu.SetActive(false);
        }

        menu.SetActive(true);
        currentMenu = menu;
        PlaySound(menuOpenSound);
        OnMenuOpened?.Invoke(name);
    }

    void HideMenu(GameObject menu)
    {
        if (menu == null) return;
        menu.SetActive(false);

        currentMenu =
            menuStack.Count > 0 ? menuStack.Pop() : null;

        if (currentMenu != null)
            currentMenu.SetActive(true);

        PlaySound(menuCloseSound);
    }

    void HideAllMenus()
    {
        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        menuStack.Clear();
        currentMenu = null;
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        HideAllMenus();

        AsyncOperation op =
            SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingBar) loadingBar.value = p;

            if (loadingText)
                loadingText.text = $"Loading {Mathf.RoundToInt(p * 100)}%";

            yield return null;
        }

        yield return _waitForSeconds0_5;
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
        
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    void UpdateContinueButton()
    {
        if (continueButton != null)
            continueButton.interactable =
                SaveSystem.HasSaveFile();
    }

    void ResetGameManagers()
    {
        if (PlayerStats.Instance == null ||
            InventoryManager.Instance == null)
            return;

        PlayerStats.Instance.ResetAllToBase();
        InventoryManager.Instance.Clear();
        StoryFlags.flags.Clear();
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void PlayButtonSound() => PlaySound(buttonClickSound);

    void PlaySound(AudioClip clip)
    {
        if (clip && Camera.main)
            AudioSource.PlayClipAtPoint(
                clip,
                Camera.main.transform.position,
                0.5f
            );
    }

    public bool IsPaused() => isPaused;
}
