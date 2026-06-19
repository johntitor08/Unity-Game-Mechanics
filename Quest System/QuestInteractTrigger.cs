using UnityEngine;

public class QuestInteractTrigger : QuestPlayerTriggerBase
{
    [Header("Interact")]
    public string interactTag;

    [Min(1)]
    public int progressAmount = 1;

    protected override void OnFire()
    {
        string tag = string.IsNullOrEmpty(interactTag) ? gameObject.name : interactTag;
        QuestManager.Instance.NotifyObjectInteracted(tag, progressAmount);
    }
}
