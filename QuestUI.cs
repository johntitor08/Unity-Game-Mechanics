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
    public KeyCode toggleKey = KeyCode.Q;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (questLogPanel != null)
            questLogPanel.SetActive(false);

        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptClicked);
            acceptButton.gameObject.SetActive(false);
        }

        if (abandonButton != null)
        {
            abandonButton.onClick.AddListener(OnAbandonClicked);
            abandonButton.gameObject.SetActive(false);
        }

        if (trackButton != null)
        {
            trackButton.onClick.AddListener(OnTrackClicked);
            trackButton.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnEnable()
    {
        QuestManager.OnReady += TrySubscribe;
        TrySubscribe();
    }

    void OnDisable()
    {
        if (isSubscribed && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnQuestAbandoned -= OnQuestAbandoned;
            QuestManager.Instance.OnObjectiveUpdated -= OnObjectiveUpdated;
            QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveCompleted;
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
            QuestManager.Instance.OnQuestAbandoned += OnQuestAbandoned;
            QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;
            QuestManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;
            isSubscribed = true;
            RefreshQuestLog();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (questLogPanel != null)
            {
                bool nowActive = !questLogPanel.activeSelf;
                questLogPanel.SetActive(nowActive);

                if (nowActive)
                    RefreshQuestLog();
            }
        }
    }

    void RefreshQuestLog()
    {
        if (QuestManager.Instance == null)
            return;

        if (questSlotPrefab == null)
        {
            Debug.LogError("QuestUI: questSlotPrefab atanmamış.");
            return;
        }

        foreach (var slot in questSlots)
            if (slot != null)
                Destroy(slot.gameObject);

        questSlots.Clear();

        SpawnSlots(QuestManager.Instance.GetActiveQuests(), activeQuestsParent, isCompleted: false);
        SpawnSlots(QuestManager.Instance.GetAvailableQuests(), availableQuestsParent, isCompleted: false);
        SpawnSlots(QuestManager.Instance.GetCompletedQuests(), completedQuestsParent, isCompleted: true);
    }

    private void SpawnSlots(IEnumerable<QuestData> quests, Transform parent, bool isCompleted)
    {
        if (parent == null || quests == null)
            return;

        foreach (var quest in quests)
        {
            var slot = Instantiate(questSlotPrefab, parent);
            slot.Setup(quest, isCompleted);
            questSlots.Add(slot);
        }
    }

    public void ShowQuestDetails(QuestData quest)
    {
        var qm = QuestManager.Instance;

        if (qm == null || quest == null)
            return;

        selectedQuest = quest;

        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(true);

        if (questTitleText != null)
            questTitleText.text = quest.questName;

        if (questDescriptionText != null)
            questDescriptionText.text = quest.description;

        if (questTypeText != null)
            questTypeText.text = $"{quest.questType} - {quest.difficulty}";

        if (questIcon != null)
            questIcon.sprite = quest.icon;

        if (objectivesParent != null && objectivePrefab != null)
        {
            foreach (Transform child in objectivesParent)
                Destroy(child.gameObject);

            foreach (var objective in quest.objectives)
            {
                var objUI = Instantiate(objectivePrefab, objectivesParent);
                var state = qm.GetObjectiveState(quest.questID, objective.objectiveID);
                objUI.Setup(objective, state);
            }
        }

        bool isActive = qm.IsQuestActive(quest.questID);
        bool isCompleted = qm.IsQuestCompleted(quest.questID);
        bool isTracked = QuestTrackerUI.Instance != null && QuestTrackerUI.Instance.IsTracked(quest.questID);

        if (acceptButton != null && abandonButton != null)
        {
            acceptButton.gameObject.SetActive(!isActive && !isCompleted);
            abandonButton.gameObject.SetActive(isActive);
        }

        if (trackButton != null)
        {
            trackButton.gameObject.SetActive(isActive);
            var label = trackButton.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = isTracked ? "Untrack" : "Track";
        }
    }

    void OnAcceptClicked()
    {
        if (selectedQuest == null)
            return;

        QuestManager.Instance.StartQuest(selectedQuest);
        RefreshQuestLog();

        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);
    }

    void OnAbandonClicked()
    {
        if (selectedQuest == null)
            return;

        QuestManager.Instance.AbandonQuest(selectedQuest);

        selectedQuest = null;
        RefreshQuestLog();

        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);
    }

    void OnTrackClicked()
    {
        if (selectedQuest == null)
            return;

        QuestManager.Instance.ToggleTracking(selectedQuest);
        ShowQuestDetails(selectedQuest);
    }

    void OnQuestStarted(QuestData quest) => RefreshQuestLog();

    void OnQuestCompleted(QuestData quest) => RefreshQuestLog();

    void OnQuestAbandoned(QuestData quest) => RefreshQuestLog();

    void OnObjectiveUpdated(QuestData quest, QuestObjective objective) => RefreshDetails(quest);

    void OnObjectiveCompleted(QuestData quest, QuestObjective objective) => RefreshDetails(quest);

    void RefreshDetails(QuestData quest)
    {
        if (selectedQuest != null && selectedQuest.questID == quest.questID)
            ShowQuestDetails(selectedQuest);
    }
}
