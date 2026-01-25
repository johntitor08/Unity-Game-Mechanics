using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;
    private QuestData selectedQuest;
    private readonly List<QuestSlotUI> questSlots = new();
    private bool isSubscribed = false;

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

    void Awake() => Instance = this;

    void Start()
    {
        questLogPanel.SetActive(false);
        questDetailsPanel.SetActive(false);

        if (acceptButton != null && abandonButton != null)
        {
            acceptButton.gameObject.SetActive(false);
            abandonButton.gameObject.SetActive(false);
        }

        TrySubscribe();
    }

    void OnEnable()
    {
        if (QuestManager.Instance == null)
            QuestManager.OnReady += TrySubscribe;
    }

    void OnDisable()
    {
        if (isSubscribed && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated -= OnObjectiveUpdated;
            isSubscribed = false;
        }

        QuestManager.OnReady -= TrySubscribe;
    }

    void TrySubscribe()
    {
        if (QuestManager.Instance != null && !isSubscribed)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;
            isSubscribed = true;
            RefreshQuestLog();
        }
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
        foreach (var slot in questSlots) if (slot != null) Destroy(slot.gameObject);
        questSlots.Clear();

        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            var slot = Instantiate(questSlotPrefab, activeQuestsParent);
            slot.Setup(quest);
            questSlots.Add(slot);
        }

        foreach (var quest in QuestManager.Instance.GetAvailableQuests())
        {
            var slot = Instantiate(questSlotPrefab, availableQuestsParent);
            slot.Setup(quest);
            questSlots.Add(slot);
        }

        foreach (var quest in QuestManager.Instance.GetCompletedQuests())
        {
            var slot = Instantiate(questSlotPrefab, completedQuestsParent);
            slot.Setup(quest, true);
            questSlots.Add(slot);
        }
    }

    public void ShowQuestDetails(QuestData quest)
    {
        selectedQuest = quest;
        questDetailsPanel.SetActive(true);
        questTitleText.text = quest.questName;
        questDescriptionText.text = quest.description;
        questTypeText.text = $"{quest.questType} - {quest.difficulty}";
        if (questIcon != null) questIcon.sprite = quest.icon;

        foreach (Transform child in objectivesParent) Destroy(child.gameObject);

        foreach (var objective in quest.objectives)
            Instantiate(objectivePrefab, objectivesParent).Setup(objective);

        bool isActive = QuestManager.Instance.IsQuestActive(quest.questID);
        bool isCompleted = QuestManager.Instance.IsQuestCompleted(quest.questID);

        if (acceptButton != null && abandonButton != null)
        {
            acceptButton.gameObject.SetActive(!isActive && !isCompleted);
            abandonButton.gameObject.SetActive(isActive);
        }
    }

    void OnAcceptClicked()
    {
        if (selectedQuest == null) return;
        QuestManager.Instance.StartQuest(selectedQuest);
        RefreshQuestLog();
        questDetailsPanel.SetActive(false);
    }

    void OnAbandonClicked()
    {
        if (selectedQuest == null) return;
        QuestManager.Instance.AbandonQuest(selectedQuest);
        RefreshQuestLog();
        questDetailsPanel.SetActive(false);
    }

    void OnQuestStarted(QuestData quest) => RefreshQuestLog();

    void OnQuestCompleted(QuestData quest) => RefreshQuestLog();

    void OnObjectiveUpdated(QuestData quest, QuestObjective objective)
    {
        if (selectedQuest != null && selectedQuest.questID == quest.questID)
            ShowQuestDetails(selectedQuest);

        if (QuestTrackerUI.Instance != null)
            QuestTrackerUI.Instance.UpdateTracker();
    }
}
