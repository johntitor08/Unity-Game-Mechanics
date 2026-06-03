using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class QuestUIHoverBridge : MonoBehaviour, IPointerClickHandler
{
    public enum TriggerKind
    {
        Talk,
        Interact,
        Location,
        DirectProgress
    }

    UIHoverRegion hoverRegion;
    public bool requireQuestActive = true;

    [Header("Quest")]
    public TriggerKind kind = TriggerKind.Interact;
    public string questID;
    public new string tag;
    public string objectiveID;

    [Min(1)]
    public int progressAmount = 1;

    [Header("Talk · Dialogue")]
    public DialogueNode dialogue;
    public bool waitForDialogueEnd = true;

    [Header("Catalog")]
    public string catalogObjectiveID;

    void Awake()
    {
        ApplyCatalogEntry();

        if (TryGetComponent(out hoverRegion))
            hoverRegion.OnRegionClicked += OnRegionClicked;
    }

    void OnDestroy()
    {
        if (hoverRegion != null)
            hoverRegion.OnRegionClicked -= OnRegionClicked;
    }

    void OnValidate()
    {
        if (!string.IsNullOrEmpty(catalogObjectiveID))
            ApplyCatalogEntry();
    }

    public void OnRegionClicked() => TryFire();

    public void OnPointerClick(PointerEventData eventData) => TryFire();

    public void TryFire()
    {
        if (QuestManager.Instance == null || (requireQuestActive && !string.IsNullOrEmpty(questID) && !QuestManager.Instance.IsQuestActive(questID)))
            return;

        if (dialogue != null && DialogueManager.Instance != null && kind == TriggerKind.Talk)
        {
            if (DialogueManager.Instance.IsInDialogue())
                return;

            if (waitForDialogueEnd)
            {
                DialogueManager.Instance.StartDialogue(dialogue, ApplyProgress);
                return;
            }

            DialogueManager.Instance.StartDialogue(dialogue);
        }

        ApplyProgress();
    }

    void ApplyProgress()
    {
        if (QuestManager.Instance == null)
            return;

        string resolvedTag = ResolveTag();

        switch (kind)
        {
            case TriggerKind.Talk:
                QuestManager.Instance.NotifyTalkToNPC(resolvedTag, progressAmount);
                break;

            case TriggerKind.Interact:
                QuestManager.Instance.NotifyObjectInteracted(resolvedTag, progressAmount);
                break;

            case TriggerKind.Location:
                QuestManager.Instance.NotifyLocationReached(resolvedTag, progressAmount);
                break;

            case TriggerKind.DirectProgress:
                if (!string.IsNullOrEmpty(questID) && !string.IsNullOrEmpty(objectiveID))
                    QuestManager.Instance.UpdateObjectiveProgress(questID, objectiveID, progressAmount);

                break;
        }
    }

    string ResolveTag()
    {
        if (!string.IsNullOrEmpty(tag))
            return tag;

        if (!string.IsNullOrEmpty(objectiveID))
            return objectiveID;

        return gameObject.name;
    }

    public void ApplyCatalogEntry()
    {
        if (string.IsNullOrEmpty(catalogObjectiveID) || !AshenveilQuestTriggerCatalog.TryGet(catalogObjectiveID, out var entry))
            return;

        questID = entry.questID;
        objectiveID = entry.objectiveID;
        tag = string.IsNullOrEmpty(entry.tag) ? entry.objectiveID : entry.tag;
        kind = entry.recommendedKind;
        progressAmount = entry.progressAmount;
    }
}
