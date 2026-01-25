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

    private QuestData quest;

    public void Setup(QuestData questData, bool isCompleted = false)
    {
        quest = questData;

        if (questNameText != null)
            questNameText.text = questData.questName;

        if (questTypeText != null)
            questTypeText.text = questData.questType.ToString();

        if (questIcon != null && questData.icon != null)
            questIcon.sprite = questData.icon;

        if (completedIndicator != null)
            completedIndicator.SetActive(isCompleted);

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
            QuestDifficulty.Easy => Color.gray,
            QuestDifficulty.Normal => Color.white,
            QuestDifficulty.Hard => Color.yellow,
            QuestDifficulty.Elite => new Color(1f, 0.5f, 0f),
            QuestDifficulty.Epic => new Color(0.8f, 0.2f, 0.8f),
            _ => Color.white
        };
    }
}
