using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static readonly WaitForSeconds _halfSecondWait = new(0.5f);
    public static MainMenuManager Instance { get; private set; }
    private AudioSource _audioSource;

    [Header("Panels")]
    public GameObject buttonPanel;
    public GameObject settingsMenu;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingBar;
    public TMPro.TextMeshProUGUI loadingText;

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;

    [Header("Settings")]
    public string gameSceneName = "GameScene";

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
        UpdateContinueButton();

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);
    }

    void SetupButtons()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    public void OnNewGameClicked()
    {
        PlayButtonSound();

        if (SaveSystem.HasSaveFile())
        {
            if (ConfirmationDialog.Instance != null)
            {
                ConfirmationDialog.Instance.Show(
                    "New Game",
                    "This will overwrite your save. Continue?",
                    StartNewGame,
                    () => SetButtonPanel(true));
            }
            else
            {
                StartNewGame();
            }
        }
        else
        {
            StartNewGame();
        }
    }

    public void OnContinueClicked()
    {
        PlayButtonSound();

        if (!SaveSystem.HasSaveFile())
        {
            Debug.LogWarning("[MainMenuManager] No save file found.");
            return;
        }

        Time.timeScale = 1f;
        SaveSystem.LoadGame();
        UpdateContinueButton();
    }

    public void OnSettingsClicked()
    {
        PlayButtonSound();
        PlaySound(menuOpenSound);

        SetButtonPanel(false);
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void OnSettingsBack()
    {
        PlaySound(menuCloseSound);

        if (settingsMenu != null)
            settingsMenu.SetActive(false);

        SetButtonPanel(true);
    }

    public void OnQuitClicked()
    {
        PlayButtonSound();

        if (ConfirmationDialog.Instance != null)
            ConfirmationDialog.Instance.Show("Quit Game", "Are you sure?", QuitGame, null);
        else
            QuitGame();
    }

    void StartNewGame()
    {
        SetButtonPanel(false);
        ResetGameManagers();
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAsync(gameSceneName));
    }

    void UpdateContinueButton()
    {
        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSaveFile();
    }

    void SetButtonPanel(bool active)
    {
        if (buttonPanel != null)
            buttonPanel.SetActive(active);
    }

    void ResetGameManagers()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ResetAllToBase();
        else
            Debug.LogWarning("[MainMenuManager] PlayerStats.Instance is null — skipping reset.");

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.Clear();
        else
            Debug.LogWarning("[MainMenuManager] InventoryManager.Instance is null — skipping reset.");

        StoryFlags.Reset();
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            Debug.LogError($"[MainMenuManager] Scene not found: {sceneName}");
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

    void QuitGame() => Application.Quit();

    void PlayButtonSound() => PlaySound(buttonClickSound);

    void PlaySound(AudioClip clip)
    {
        if (clip == null || _audioSource == null)
            return;

        _audioSource.PlayOneShot(clip, 0.5f);
    }
}
