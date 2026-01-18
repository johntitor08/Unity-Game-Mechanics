using UnityEngine;
using TMPro;

public class QuestTrackerEntry : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questNameText;
    public Transform objectivesParent;
    public TextMeshProUGUI objectiveTextPrefab;

    public void Setup(QuestData quest)
    {
        if (questNameText != null)
            questNameText.text = quest.questName;

        // Clear objectives
        foreach (Transform child in objectivesParent)
            Destroy(child.gameObject);

        // Add objectives
        foreach (var objective in quest.objectives)
        {
            if (objective.isCompleted) continue;

            var objText = Instantiate(objectiveTextPrefab, objectivesParent);
            objText.text = $"• {objective.description} ({objective.currentProgress}/{objective.GetRequiredCount()})";
        }
    }
}
