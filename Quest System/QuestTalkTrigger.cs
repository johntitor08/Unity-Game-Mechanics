using UnityEngine;

public class QuestTalkTrigger : QuestPlayerTriggerBase
{
    [Header("Talk")]
    public string npcTag;

    [Min(1)]
    public int progressAmount = 1;

    [Header("Dialogue (Optional)")]
    public DialogueNode dialogue;
    public bool waitForDialogueEnd = true;

    protected override bool OnBeforeFire()
    {
        if (dialogue == null || DialogueManager.Instance == null)
            return true;

        if (DialogueManager.Instance.IsInDialogue())
            return false;

        if (waitForDialogueEnd)
        {
            DialogueManager.Instance.StartDialogue(dialogue, OnDialogueFinished);
            return false;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
        return true;
    }

    protected override void OnFire() => ApplyTalkProgress();

    void OnDialogueFinished()
    {
        ApplyTalkProgress();
        CommitFire();
    }

    void ApplyTalkProgress()
    {
        string tag = string.IsNullOrEmpty(npcTag) ? gameObject.name : npcTag;
        QuestManager.Instance.NotifyTalkToNPC(tag, progressAmount);
    }
}
