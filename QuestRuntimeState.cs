using System.Collections.Generic;

[System.Serializable]
public class QuestRuntimeState
{
    public string questID;
    public List<string> objectiveIDs = new();
    public List<ObjectiveRuntimeState> objectiveStates = new();
    [System.NonSerialized] private Dictionary<string, ObjectiveRuntimeState> objectives;

    public QuestRuntimeState(string questID)
    {
        this.questID = questID;
    }

    public void RebuildLookup()
    {
        objectives = new Dictionary<string, ObjectiveRuntimeState>();

        for (int i = 0; i < objectiveIDs.Count && i < objectiveStates.Count; i++)
            objectives[objectiveIDs[i]] = objectiveStates[i];
    }

    void EnsureLookup()
    {
        if (objectives == null)
            RebuildLookup();
    }

    public ObjectiveRuntimeState GetObjective(string objectiveID)
    {
        EnsureLookup();

        if (!objectives.TryGetValue(objectiveID, out var state))
        {
            state = new ObjectiveRuntimeState(objectiveID);
            objectives[objectiveID] = state;
            objectiveIDs.Add(objectiveID);
            objectiveStates.Add(state);
        }

        return state;
    }
}

[System.Serializable]
public class ObjectiveRuntimeState
{
    public string objectiveID;
    public int currentProgress;
    public bool isCompleted;

    public ObjectiveRuntimeState(string objectiveID)
    {
        this.objectiveID = objectiveID;
        currentProgress = 0;
        isCompleted = false;
    }
}

[System.Serializable]
public class QuestSaveData
{
    public List<string> activeQuestIDs = new();
    public List<string> completedQuestIDs = new();
    public List<QuestRuntimeState> runtimeStates = new();
    public List<string> trackedQuestIDs = new();
    public List<float> questTimerValues = new();
    public List<string> questTimerKeys = new();
}
