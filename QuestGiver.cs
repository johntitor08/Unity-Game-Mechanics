using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour, IPointerClickHandler
{
    [Header("Quests")]
    public List<QuestData> availableQuests;

    [Header("Interaction")]
    public GameObject exclamationMark;
    public GameObject questionMark;
    public GameObject goldExclamation;
    public float interactionRange = 2f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Visual")]
    public GameObject interactionPrompt;

    private bool playerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

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

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
            Interact();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playerInRange)
            Interact();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    bool AreAllObjectivesComplete(QuestData quest)
    {
        return QuestManager.Instance != null && QuestManager.Instance.AreRequiredObjectivesComplete(quest);
    }

    void UpdateQuestIndicators()
    {
        if (QuestManager.Instance == null)
            return;

        bool hasNewQuest = false;
        bool hasActiveQuest = false;
        bool hasCompleteQuest = false;

        if (availableQuests == null)
            return;

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
                if (AreAllObjectivesComplete(quest))
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

            if (AreAllObjectivesComplete(quest))
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
