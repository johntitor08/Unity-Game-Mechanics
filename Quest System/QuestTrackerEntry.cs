using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestTrackerEntry : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questNameText;
    public RectTransform objectivesContainer;
    public TextMeshProUGUI objectiveTextPrefab;
    public ScrollRect scrollRect;

    private readonly List<TextMeshProUGUI> objectiveTexts = new();
    private readonly int maxVisibleRows = 4;
    private readonly int objectiveTextHeight = 50;

    public void Setup(QuestData quest)
    {
        if (QuestManager.Instance == null)
            return;

        if (questNameText != null)
            questNameText.text = quest.DisplayName;

        var incompleteObjectives = new List<(QuestObjective obj, ObjectiveRuntimeState state)>();

        foreach (var objective in quest.objectives)
        {
            var state = QuestManager.Instance.GetObjectiveState(quest.questID, objective.objectiveID);

            if (!state.isCompleted)
                incompleteObjectives.Add((objective, state));
        }

        int count = incompleteObjectives.Count;

        while (objectiveTexts.Count < count && objectiveTextPrefab != null && objectivesContainer != null)
        {
            var t = Instantiate(objectiveTextPrefab, objectivesContainer);
            objectiveTexts.Add(t);
        }

        for (int i = 0; i < objectiveTexts.Count; i++)
        {
            if (i < count)
            {
                var (obj, state) = incompleteObjectives[i];
                var textComp = objectiveTexts[i];
                textComp.text = $"- {obj.DisplayDescription} ({state.currentProgress}/{obj.GetRequiredCount()})";
                textComp.gameObject.SetActive(true);
            }
            else
            {
                objectiveTexts[i].gameObject.SetActive(false);
            }
        }

        float totalContentHeight = count * objectiveTextHeight;
        bool needsScroll = count > maxVisibleRows;
        float visibleAreaHeight = needsScroll ? maxVisibleRows * objectiveTextHeight : totalContentHeight;
        UpdateScrollRect(visibleAreaHeight, needsScroll);
        ResizeSelf(count);
    }

    private void UpdateScrollRect(float visibleAreaHeight, bool needsScroll)
    {
        if (scrollRect == null)
            return;

        scrollRect.horizontal = false;
        scrollRect.vertical = needsScroll;
        scrollRect.verticalScrollbar = needsScroll ? scrollRect.verticalScrollbar : null;
        scrollRect.verticalNormalizedPosition = 1f;

        if (scrollRect.TryGetComponent<RectTransform>(out var scrollRt))
            scrollRt.sizeDelta = new Vector2(scrollRt.sizeDelta.x, visibleAreaHeight);
    }

    private void ResizeSelf(int objectiveCount)
    {
        if (!TryGetComponent<RectTransform>(out var selfRt))
            return;

        if (objectiveCount > maxVisibleRows)
        {
            selfRt.sizeDelta = new Vector2(selfRt.sizeDelta.x, maxVisibleRows * objectiveTextHeight + 100f);
        }
        else
        {
            selfRt.sizeDelta = new Vector2(selfRt.sizeDelta.x, objectiveCount * objectiveTextHeight + 100f);
        }
    }
}
