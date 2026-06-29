using UnityEngine;

public class MinigameExitButton : MonoBehaviour
{
    [SerializeField] private bool allowEscapeKey = true;

    void Update()
    {
        if (allowEscapeKey && Input.GetKeyDown(KeyCode.Escape))
            ExitToGame();
    }

    public void ExitToGame()
    {
        if (MinigameLauncher.Instance != null && MinigameLauncher.Instance.InMinigame)
            MinigameLauncher.Instance.ReturnToGame();
        else
            Debug.Log("[MinigameExitButton] No active MinigameLauncher session (standalone play?).");
    }
}
