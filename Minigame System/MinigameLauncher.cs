using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLauncher : MonoBehaviour
{
    public const string PuzzleSceneName = "PuzzleGame";
    public const string DrawingSceneName = "DrawingApp";
    public static MinigameLauncher Instance { get; private set; }
    private bool _busy;
    private bool _inMinigame;
    private string _activeMinigameScene;
    private string _gameSceneName;
    private readonly List<GameObject> _hiddenRoots = new();
    private TimePhaseManager _pausedTimePhase;
    private bool _timePhaseWasEnabled;
    public bool InMinigame => _inMinigame;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LaunchPuzzle() => Launch(PuzzleSceneName);

    public void LaunchDrawing() => Launch(DrawingSceneName);

    public void Launch(string minigameScene)
    {
        if (_busy || _inMinigame)
            return;

        StartCoroutine(LaunchRoutine(minigameScene));
    }

    public void ReturnToGame()
    {
        if (_busy || !_inMinigame)
            return;

        StartCoroutine(ReturnRoutine());
    }

    IEnumerator LaunchRoutine(string minigameScene)
    {
        _busy = true;
        Scene gameScene = SceneManager.GetActiveScene();
        _gameSceneName = gameScene.name;
        _hiddenRoots.Clear();

        foreach (GameObject root in gameScene.GetRootGameObjects())
        {
            if (root == gameObject)
                continue;

            if (root.activeSelf)
            {
                _hiddenRoots.Add(root);
                root.SetActive(false);
            }
        }

        _pausedTimePhase = FindAnyObjectByType<TimePhaseManager>();

        if (_pausedTimePhase != null)
        {
            _timePhaseWasEnabled = _pausedTimePhase.enabled;
            _pausedTimePhase.enabled = false;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(minigameScene, LoadSceneMode.Additive);

        if (op == null)
        {
            Debug.LogError($"[MinigameLauncher] Scene not found in Build Settings: {minigameScene}. Restoring game.");
            RestoreGameRoots();
            _busy = false;
            yield break;
        }

        yield return op;
        Scene loaded = SceneManager.GetSceneByName(minigameScene);

        if (loaded.IsValid())
            SceneManager.SetActiveScene(loaded);

        _activeMinigameScene = minigameScene;
        _inMinigame = true;
        _busy = false;
    }

    IEnumerator ReturnRoutine()
    {
        _busy = true;

        if (!string.IsNullOrEmpty(_activeMinigameScene))
        {
            Scene mg = SceneManager.GetSceneByName(_activeMinigameScene);

            if (mg.IsValid() && mg.isLoaded)
            {
                AsyncOperation op = SceneManager.UnloadSceneAsync(mg);

                if (op != null)
                    yield return op;
            }
        }

        RestoreGameRoots();
        Scene gameScene = SceneManager.GetSceneByName(_gameSceneName);

        if (gameScene.IsValid())
            SceneManager.SetActiveScene(gameScene);

        _activeMinigameScene = null;
        _inMinigame = false;
        _busy = false;
    }

    void RestoreGameRoots()
    {
        foreach (GameObject root in _hiddenRoots)
        {
            if (root != null)
                root.SetActive(true);
        }

        _hiddenRoots.Clear();

        if (_pausedTimePhase != null)
        {
            _pausedTimePhase.enabled = _timePhaseWasEnabled;
            _pausedTimePhase = null;
        }
    }
}
