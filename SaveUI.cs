using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    [Header("Quest")]
    public string questID;
    public string objectiveID;

    [Header("Visual")]
    public GameObject markerVisual;
    public float floatHeight = 0.5f;
    public float floatSpeed = 1f;

    private Vector3 startPos;
    private bool isVisible = false;
    private System.Action<QuestData> onQuestStarted;
    private System.Action<QuestData> onQuestCompleted;
    private System.Action<QuestData, QuestObjective> onObjectiveCompleted;

    void Start()
    {
        startPos = transform.position;
        onQuestStarted = _ => RefreshVisibility();
        onQuestCompleted = _ => RefreshVisibility();
        onObjectiveCompleted = (_, __) => RefreshVisibility();
        RefreshVisibility();

        if (QuestManager.Instance != null)
            Subscribe(QuestManager.Instance);
        else
            QuestManager.OnReady += OnQuestManagerReady;
    }

    void Update()
    {
        if (!isVisible || markerVisual == null)
            return;

        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnDestroy()
    {
        QuestManager.OnReady -= OnQuestManagerReady;

        if (QuestManager.Instance != null)
            Unsubscribe(QuestManager.Instance);
    }

    void OnQuestManagerReady()
    {
        QuestManager.OnReady -= OnQuestManagerReady;
        Subscribe(QuestManager.Instance);
        RefreshVisibility();
    }

    void Subscribe(QuestManager qm)
    {
        qm.OnQuestStarted += onQuestStarted;
        qm.OnQuestCompleted += onQuestCompleted;
        qm.OnObjectiveCompleted += onObjectiveCompleted;
    }

    void Unsubscribe(QuestManager qm)
    {
        qm.OnQuestStarted -= onQuestStarted;
        qm.OnQuestCompleted -= onQuestCompleted;
        qm.OnObjectiveCompleted -= onObjectiveCompleted;
    }

    void RefreshVisibility()
    {
        if (QuestManager.Instance == null)
            return;

        if (string.IsNullOrEmpty(objectiveID))
        {
            isVisible = QuestManager.Instance.IsQuestActive(questID);
        }
        else
        {
            var quest = QuestManager.Instance.GetActiveQuest(questID);
            isVisible = false;

            if (quest != null)
            {
                var objective = QuestManager.Instance.GetObjective(quest, objectiveID);

                if (objective != null)
                    isVisible = !QuestManager.Instance.GetObjectiveState(questID, objectiveID).isCompleted;
            }
        }

        if (markerVisual != null)
            markerVisual.SetActive(isVisible);
    }
}
