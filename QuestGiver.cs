using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour, IPointerClickHandler
{
    [Header("Quests")]
    public List<QuestData> availableQuests;

    [Header("Visual")]
    public GameObject exclamationMark;
    public GameObject questionMark;
    public GameObject goldExclamation;

    void Start()
    {
        if (QuestManager.Instance != null)
            Subscribe();
        else
            QuestManager.OnReady += OnQuestManagerReady;

        UpdateQuestIndicators();
    }

    void OnDestroy()
    {
        QuestManager.OnReady -= OnQuestManagerReady;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestChanged;
            QuestManager.Instance.OnQuestFailed -= OnQuestChanged;
            QuestManager.Instance.OnQuestAbandoned -= OnQuestChanged;
            QuestManager.Instance.OnQuestReadyToComplete -= OnQuestChanged;
            QuestManager.Instance.OnObjectiveCompleted -= OnObjectiveChanged;
        }
    }

    void OnQuestManagerReady()
    {
        QuestManager.OnReady -= OnQuestManagerReady;
        Subscribe();
        UpdateQuestIndicators();
    }

    void Subscribe()
    {
        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestStarted += OnQuestChanged;
        QuestManager.Instance.OnQuestCompleted += OnQuestChanged;
        QuestManager.Instance.OnQuestFailed += OnQuestChanged;
        QuestManager.Instance.OnQuestAbandoned += OnQuestChanged;
        QuestManager.Instance.OnQuestReadyToComplete += OnQuestChanged;
        QuestManager.Instance.OnObjectiveCompleted += OnObjectiveChanged;
    }

    void OnQuestChanged(QuestData _) => UpdateQuestIndicators();

    void OnObjectiveChanged(QuestData _, QuestObjective __) => UpdateQuestIndicators();

    public void OnPointerClick(PointerEventData eventData) => Interact();

    void UpdateQuestIndicators()
    {
        if (QuestManager.Instance == null || availableQuests == null)
            return;

        bool hasNewQuest = false;
        bool hasActiveQuest = false;
        bool hasCompleteQuest = false;

        foreach (var quest in availableQuests)
        {
            if (quest == null)
                continue;

            if (QuestManager.Instance.CanStartQuest(quest))
            {
                hasNewQuest = true;
            }
            else if (QuestManager.Instance.IsQuestActive(quest.questID))
            {
                if (QuestManager.Instance.AreRequiredObjectivesComplete(quest))
                    hasCompleteQuest = true;
                else
                    hasActiveQuest = true;
            }
        }

        if (exclamationMark != null)
            exclamationMark.SetActive(hasNewQuest);

        if (questionMark != null)
            questionMark.SetActive(hasActiveQuest && !hasCompleteQuest);

        if (goldExclamation != null)
            goldExclamation.SetActive(hasCompleteQuest);
    }

    void Interact()
    {
        if (QuestManager.Instance == null || availableQuests == null)
            return;

        foreach (var quest in availableQuests)
        {
            if (quest == null || !QuestManager.Instance.IsQuestActive(quest.questID))
                continue;

            if (QuestManager.Instance.AreRequiredObjectivesComplete(quest))
            {
                StartDialogueOrRun(quest.completionDialogue, () => QuestManager.Instance.CompleteQuest(quest));
                return;
            }

            if (quest.progressDialogue != null)
            {
                StartDialogueOrRun(quest.progressDialogue, null);
                return;
            }
        }

        foreach (var quest in availableQuests)
        {
            if (quest == null)
                continue;

            if (QuestManager.Instance.CanStartQuest(quest))
            {
                StartDialogueOrRun(quest.startDialogue, () => QuestManager.Instance.StartQuest(quest));
                return;
            }
        }
    }

    void StartDialogueOrRun(DialogueNode dialogue, System.Action onComplete)
    {
        if (dialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, onComplete);
            return;
        }

        onComplete?.Invoke();
    }
}
