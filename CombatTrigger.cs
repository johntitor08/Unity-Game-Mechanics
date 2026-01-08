using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    [Header("Enemy")]
    public EnemyData enemy;

    [Header("Trigger Settings")]
    public bool oneTimeOnly = true;
    public bool requireInteraction = false;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Visual")]
    public GameObject interactionPrompt;

    private bool hasTriggered = false;
    private bool playerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (requireInteraction && playerInRange && !hasTriggered)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                TriggerCombat();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (requireInteraction)
            {
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(true);
            }
            else if (!hasTriggered)
            {
                TriggerCombat();
            }
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

    void TriggerCombat()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.inCombat)
            return;

        CombatManager.Instance.StartCombat(enemy);
        hasTriggered = true;

        if (oneTimeOnly)
        {
            gameObject.SetActive(false);
        }
    }
}
