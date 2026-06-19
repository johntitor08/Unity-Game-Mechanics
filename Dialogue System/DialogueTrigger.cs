using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueNode startNode;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;
    public bool requireInteraction = true;
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 2f;

    [Header("Visual")]
    public GameObject interactionPrompt;
    public Sprite npcPortrait;

    private bool isExhausted = false;
    private bool playerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (requireInteraction && playerInRange && !isExhausted)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                TriggerDialogue();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactionPrompt != null && requireInteraction)
            {
                interactionPrompt.SetActive(true);
            }
            else if (!requireInteraction && !isExhausted)
            {
                TriggerDialogue();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    void TriggerDialogue()
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.IsInDialogue())
            return;

        DialogueManager.Instance.StartDialogue(startNode);

        if (triggerOnce)
        {
            isExhausted = true;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}
