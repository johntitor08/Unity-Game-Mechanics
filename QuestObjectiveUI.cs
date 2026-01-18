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

    public void Setup(QuestObjective objective)
    {
        if (descriptionText != null)
            descriptionText.text = objective.description;

        UpdateProgress(objective);
    }

    public void UpdateProgress(QuestObjective objective)
    {
        if (progressText != null)
        {
            progressText.text = $"{objective.currentProgress}/{objective.GetRequiredCount()}";
        }

        if (progressBar != null)
        {
            progressBar.value = objective.GetProgressPercentage();
        }

        if (checkmarkIcon != null)
        {
            checkmarkIcon.gameObject.SetActive(objective.isCompleted);
        }
    }
}
