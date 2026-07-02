using UnityEngine;
using TMPro;

public class PuzzleRewarder : MonoBehaviour
{
    [Header("Reward (Gold) per difficulty")]
    [SerializeField] private int easyReward = 25;
    [SerializeField] private int mediumReward = 50;
    [SerializeField] private int hardReward = 100;

    [Header("Win panel label (optional)")]
    [SerializeField] private TextMeshProUGUI rewardText;

    private Difficulty _difficulty = Difficulty.Medium;

    void OnEnable()
    {
        PuzzleEvents.OnDifficultyChanged += OnDifficultyChanged;
        PuzzleEvents.OnPuzzleCompleted += OnPuzzleCompleted;
    }

    void OnDisable()
    {
        PuzzleEvents.OnDifficultyChanged -= OnDifficultyChanged;
        PuzzleEvents.OnPuzzleCompleted -= OnPuzzleCompleted;
    }

    void OnDifficultyChanged(Difficulty difficulty) => _difficulty = difficulty;

    void OnPuzzleCompleted()
    {
        int amount = _difficulty switch
        {
            Difficulty.Easy => easyReward,
            Difficulty.Hard => hardReward,
            _ => mediumReward
        };

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.Add(CurrencyType.Gold, amount, showNotification: false);
        else
            Debug.LogWarning("[PuzzleRewarder] CurrencyManager not found; reward not granted.");

        if (rewardText != null)
            rewardText.text = $"+{amount} {Loc.T("Gold", "Altın")}";
    }
}
