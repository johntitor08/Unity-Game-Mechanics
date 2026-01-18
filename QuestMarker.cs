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

    void Start()
    {
        startPos = transform.position;
        UpdateVisibility();
    }

    void Update()
    {
        UpdateVisibility();
        AnimateMarker();
    }

    void UpdateVisibility()
    {
        if (QuestManager.Instance == null) return;

        var quest = QuestManager.Instance.GetActiveQuest(questID);
        bool shouldShow = false;

        if (quest != null)
        {
            var objective = QuestManager.Instance.GetObjective(quest, objectiveID);
            shouldShow = objective != null && !objective.isCompleted;
        }

        if (markerVisual != null)
            markerVisual.SetActive(shouldShow);
    }

    void AnimateMarker()
    {
        if (markerVisual == null || !markerVisual.activeSelf) return;

        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
