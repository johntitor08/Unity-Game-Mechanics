using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;

    [Header("Panels")]
    public GameObject questLogPanel;
    public GameObject questDetailsPanel;

    [Header("Quest Log")]
    public Transform activeQuestsParent;
    public Transform availableQuestsParent;
    public Transform completedQuestsParent;
    public QuestSlotUI questSlotPrefab;

    [Header("Quest Details")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questTypeText;
    public Image questIcon;
    public Transform objectivesParent;
    public QuestObjectiveUI objectivePrefab;
    public Transform rewardsParent;
    public Button acceptButton;
    public Button abandonButton;
    public Button trackButton;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.J;

    private QuestData selectedQuest;
    private List<QuestSlotUI> questSlots = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;
        }

        questLogPanel.SetActive(false);
        questDetailsPanel.SetActive(false);

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);

        if (abandonButton != null)
            abandonButton.onClick.AddListener(OnAbandonClicked);

        RefreshQuestLog();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            questLogPanel.SetActive(!questLogPanel.activeSelf);
            if (questLogPanel.activeSelf)
                RefreshQuestLog();
        }
    }

    void RefreshQuestLog()
    {
        // Clear existing
        foreach (var slot in questSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        questSlots.Clear();

        // Active quests
        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            var slot = Instantiate(questSlotPrefab, activeQuestsParent);
            slot.Setup(quest, true);
            questSlots.Add(slot);
        }

        // Available quests
        foreach (var quest in QuestManager.Instance.GetAvailableQuests())
        {
            var slot = Instantiate(questSlotPrefab, availableQuestsParent);
            slot.Setup(quest, false);
            questSlots.Add(slot);
        }

        // Completed quests
        foreach (var quest in QuestManager.Instance.GetCompletedQuests())
        {
            var slot = Instantiate(questSlotPrefab, completedQuestsParent);
            slot.Setup(quest, false, true);
            questSlots.Add(slot);
        }
    }

    public void ShowQuestDetails(QuestData quest)
    {
        selectedQuest = quest;
        questDetailsPanel.SetActive(true);

        // Basic info
        if (questTitleText != null)
            questTitleText.text = quest.questName;

        if (questDescriptionText != null)
            questDescriptionText.text = quest.description;

        if (questTypeText != null)
            questTypeText.text = $"{quest.questType} - {quest.difficulty}";

        if (questIcon != null && quest.icon != null)
            questIcon.sprite = quest.icon;

        // Objectives
        foreach (Transform child in objectivesParent)
            Destroy(child.gameObject);

        foreach (var objective in quest.objectives)
        {
            var objUI = Instantiate(objectivePrefab, objectivesParent);
            objUI.Setup(objective);
        }

        // Rewards
        // (Similar setup for rewards)

        // Buttons
        bool isActive = QuestManager.Instance.IsQuestActive(quest.questID);
        bool isCompleted = QuestManager.Instance.IsQuestCompleted(quest.questID);

        if (acceptButton != null)
            acceptButton.gameObject.SetActive(!isActive && !isCompleted);

        if (abandonButton != null)
            abandonButton.gameObject.SetActive(isActive);
    }

    void OnAcceptClicked()
    {
        if (selectedQuest != null)
        {
            QuestManager.Instance.StartQuest(selectedQuest);
            RefreshQuestLog();
            questDetailsPanel.SetActive(false);
        }
    }

    void OnAbandonClicked()
    {
        if (selectedQuest != null)
        {
            QuestManager.Instance.AbandonQuest(selectedQuest);
            RefreshQuestLog();
            questDetailsPanel.SetActive(false);
        }
    }

    void OnQuestStarted(QuestData quest)
    {
        RefreshQuestLog();
    }

    void OnQuestCompleted(QuestData quest)
    {
        RefreshQuestLog();
    }

    void OnObjectiveUpdated(QuestData quest, QuestObjective objective)
    {
        if (QuestTrackerUI.Instance != null)
        {
            QuestTrackerUI.Instance.UpdateTracker();
        }
    }
}
