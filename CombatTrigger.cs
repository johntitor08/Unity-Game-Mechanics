using UnityEngine;
using UnityEngine.EventSystems;

public class CombatTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("Enemy")]
    public EnemyData enemy;

    [Header("Trigger Settings")]
    public bool oneTimeOnly = true;
    public bool requirePlayerInRange = false;

    [Header("Visual")]
    public GameObject interactionPrompt;

    private bool hasTriggered = false;
    private bool playerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
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
        if (hasTriggered)
            return;

        // Ýstersen sadece yakýndayken týklanabilsin
        if (requirePlayerInRange && !playerInRange)
            return;

        TriggerCombat();
    }

    public void TriggerCombat()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.inCombat)
            return;

        CombatManager.Instance.StartCombat(enemy);
        hasTriggered = true;

        if (oneTimeOnly)
            gameObject.SetActive(false);
    }
}
