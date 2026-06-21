using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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
    public bool triggerOnce = true;
    bool hasFired;

    [Header("Quest")]
    public TriggerKind kind = TriggerKind.Interact;
    public string questID;
    [FormerlySerializedAs("tag")]
    public string questTag;
    public string objectiveID;

    [Min(1)]
    public int progressAmount = 1;

    [Header("Talk · Dialogue")]
    public DialogueNode dialogue;
    public bool waitForDialogueEnd = true;

    [Header("Catalog")]
    public string catalogObjectiveID;

    [Header("Location")]
    public string backgroundName;

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
        if (QuestManager.Instance == null || (hasFired && triggerOnce) || (requireQuestActive && !string.IsNullOrEmpty(questID) && !QuestManager.Instance.IsQuestActive(questID)))
            return;

        if (!string.IsNullOrEmpty(backgroundName) && SceneEvent.Instance != null)
            SceneEvent.Instance.ShowQuestLocation(backgroundName);

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
                hasFired = true;
                break;

            case TriggerKind.Interact:
                QuestManager.Instance.NotifyObjectInteracted(resolvedTag, progressAmount);
                hasFired = true;
                break;

            case TriggerKind.Location:
                QuestManager.Instance.NotifyLocationReached(resolvedTag, progressAmount);
                hasFired = true;
                break;

            case TriggerKind.DirectProgress:
                if (!string.IsNullOrEmpty(questID) && !string.IsNullOrEmpty(objectiveID))
                {
                    QuestManager.Instance.UpdateObjectiveProgress(questID, objectiveID, progressAmount);
                    hasFired = true;
                }

                break;
        }
    }

    string ResolveTag()
    {
        if (!string.IsNullOrEmpty(questTag))
            return questTag;

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
        questTag = string.IsNullOrEmpty(entry.tag) ? entry.objectiveID : entry.tag;
        kind = entry.recommendedKind;
        progressAmount = entry.progressAmount;
        backgroundName = AshenveilQuestTriggerCatalog.BackgroundFor(entry.suggestedSceneObjectName);
    }
}
