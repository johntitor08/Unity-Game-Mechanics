using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestObjectiveUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Image checkmarkIcon;

    public void Setup(QuestObjective objective, ObjectiveRuntimeState state)
    {
        if (descriptionText != null)
            descriptionText.text = objective.description;

        UpdateProgress(objective, state);
    }

    public void UpdateProgress(QuestObjective objective, ObjectiveRuntimeState state)
    {
        int required = objective.GetRequiredCount();

        if (progressText != null)
            progressText.text = $"{state.currentProgress}/{required}";

        if (progressBar != null)
        {
            float pct = required == 0 ? 1f : Mathf.Clamp01((float)state.currentProgress / required);
            progressBar.value = pct;
        }

        if (checkmarkIcon != null)
            checkmarkIcon.gameObject.SetActive(state.isCompleted);
    }
}
