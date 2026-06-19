using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuestUI : HotkeyPanelUI
{
    public static QuestUI Instance;
    private QuestData selectedQuest;
    private readonly List<QuestSlotUI> questSlots = new();
    private bool isSubscribed = false;

    [Header("Panels")]
    public GameObject questPanel;
    public GameObject questLogPanel;
    public GameObject questDetailsPanel;

    [Header("Quest Log")]
    public Transform activeQuestsContainer;
    public Transform availableQuestsContainer;
    public Transform completedQuestsContainer;
    public QuestSlotUI questSlotPrefab;

    [Header("Quest Log ScrollRects")]
    public ScrollRect activeQuestsScrollRect;
    public ScrollRect availableQuestsScrollRect;
    public ScrollRect completedQuestsScrollRect;
    public float maxLogPanelWidth = 750f;

    [Header("Quest Details")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questTypeText;
    public Image questIcon;
    public Transform objectivesContainer;
    public QuestObjectiveUI objectivePrefab;
    public Transform rewardsContainer;
    public RewardItemUI rewardItemPrefab;
    public Button acceptButton;
    public Button abandonButton;
    public Button trackButton;

    [Header("Quest Details ScrollRects")]
    public ScrollRect objectivesScrollRect;
    public ScrollRect rewardsScrollRect;
    public float maxDetailsHeight = 300f;

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
        if (questPanel != null)
            questPanel.SetActive(false);

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
            QuestManager.Instance.OnQuestFailed -= OnQuestFailed;
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
            QuestManager.Instance.OnQuestFailed += OnQuestFailed;
            QuestManager.Instance.OnQuestAbandoned += OnQuestAbandoned;
            QuestManager.Instance.OnObjectiveUpdated += OnObjectiveUpdated;
            QuestManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;
            isSubscribed = true;
            RefreshQuestLog();
        }
    }

    void Update()
    {
        if (PanelInputBlocked())
            return;

        if (Input.GetKeyDown(toggleKey) && questPanel != null)
        {
            bool nowActive = !questPanel.activeSelf;
            questPanel.SetActive(nowActive);

            if (questLogPanel != null)
                questLogPanel.SetActive(nowActive);

            if (nowActive)
                RefreshQuestLog();
        }
    }

    public void RefreshQuestLog()
    {
        if (QuestManager.Instance == null || questSlotPrefab == null)
            return;

        foreach (var slot in questSlots)
            if (slot != null)
                slot.gameObject.SetActive(false);

        int index = 0;
        index = RefreshSlots(QuestManager.Instance.GetActiveQuests(), activeQuestsContainer, false, index);
        index = RefreshSlots(QuestManager.Instance.GetAvailableQuests(), availableQuestsContainer, false, index);
        RefreshSlots(QuestManager.Instance.GetCompletedQuests(), completedQuestsContainer, true, index);
        StartCoroutine(UpdateLogScrollsNextFrame());

        if (selectedQuest != null && questDetailsPanel != null && questDetailsPanel.activeSelf)
            ShowQuestDetails(selectedQuest);
    }

    private int RefreshSlots(IEnumerable<QuestData> quests, Transform parent, bool isCompleted, int startIndex)
    {
        if (parent == null || quests == null)
            return startIndex;

        int index = startIndex;

        foreach (var quest in quests)
        {
            QuestSlotUI slot;

            if (index < questSlots.Count)
            {
                slot = questSlots[index];
                slot.transform.SetParent(parent, false);
            }
            else
            {
                slot = Instantiate(questSlotPrefab, parent);
                questSlots.Add(slot);
            }

            slot.gameObject.SetActive(true);
            slot.Setup(quest, isCompleted);
            index++;
        }

        return index;
    }

    System.Collections.IEnumerator UpdateLogScrollsNextFrame()
    {
        yield return null;
        UpdateHorizontalScrollRect(activeQuestsContainer, activeQuestsScrollRect, maxLogPanelWidth);
        UpdateHorizontalScrollRect(availableQuestsContainer, availableQuestsScrollRect, maxLogPanelWidth);
        UpdateHorizontalScrollRect(completedQuestsContainer, completedQuestsScrollRect, maxLogPanelWidth);
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

        if (objectivesContainer != null && objectivePrefab != null)
        {
            ClearContainer(objectivesContainer);

            foreach (var objective in GetObjectives(quest))
            {
                var objUI = Instantiate(objectivePrefab, objectivesContainer);
                var state = qm.GetObjectiveState(quest.questID, objective.objectiveID);
                objUI.Setup(objective, state);
            }
        }

        PopulateRewards(quest);
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

        StartCoroutine(UpdateDetailsScrollsNextFrame());
    }

    System.Collections.IEnumerator UpdateDetailsScrollsNextFrame()
    {
        yield return null;
        UpdateVerticalScrollRect(objectivesContainer, objectivesScrollRect, maxDetailsHeight);
        UpdateVerticalScrollRect(rewardsContainer, rewardsScrollRect, maxDetailsHeight);
    }

    void UpdateHorizontalScrollRect(Transform contentParent, ScrollRect sr, float threshold)
    {
        if (sr == null || contentParent == null)
            return;

        float contentWidth = contentParent is RectTransform rt ? rt.rect.width : 0f;
        bool needsScroll = contentWidth > threshold;
        sr.vertical = false;
        sr.horizontal = needsScroll;
        sr.horizontalScrollbar = needsScroll ? sr.horizontalScrollbar : null;
        sr.horizontalNormalizedPosition = 0f;
    }

    void UpdateVerticalScrollRect(Transform contentParent, ScrollRect sr, float threshold)
    {
        if (sr == null || contentParent == null)
            return;

        float contentHeight = contentParent is RectTransform rt ? rt.rect.height : 0f;
        bool needsScroll = contentHeight > threshold;
        sr.horizontal = false;
        sr.vertical = needsScroll;
        sr.verticalScrollbar = needsScroll ? sr.verticalScrollbar : null;
        sr.verticalNormalizedPosition = 1f;
    }

    void OnAcceptClicked()
    {
        if (selectedQuest == null || QuestManager.Instance == null)
            return;

        QuestManager.Instance.StartQuest(selectedQuest);
        selectedQuest = null;
        RefreshQuestLog();

        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);
    }

    void OnAbandonClicked()
    {
        if (selectedQuest == null || QuestManager.Instance == null)
            return;

        QuestManager.Instance.AbandonQuest(selectedQuest);
        selectedQuest = null;
        RefreshQuestLog();

        if (questDetailsPanel != null)
            questDetailsPanel.SetActive(false);
    }

    void OnTrackClicked()
    {
        if (selectedQuest == null || QuestManager.Instance == null)
            return;

        QuestManager.Instance.ToggleTracking(selectedQuest);
        ShowQuestDetails(selectedQuest);
    }

    void OnQuestStarted(QuestData quest) => RefreshQuestLog();

    void OnQuestCompleted(QuestData quest) => RefreshQuestLog();

    void OnQuestFailed(QuestData quest) => RefreshQuestLog();

    void OnQuestAbandoned(QuestData quest) => RefreshQuestLog();

    void OnObjectiveUpdated(QuestData quest, QuestObjective objective) => RefreshDetails(quest);

    void OnObjectiveCompleted(QuestData quest, QuestObjective objective) => RefreshDetails(quest);

    void RefreshDetails(QuestData quest)
    {
        if (selectedQuest != null && selectedQuest.questID == quest.questID)
            ShowQuestDetails(selectedQuest);
    }

    public void CloseQuestPanel()
    {
        UIPanelAnimator.Hide(questPanel);
        UIPanelAnimator.Hide(questDetailsPanel);
    }

    void PopulateRewards(QuestData quest)
    {
        if (rewardsContainer == null || rewardItemPrefab == null)
            return;

        ClearContainer(rewardsContainer);

        if (quest.experienceReward > 0)
            SpawnRewardItem("Experience", quest.experienceReward.ToString(), null);

        if (quest.currencyRewards != null)
        {
            foreach (var reward in quest.currencyRewards)
            {
                var currencyInfo = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetCurrencyInfo(reward.type) : null;
                SpawnRewardItem(reward.type.ToString(), reward.amount.ToString(), currencyInfo?.icon);
            }
        }

        if (quest.itemRewards != null)
        {
            for (int i = 0; i < quest.itemRewards.Length; i++)
            {
                var item = quest.itemRewards[i];

                if (item == null)
                    continue;

                int qty = quest.itemRewardQuantities != null && i < quest.itemRewardQuantities.Length ? quest.itemRewardQuantities[i] : 1;
                SpawnRewardItem(item.itemName, $"x{qty}", item.icon);
            }
        }

        if (quest.optionalRewards != null)
        {
            var qm = QuestManager.Instance;
            bool canSelectOptionalReward = qm != null && qm.IsQuestActive(quest.questID);
            var selectedOptionalReward = qm != null ? qm.GetSelectedOptionalReward(quest.questID) : null;

            foreach (var item in quest.optionalRewards)
            {
                if (item == null)
                    continue;

                var reward = item;
                bool isSelected = selectedOptionalReward == reward;
                string value = canSelectOptionalReward ? isSelected ? "Selected" : "Choose" : "x1";
                System.Action onClick = null;

                if (canSelectOptionalReward)
                {
                    onClick = () =>
                    {
                        if (QuestManager.Instance != null && QuestManager.Instance.SelectOptionalReward(quest, reward))
                            ShowQuestDetails(quest);
                    };
                }

                SpawnRewardItem($"{reward.itemName} (Optional)", value, reward.icon, onClick, isSelected);
            }
        }
    }

    void SpawnRewardItem(string label, string value, Sprite icon, System.Action onClicked = null, bool isSelected = false)
    {
        var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
        rewardUI.Setup(label, value, icon, onClicked, isSelected);
    }

    void ClearContainer(Transform container)
    {
        if (container == null)
            return;

        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    IEnumerable<QuestObjective> GetObjectives(QuestData quest)
    {
        if (quest == null || quest.objectives == null)
            yield break;

        foreach (var objective in quest.objectives)
            if (objective != null)
                yield return objective;
    }
}
