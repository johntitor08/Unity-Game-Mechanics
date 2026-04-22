using UnityEngine;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
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
    private readonly List<QuestData> activePlayerQuests = new();

    void Start()
    {
        UpdateQuestIndicators();
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += _ => UpdateQuestIndicators();
            QuestManager.Instance.OnQuestCompleted += _ => UpdateQuestIndicators();
            QuestManager.Instance.OnObjectiveCompleted += (_, __) => UpdateQuestIndicators();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
            Interact();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    void UpdateQuestIndicators()
    {
        activePlayerQuests.Clear();

        bool hasNewQuest = false;
        bool hasActiveQuest = false;
        bool hasCompleteQuest = false;

        foreach (var quest in availableQuests)
        {
            if (QuestManager.Instance.CanStartQuest(quest))
            {
                hasNewQuest = true;
            }
            else if (QuestManager.Instance.IsQuestActive(quest.questID))
            {
                activePlayerQuests.Add(quest);
                bool allComplete = true;

                foreach (var obj in quest.objectives)
                {
                    if (!obj.isOptional)
                    {
                        var state = QuestManager.Instance.GetObjectiveState(quest.questID, obj.objectiveID);

                        if (!state.isCompleted)
                        {
                            allComplete = false;
                            break;
                        }
                    }
                }

                if (allComplete)
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
        foreach (var quest in activePlayerQuests)
        {
            bool allComplete = true;

            foreach (var obj in quest.objectives)
            {
                if (!obj.isOptional)
                {
                    var state = QuestManager.Instance.GetObjectiveState(quest.questID, obj.objectiveID);

                    if (!state.isCompleted)
                    {
                        allComplete = false;
                        break;
                    }
                }
            }

            if (allComplete)
            {
                QuestManager.Instance.CompleteQuest(quest);
                return;
            }
            else if (quest.progressDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(quest.progressDialogue);
                return;
            }
        }

        foreach (var quest in availableQuests)
        {
            if (QuestManager.Instance.CanStartQuest(quest))
            {
                if (quest.startDialogue != null)
                    DialogueManager.Instance.StartDialogue(quest.startDialogue, () => QuestManager.Instance.StartQuest(quest));
                else
                    QuestManager.Instance.StartQuest(quest);

                return;
            }
        }
    }
}
