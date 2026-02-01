using UnityEngine;

public class PuzzleMainMenu : MonoBehaviour
{
    public void Easy()
    {
        PuzzleEvents.OnDifficultyChanged?.Invoke(Difficulty.Easy);
    }

    public void Medium()
    {
        PuzzleEvents.OnDifficultyChanged?.Invoke(Difficulty.Medium);
    }

    public void Hard()
    {
        PuzzleEvents.OnDifficultyChanged?.Invoke(Difficulty.Hard);
    }

    public void Play()
    {
        PuzzleEvents.OnPuzzleRestarted?.Invoke();
    }
}
