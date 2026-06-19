using UnityEngine;
using UnityEngine.EventSystems;

public abstract class QuestPlayerTriggerBase : MonoBehaviour, IPointerClickHandler
{
    [Header("Quest Filter")]
    public string questID;

    [Header("Trigger")]
    public bool triggerOnce = true;
    public bool requirePlayerInRange = false;
    public bool allowPointerClick = true;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Visual")]
    public GameObject interactionPrompt;

    private bool hasFired;
    private bool playerInRange;
    public bool requireQuestActive = true;

    protected virtual void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    protected virtual void Update()
    {
        if (!CanAcceptInput() || !playerInRange)
            return;

        if (Input.GetKeyDown(interactionKey))
            TryFire();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!allowPointerClick)
            return;

        TryFire();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void TryFire()
    {
        if (!CanAcceptInput() || (requirePlayerInRange && !playerInRange) || !PassesQuestFilter() || (hasFired && triggerOnce) || !OnBeforeFire())
            return;

        OnFire();
        CommitFire();
    }

    protected void CommitFire()
    {
        hasFired = true;

        if (triggerOnce && interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    protected virtual bool OnBeforeFire() => true;

    protected abstract void OnFire();

    protected virtual bool CanAcceptInput() => QuestManager.Instance != null;

    protected virtual bool PassesQuestFilter()
    {
        if (!requireQuestActive || string.IsNullOrEmpty(questID))
            return true;

        return QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(questID);
    }

    public void ResetTrigger()
    {
        hasFired = false;

        if (interactionPrompt != null && playerInRange)
            interactionPrompt.SetActive(true);
    }
}
