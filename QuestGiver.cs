using UnityEngine;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
{
    [Header("Quests")]
    public List<QuestData> availableQuests;

    [Header("Interaction")]
    public GameObject exclamationMark; // New quest available
    public GameObject questionMark;    // Quest in progress
    public GameObject goldExclamation; // Quest complete
    public float interactionRange = 2f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Visual")]
    public GameObject interactionPrompt;

    private bool playerInRange = false;
    private QuestData activePlayerQuest;

    void Start()
    {
        UpdateQuestIndicators();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        UpdateQuestIndicators();

        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
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

    void UpdateQuestIndicators()
    {
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
                activePlayerQuest = quest;

                // Check if quest is completable
                bool allComplete = true;
                foreach (var obj in quest.objectives)
                {
                    if (!obj.isOptional && !obj.isCompleted)
                    {
                        allComplete = false;
                        break;
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
        // Check for completable quests
        if (activePlayerQuest != null)
        {
            bool canComplete = true;
            foreach (var obj in activePlayerQuest.objectives)
            {
                if (!obj.isOptional && !obj.isCompleted)
                {
                    canComplete = false;
                    break;
                }
            }

            if (canComplete)
            {
                QuestManager.Instance.CompleteQuest(activePlayerQuest);
                activePlayerQuest = null;
                return;
            }
            else if (activePlayerQuest.progressDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(activePlayerQuest.progressDialogue);
                return;
            }
        }

        // Offer new quests
        foreach (var quest in availableQuests)
        {
            if (QuestManager.Instance.CanStartQuest(quest))
            {
                if (quest.startDialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(quest.startDialogue,
                        () => QuestManager.Instance.StartQuest(quest));
                }
                else
                {
                    QuestManager.Instance.StartQuest(quest);
                }
                return;
            }
        }
    }
}
