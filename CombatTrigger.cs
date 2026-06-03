using UnityEngine;
using UnityEngine.EventSystems;

public class CombatTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("Enemy")]
    public EnemyData enemy;

    [Header("Quest")]
    public string catalogObjectiveID;

    [Header("Trigger Settings")]
    public bool requirePlayerInRange = false;

    [Header("Visual")]
    public GameObject interactionPrompt;

    private bool hasTriggered = false;
    private bool playerInRange = false;
    private string questID;
    private string objectiveID;
    private int progressAmount = 1;

    void Awake()
    {
        if (!string.IsNullOrEmpty(catalogObjectiveID) &&
            AshenveilQuestTriggerCatalog.TryGet(catalogObjectiveID, out var entry))
        {
            questID = entry.questID;
            objectiveID = entry.objectiveID;
            progressAmount = entry.progressAmount;
        }
    }

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void OnEnable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatVictory += OnVictory;
    }

    void OnDisable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatVictory -= OnVictory;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasTriggered || (requirePlayerInRange && !playerInRange))
            return;

        TriggerCombat();
    }

    public void TriggerCombat()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.inCombat)
            return;

        hasTriggered = true;
        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        CombatManager.Instance.StartCombat(enemy);
        gameObject.SetActive(false);
    }

    void OnVictory(EnemyData defeated)
    {
        if (defeated != enemy || string.IsNullOrEmpty(questID) || QuestManager.Instance == null)
            return;

        QuestManager.Instance.UpdateObjectiveProgress(questID, objectiveID, progressAmount);
    }
}
