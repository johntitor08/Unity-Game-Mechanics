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

        foreach (Transform child in objectivesParent)
            Destroy(child.gameObject);

        foreach (var objective in quest.objectives)
        {
            var state = QuestManager.Instance.GetObjectiveState(quest.questID, objective.objectiveID);

            if (state.isCompleted)
                continue;

            var objText = Instantiate(objectiveTextPrefab, objectivesParent);
            objText.text = $"• {objective.description} ({state.currentProgress}/{objective.GetRequiredCount()})";
        }
    }
}
