using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Win Panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winMovesText;
    [SerializeField] private TextMeshProUGUI winTimeText;

    [SerializeField] private PuzzleImageSelector imageSelector;
    private int moves;
    private float startTime;
    private bool timerRunning;

    void OnEnable()
    {
        PuzzleEvents.OnMoveMade += IncrementMoves;
        PuzzleEvents.OnPuzzleCompleted += ShowWinPanel;
        PuzzleEvents.OnPuzzleRestarted += ResetUI;
        PuzzleEvents.OnPuzzleStarted += StartTimer;
    }

    void OnDisable()
    {
        PuzzleEvents.OnMoveMade -= IncrementMoves;
        PuzzleEvents.OnPuzzleCompleted -= ShowWinPanel;
        PuzzleEvents.OnPuzzleRestarted -= ResetUI;
        PuzzleEvents.OnPuzzleStarted -= StartTimer;
    }

    void Update()
    {
        if (!timerRunning) return;
        float elapsed = Time.time - startTime;
        int min = Mathf.FloorToInt(elapsed / 60f);
        int sec = Mathf.FloorToInt(elapsed % 60f);
        timeText.text = $"Süre: {min:00}:{sec:00}";
    }

    void StartTimer()
    {
        startTime = Time.time;
        timerRunning = true;
    }

    public void IncrementMoves()
    {
        moves++;
        movesText.text = $"Hamle: {moves}";
    }

    void ShowWinPanel()
    {
        timerRunning = false;
        winPanel.SetActive(true);
        winMovesText.text = $"Hamle: {moves}";
        winTimeText.text = timeText.text;
    }

    public void ResetUI()
    {
        moves = 0;
        startTime = Time.time;
        timerRunning = true;
        movesText.text = "Hamle: 0";
        timeText.text = "Süre: 00:00";

        if (winPanel)
            winPanel.SetActive(false);
    }

    public void Replay()
    {
        ResetUI();

        if (imageSelector != null && imageSelector.GetImageCount() > 0)
            imageSelector.SelectNewRandomImage();
        else
            PuzzleEvents.OnPuzzleRestarted?.Invoke();
    }
}
