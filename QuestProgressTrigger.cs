using UnityEngine;

public class QuestProgressTrigger : QuestPlayerTriggerBase
{
    [Header("Direct Progress")]
    public string objectiveID;

    [Min(1)]
    public int progressAmount = 1;

    protected override bool PassesQuestFilter()
    {
        if (string.IsNullOrEmpty(questID) || string.IsNullOrEmpty(objectiveID))
            return false;

        return base.PassesQuestFilter();
    }

    protected override void OnFire()
    {
        QuestManager.Instance.UpdateObjectiveProgress(questID, objectiveID, progressAmount);
    }
}
