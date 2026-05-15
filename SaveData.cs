using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestTrackerEntry : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questNameText;
    public Transform objectivesParent;
    public TextMeshProUGUI objectiveTextPrefab;

    private readonly List<TextMeshProUGUI> objectiveTexts = new();

    public void Setup(QuestData quest)
    {
        if (QuestManager.Instance == null)
            return;

        if (questNameText != null)
            questNameText.text = quest.questName;

        var incompleteObjectives = new List<(QuestObjective obj, ObjectiveRuntimeState state)>();

        foreach (var objective in quest.objectives)
        {
            var state = QuestManager.Instance.GetObjectiveState(quest.questID, objective.objectiveID);

            if (!state.isCompleted)
                incompleteObjectives.Add((objective, state));
        }

        while (objectiveTexts.Count < incompleteObjectives.Count)
        {
            var t = Instantiate(objectiveTextPrefab, objectivesParent);
            objectiveTexts.Add(t);
        }

        for (int i = 0; i < objectiveTexts.Count; i++)
        {
            if (i < incompleteObjectives.Count)
            {
                var (obj, state) = incompleteObjectives[i];
                objectiveTexts[i].text = $"• {obj.description} ({state.currentProgress}/{obj.GetRequiredCount()})";
                objectiveTexts[i].gameObject.SetActive(true);
            }
            else
            {
                objectiveTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
