using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Graphic))]
public class MinigameLaunchButton : MonoBehaviour, IPointerClickHandler
{
    public enum Target { Puzzle, Drawing }
    [SerializeField] private Target target = Target.Puzzle;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MinigameLauncher.Instance == null)
        {
            Debug.LogWarning("[MinigameLaunchButton] No MinigameLauncher in scene.");
            return;
        }

        if (MinigameLauncher.Instance.InMinigame)
            return;

        switch (target)
        {
            case Target.Puzzle:
                MinigameLauncher.Instance.LaunchPuzzle();
                break;

            case Target.Drawing:
                MinigameLauncher.Instance.LaunchDrawing();
                break;
        }
    }
}
