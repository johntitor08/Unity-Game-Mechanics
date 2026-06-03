using UnityEngine;
using UnityEngine.EventSystems;

public class WorldItem : MonoBehaviour, IPointerClickHandler
{
    public ItemData data;
    public int quantity = 1;

    [Header("Quest")]
    public string catalogObjectiveID;
    private string questObjectiveTag;

    void Awake()
    {
        if (!string.IsNullOrEmpty(catalogObjectiveID) &&
            AshenveilQuestTriggerCatalog.TryGet(catalogObjectiveID, out var entry))
            questObjectiveTag = entry.tag;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data == null)
        {
            Debug.LogWarning($"[WorldItem] {name} has no ItemData assigned.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[WorldItem] InventoryManager not available.");
            return;
        }

        InventoryManager.Instance.AddItem(data, quantity);

        if (!string.IsNullOrEmpty(questObjectiveTag) && QuestManager.Instance != null)
            QuestManager.Instance.NotifyObjectInteracted(questObjectiveTag, quantity);

        if (LootNotificationUI.Instance != null)
            LootNotificationUI.Instance.ShowLoot(data);

        Destroy(gameObject);
    }
}
