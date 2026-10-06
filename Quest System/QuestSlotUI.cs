using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questTypeText;
    public Image questIcon;
    public Image difficultyIcon;
    public Button detailsButton;
    public GameObject completedIndicator;
    public GameObject newIndicator;
    public Sprite fallbackQuestIcon;

    private QuestData quest;

    public void Setup(QuestData questData, bool isCompleted = false, bool isNew = false)
    {
        quest = questData;

        if (questNameText != null)
            questNameText.text = questData.DisplayName;

        if (questTypeText != null)
            questTypeText.text = questData.questType.Display();

        if (questIcon != null)
        {
            questIcon.sprite = questData.icon != null ? questData.icon : fallbackQuestIcon;
            questIcon.preserveAspect = true;
            questIcon.enabled = questIcon.sprite != null;
        }

        if (completedIndicator != null)
            completedIndicator.SetActive(isCompleted);

        if (newIndicator != null)
            newIndicator.SetActive(isNew);

        if (difficultyIcon != null)
            difficultyIcon.color = GetDifficultyColor(questData.difficulty);

        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(OnDetailsClicked);
        }
    }

    void OnDetailsClicked()
    {
        if (QuestUI.Instance != null && quest != null)
            QuestUI.Instance.ShowQuestDetails(quest);
    }

    Color GetDifficultyColor(QuestDifficulty difficulty)
    {
        return difficulty switch
        {
            QuestDifficulty.Easy => UIPalette.Muted,
            QuestDifficulty.Normal => UIPalette.Cream,
            QuestDifficulty.Hard => UIPalette.Gold,
            QuestDifficulty.Elite => UIPalette.Legendary,
            QuestDifficulty.Epic => UIPalette.Epic,
            _ => UIPalette.Cream
        };
    }
}
